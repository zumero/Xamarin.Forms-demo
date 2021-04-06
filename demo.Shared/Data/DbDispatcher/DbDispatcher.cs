using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sample.DbDispatcher
{
    // These interfaces are here in case you need to create your own DbImplementation for 
    // a SQLite data access layer other than SQLite-Net-Pcl. Like Entity Framework Core, 
    // for example.
    public interface IDatabaseHandle : IDisposable
    {
        void CloseConnection();
        void ExecuteSqlPing();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        int SaveChanges();
    }
    public interface IDatabaseHandle<T> : IDatabaseHandle
    {
        T GetHandle();
        bool ShouldRetry(Exception e);
    }
    public interface IDbFactory<THandle>
    {
        string GetDatabasePath();
        IDatabaseHandle<THandle> CreateReadOnly();
        IDatabaseHandle<THandle> Create();
    }

    ///<summary>
    /// DbDispatcher is a gatekeeper class that will manage connections to a SQLite database in WAL mode. It is coded to handle the specific requirements of 
    /// concurrent access to SQLite in WAL mode. 
    /// <list type="bullet">
    /// <item>
    ///     <description>There can be only one writer thread.</description>
    /// </item>
    /// <item>
    ///     <description>By default, DbDispatcher will open 3 reader threads. That can be customized.</description>
    /// </item>
    /// <item>
    ///     <description>Reader threads are kept open an reused. The writer thread will be closed and reopened after every operation.</description>
    /// </item>
    /// </list>
    /// 
    /// This DbDispatcher is a step up from the older ZAG data-access layer. It's more complicated, but 
    /// it has several distinct advantages:
    /// <list type="bullet">
    /// <item>
    ///     <description>It is async compatible.</description>
    /// </item>
    /// <item>
    ///     <description>It handles concurrency better.  Reads can happen concurrently, and all 
    ///     write operations queue up.</description>
    /// </item>
    /// <item>
    ///     <description>Prevents corruptions that can be caused by reusing SQLite connections 
    ///     between different threads.</description>
    /// </item>
    /// <item>
    ///     <description>Reduces the number of times that a connection to SQLite has to be opened, 
    ///     since that is usually the slowest operation you can do in SQLite.</description>
    /// </item>
    /// <item>
    ///     <description>Routes sync requests through the same DbDispatcher layer, to 
    ///     ensure that write operations will wait for a sync to complete, and that the 
    ///     sync will wait for write operations to complete.</description>
    /// </item>
    /// <item>
    ///     <description>Prevents trying to start multiple syncs at once. They will queue up 
    ///     behind each other.</description>
    /// </item>
    /// </list>
    /// </summary>
    public class DbDispatcher<THandle> : IDisposable
    {
        List<DbThread<THandle>> myPool = new List<DbThread<THandle>>();
        DbThread<THandle> writerThread;
        private object disposelock = new object();
        private IDbFactory<THandle> dbFactory;
        private Task createThreadsTask = null;
        public DbDispatcher(IDbFactory<THandle> dbFactory)
        {
            this.dbFactory = dbFactory;
            createThreadsTask = Task.Run(() => CreateThreads());
        }
        private static int ReaderThreadCount = 3;
        private void CreateThreads()
        {
            lock (disposelock)
            {
                writerThread = new DbThread<THandle>(dbFactory, isreadonly: false);
                var wu = new WorkUnit<IDatabaseHandle<THandle>, int>()
                {
                    action = context => { context?.ExecuteSqlPing(); return 1; },
                    id = Guid.NewGuid(),
                    workCompleted = new ManualResetEvent(false)
                };
                _waitForCompletion(wu, writerThread).Wait();
                //Create each of the DbThread objects.
                for (int i = 0; i < ReaderThreadCount; i++)
                {
                    var thread = new DbThread<THandle>(dbFactory, isreadonly: true);
                    if (i < ReaderThreadCount)
                    {
                        var wu2 = new WorkUnit<IDatabaseHandle<THandle>, int>()
                        {
                            action = context => { context?.ExecuteSqlPing(); return 1; },
                            id = Guid.NewGuid(),
                            workCompleted = new ManualResetEvent(false)
                        };
                        _waitForCompletion(wu2, thread).Wait();
                    }
                    myPool.Add(thread);
                }
            }
        }
        private void DbFactory_DatabaseConnectionsReset(object sender, EventArgs e)
        {
            ResetAllConnections();
        }

        public string GetDatabasePath()
        {
            return dbFactory.GetDatabasePath();
        }

        /// <summary>
        /// Close all open connections. 
        /// </summary>
        public void Dispose()
        {
            CloseAllConnections();
        }

        public void ThrowIfActionIsAsync(Action<THandle> bgTask)
        {
            var mi = bgTask.GetMethodInfo();
            if (mi.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null)
                throw new Exception("You should not be performing async operations inside your callback.");
        }

        ///<summary>
        /// Perform a sync operation on the writer thread. The sync should happen in the callback Action that you provide.
        /// This is identical to a write operation EXCEPT that the DbThread will not surround the callback in a transaction.
        ///</summary>
        public Task ExecuteSync(Action<THandle> bgTask, int timeoutSeconds = -1, Action timeoutElapsedCallback = null)
        {
            ThrowIfActionIsAsync(bgTask);
            var workunit = new WorkUnit<IDatabaseHandle<THandle>, int>()
            {
                id = Guid.NewGuid(),
                isSync = true,
                action = h =>
                {
                    if (h != null)
                    {
                        var handle = h.GetHandle();
                        if (handle != null)
                        {
                            bgTask(h.GetHandle());
                            return h.SaveChanges();
                        }
                        else
                            return 0;
                    }
                    else
                    {
                        bgTask(default(THandle));
                        return 1;
                    }
                },
                syncTimeout = timeoutSeconds > 0 ? timeoutSeconds : -1,
                timeoutElapsedCallback = timeoutElapsedCallback,
                workStarted = new ManualResetEvent(false),
                workCompleted = new ManualResetEvent(false)
            };
            return _waitForCompletion(workunit, ChooseThread(true));
        }

        /// <summary>
        /// Execute an action on the writer thread. The DbThread will open a transaction
        /// and commit after the action is complete.
        /// </summary>
        /// <param name="bgTask">The action that will be performed on the writer thread. A connection handle will be provided, and the transaction will be opened.</param>
        /// <returns>An awaitable Task</returns>
        public Task ExecuteWriteTransaction(Action<THandle> bgTask)
        {
            ThrowIfActionIsAsync(bgTask);
            var workunit = new WorkUnit<IDatabaseHandle<THandle>, int>()
            {
                id = Guid.NewGuid(),
                action = h =>
                {
                    var handle = h.GetHandle();
                    if (handle != null)
                    {
                        bgTask(handle);
                        return h.SaveChanges();
                    }
                    else
                        return 0;
                },
                workStarted = new ManualResetEvent(false),
                workCompleted = new ManualResetEvent(false)
            };
            return _waitForCompletion(workunit, ChooseThread(true));
        }

        /// <summary>
        /// Execute a query on one of the reader threads. No transaction will be created for this query, 
        /// and you should not perform any commands that will change the database.
        /// </summary>
        /// <typeparam name="R">The return type for the query.</typeparam>
        /// <param name="bgTask">The action that will be performed on a reader thread.</param>
        /// <returns></returns>
        public Task<R> ExecuteQuery<R>(Func<THandle, R> bgTask)
        {
            var mi = bgTask.GetMethodInfo();
            if (mi.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null)
                throw new Exception("You should not be performing async operations inside your callback.");
            var workunit = new WorkUnit<IDatabaseHandle<THandle>, R>()
            {
                id = Guid.NewGuid(),
                action = h => 
                {
                    var handle = h.GetHandle();
                    if (handle != null)
                    {
                        return bgTask(handle);
                    }
                    else
                        return default(R);
                },
                workStarted = new ManualResetEvent(false),
                workCompleted = new ManualResetEvent(false),
            };
            return _waitForCompletion(workunit, ChooseThread(false));
        }

        private DbThread<THandle> ChooseThread(bool writeOperation)
        {
            if (createThreadsTask != null && createThreadsTask.IsCompleted == false)
                createThreadsTask.Wait();
            if (writeOperation)
                return writerThread;
            do
            {
                bool poolIsEmpty = false;
                lock (disposelock)
                {
                    foreach (var thread in myPool)
                    {
                        if (thread.WorkQueue.Count == 0 && thread.isBusy == false)
                        {
                            return thread;
                        }
                    }
                    poolIsEmpty = myPool.Count == 0;
                }
                if (poolIsEmpty)
                    Task.Delay(100);
                else
                    return myPool[0];
            } while (true);
        }
        private Task<R> _waitForCompletion<R>(WorkUnit<IDatabaseHandle<THandle>, R> workunit, DbThread<THandle> dbThread)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            if (dbThread == null)
                throw new Exception("Database error. There is no worker pool available.");
            string operation = "Read Operation --";
            if (dbThread == writerThread)
                operation = "Write Operation -- ";
            if (workunit.isSync)
                operation = "Sync -- ";
            var builder = new StringBuilder();
#if DEBUG
            var stack = new StackTrace();
            foreach (var line in stack.GetFrames())
            {
                var decname = line.GetMethod().DeclaringType.Name;
                builder.Append($"|{line.GetMethod().DeclaringType.Name}.{line.GetMethod().Name}|");
            }
#endif
            var description = $"{DateTime.UtcNow.ToString("O")}: {operation} {builder.ToString()}";
            Debug.WriteLine($"{DateTime.UtcNow.ToString("O")} Starting {description}");
            lock (disposelock)
            {
                dbThread.WorkQueue.Add(workunit);
            }
            return Task.Run<R>(() =>
            {
                workunit.workStarted?.WaitOne();
                if (workunit.isSync)
                {
                    var completed = workunit.workCompleted.WaitOne(workunit.syncTimeout > 0 ? workunit.syncTimeout * 1000 : -1);
                    if (completed == false)
                    {
                        workunit.timeoutElapsedCallback();
                        workunit.workCompleted.WaitOne();
                    }
                }
                else
                    workunit.workCompleted.WaitOne();
                s.Stop();
                Debug.WriteLine($"{DateTime.UtcNow.ToString("O")} Finishing {description}");

                Debug.WriteLine(operation +  " Work Unit DB Time: " + workunit.dbTimeElapsed + " Waiting Time: " + s.ElapsedMilliseconds + " milliseconds.");
                if (workunit.exception != null)
                    throw new Exception("DB operation threw an exception", workunit.exception);
                return workunit.result;
            });
        }

        public void ResetAllConnections()
        {
            lock (disposelock)
            {
                CloseAllConnections();
                createThreadsTask = Task.Run(() => CreateThreads());
            }
        }
        public void CloseAllConnections()
        {
            lock (disposelock)
            {
                createThreadsTask = null;
                foreach (var t in myPool)
                {
                    t.Dispose();
                }
                bool flag = true;
                //wait for thread state to change from running to idle(WaitSleepJoin state)
                while (flag)
                {
                    if (writerThread == null)
                        flag = false;
                    else if (writerThread != null && (writerThread.MyThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin
                                                    || writerThread.MyThread.ThreadState == System.Threading.ThreadState.Stopped))
                    {
                        if (writerThread.MyThread.ThreadState != System.Threading.ThreadState.Stopped)
                            writerThread.Dispose();
                        flag = false;
                    }
                }

                myPool.Clear();
            }
        }
    }
}



using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace sample.DbDispatcher
{
    internal class WorkUnit<T>
    {
        internal Guid id;
        internal bool isSync = false;
        internal long dbTimeElapsed;
        internal ManualResetEvent workStarted;
        internal ManualResetEvent workCompleted;
        internal Exception exception;
        internal bool closeCommand = false;
        internal virtual void Execute(T context)
        {
        }
    }
    internal class WorkUnit<T, R> : WorkUnit<T>
    {
        internal Func<T, R> action;
        internal R result;
        internal int syncTimeout = -1;
        internal Action timeoutElapsedCallback;

        internal override void Execute(T context)
        {
            result = action(context);
        }
    }

    internal class DbThread<THandle> : IDisposable
    {
        private IDatabaseHandle<THandle> dbContext = default(IDatabaseHandle<THandle>);

        public  BlockingCollection<WorkUnit<IDatabaseHandle<THandle>>> WorkQueue{ get; set; }

        private Thread myThread;
        private bool shuttingDown = false;
        private readonly IDbFactory<THandle> dbFactory;
        private readonly bool isreadonly;
        public bool isBusy { get; private set; }
        public Thread MyThread
        {
            get
            {
                return myThread;
            }
        }
        public DbThread(IDbFactory<THandle> dbFactory, bool isreadonly)
        {
            this.dbFactory = dbFactory;
            this.isreadonly = isreadonly;
            myThread = new Thread(ThreadLoop);
            myThread.Name = isreadonly ? "Db Reader Thread" : "Db Writer Thread";
            myThread.Start();
            WorkQueue = new BlockingCollection<WorkUnit<IDatabaseHandle<THandle>>>();
        }

        public void ThreadLoop()
        {
            if (isreadonly)
                dbContext = dbFactory.CreateReadOnly();
            isBusy = true;
            bool fullstop = false;
            WorkUnit<IDatabaseHandle<THandle>> currentWork = null;
            WorkUnit<IDatabaseHandle<THandle>> retry = null;
            Stopwatch s = new Stopwatch();

            //Wait a random number of milliseconds, to give everyone else a chance to finish setting up.
            //That reduces the chance that there will be a hang trying to connect 4 different threads
            //simultaneously.
            var r = new Random(DateTime.Now.Millisecond);
            int retryCount = 0, maxRetries = 5;
            while (!fullstop)
            {
                try
                {
                    //Trying to make sure that the main loop is as tight as possible.
                    while (true)
                    {
                        if (!isreadonly)
                        {
                            if (dbContext != null)
                            {
                                dbContext.CloseConnection();
                                dbContext.Dispose();
                            }
                            dbContext = dbFactory.Create();
                            dbContext.ExecuteSqlPing();
                        }
                        s.Reset();
                        isBusy = false;
                        if (retry == null)
                            currentWork = WorkQueue.Take();
                        else
                            currentWork = retry;
                        retry = null;
                        isBusy = true;
                        if (currentWork.closeCommand == true)
                        {
                            fullstop = true;
                            break;
                        }
                        s.Start();
                        if (currentWork.isSync)
                        {
                            if (dbContext != null)
                            {
                                dbContext.CloseConnection();
                                dbContext.Dispose();
                                dbContext = null;
                            }
                            currentWork.workStarted?.Set();
                            currentWork.Execute(dbContext);
                        }
                        else
                        {
                            currentWork.workStarted?.Set();
                            if (!isreadonly) dbContext.BeginTransaction();
                            currentWork.Execute(dbContext);
                            if (!isreadonly) dbContext.CommitTransaction();
                        }
                        s.Stop();
                        currentWork.dbTimeElapsed = s.ElapsedMilliseconds;
                        currentWork.workCompleted.Set();
                        
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        dbContext?.RollbackTransaction();
                    }
                    catch
                    {
                        //Ignore any exceptions on the rollback.
                    }
                    if (!shuttingDown)
                    {
                        Debug.Write(e.ToString());
                        if (dbContext?.ShouldRetry(e) == true && retryCount < maxRetries)
                        {
                            Debug.Write("retry requested");
                            retry = currentWork;
                        }
                        else if (currentWork != null)
                        {
                            currentWork.exception = e;
                            currentWork.workCompleted.Set();
                        }
                    }
                }
                retryCount = retry == null ? 0 : retryCount+1;
            }
            dbContext.CloseConnection();
            dbContext.Dispose();
            WorkQueue.Dispose();
        }

        private void recreateWriteContext()
        {
            if (!shuttingDown && !isreadonly)
            {
                dbContext.CloseConnection();
                dbContext.Dispose();
                dbContext = dbFactory.Create();
                dbContext.ExecuteSqlPing();
            }
        }
        public void Dispose()
        {
            shuttingDown = true;
            WorkQueue.Add(new WorkUnit<IDatabaseHandle<THandle>>()
            {
                closeCommand = true
            });

        }
    }
}


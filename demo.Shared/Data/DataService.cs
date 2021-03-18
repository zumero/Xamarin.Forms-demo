using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using demo.Models;
using sample.DbDispatcher;
using SQLite;
using Zumero;

namespace demo.Data
{
    public interface IDataService
    {
        Task<IList<T>> LoadAll<T>() where T : new();
        Task<IList<T>> LoadFiltered<T>(Expression<Func<T, bool>> expression) where T : new();
        Task Update(object item);
        Task Insert(object item);
        Task ExecuteBatchInsertsFirst(object[] inserts, object[] updates = null, object[] deletes = null);
        Task ExecuteBatchUpdatesFirst(object[] updates, object[] inserts = null, object[] deletes = null);

        Task<bool> IsEmpty();
        Task<string> DescribeSync(int syncid);
        Task<int> Sync(SyncParams syncParams, ZumeroClient.callback_progress_handler callback);
        void CreateConnections();
        void CloseConnections();
        Task<SyncParams> LoadSyncParams();
        Task SaveSyncParams(SyncParams syncParams);
    }

    /// <summary>
    /// This is written in such a way that all of your database operations flow through
    /// this class, and are routed into the DbDispatcher.It is best to retain that structure,
    /// or make sure that a known set of classes is interacting with the DbDispatcher. That will
    /// help make sure that you don't get database connection and query logic in your views and 
    /// view models. 
    /// </summary>
    public class DataService : IDataService
    {
        private static DbDispatcher<SQLiteConnection> _dispatcher = null;
        private DbDispatcher<SQLiteConnection> Dispatcher
        {
            get
            {
                if (_dispatcher == null)
                {
                    _dispatcher = new DbDispatcher<SQLiteConnection>(new SqliteNetPclFactory(SharedApp.DatabasePath));
                }
                return _dispatcher;
            }
        }
        public DataService()
        {
        }

        public Task<bool> IsEmpty()
        {
            return Dispatcher.ExecuteQuery(db =>
            {
                try
                {

                    db.Query<int>("SELECT COUNT(*) FROM t$v;");
                    return false;
                }
                catch (Exception)
                {
                    return true;
                }
            });
        }

        public Task<IList<T>> LoadAll<T>() where T : new()
        {
            return Dispatcher.ExecuteQuery(db =>
            {
                IList<T> results = db.Table<T>().ToList();
                return results;
            });
        }
        /// <summary>
        /// A helper method to return a subset of rows. The filtering is done in the database, 
        /// before any rows are loaded into the C# layer.
        /// </summary>
        /// <example>
        /// <code>
        ///     var results = await DependencyService.Get<IDataService>().LoadFiltered<Models.presidents>(p => p.full_name = "George Washington");
        /// </code>
        /// </example>
        /// <example>
        /// For columns like datetime, numeric, and guid fields (which 
        /// are stored as strings or ints), you will only be able to 
        /// query on the raw storage column.  ZAG has generated a 
        /// conversion function to help.  The additional wrinkle is that 
        /// you need to cache the search term in a local variable before 
        /// querying
        /// <code>
        ///     long? searchNum = Models.chemical_elements.electronegativity_ConvertToInt(1.0m);
        ///     var results = await DependencyService.Get<IDataService>().LoadFiltered<Models.chemical_elements>(e => e.electronegativity_raw < searchNum);
        /// </code>
        /// </example>
        /// <typeparam name="T">The type to be queried in the database, and returned.</typeparam>
        /// <param name="expression">The expression that will be provided to the Where() clause of the database query.</param>
        /// <returns></returns>
        public Task<IList<T>> LoadFiltered<T>(Expression<Func<T, bool>> expression) where T : new()
        {
            return Dispatcher.ExecuteQuery(db =>
            {
                IList<T> results = db.Table<T>().Where(expression).ToList();
                return results;
            });
        }
        public Task Update(object item)
        {
            return Dispatcher.ExecuteWriteTransaction(db =>
            {
                db.Update(item);
            });
        }

        public Task Insert(object item)
        {
            return Dispatcher.ExecuteWriteTransaction(db =>
            {
                db.Insert(item);
            });
        }

        /// <summary>
        /// This is a sample of how to multiple operations in one transaction. This method may be useful,
        /// but more likely, you will need to write a special-purpose method that does the operations you
        /// need in exactly the right order.
        /// </summary>
        /// <param name="inserts"></param>
        /// <param name="updates"></param>
        /// <param name="deletes"></param>
        /// <returns></returns>
        public Task ExecuteBatchInsertsFirst(object[] inserts, object[] updates = null, object[] deletes = null)
        {
            return Dispatcher.ExecuteWriteTransaction(db =>
            {
                foreach (var item in inserts)
                    db.Insert(item);
                if (updates != null)
                    foreach (var item in updates)
                        db.Update(item);
                if (deletes != null)
                    foreach (var item in deletes)
                        db.Update(item);
            });
        }

        public Task ExecuteBatchUpdatesFirst(object[] updates, object[] inserts = null, object[] deletes = null)
        {
            return Dispatcher.ExecuteWriteTransaction(db =>
            {
                foreach (var item in updates)
                    db.Update(item);
                if (inserts != null)
                    foreach (var item in inserts)
                        db.Insert(item);
                if (deletes != null)
                    foreach (var item in deletes)
                        db.Update(item);
            });
        }
        private class StringHolder
        {
            public StringHolder()
            { }
            public string name { get; set; }
        }
        public Task<string> DescribeSync(int syncid)
        {
            return Dispatcher.ExecuteQuery(db =>
            {
                StringBuilder sb = new StringBuilder();

                IEnumerable<StringHolder> tables = db.Query<StringHolder>("SELECT name from sqlite_master WHERE type = 'table' and name NOT LIKE '%$%'and name NOT LIKE 'zumero_%'and name NOT LIKE 'sqlite_%' ORDER BY name;", new object[] { });

                foreach (StringHolder t in tables)
                {
                    string tableName = t.name;
                    try
                    {

                        string syncDescription = db.ExecuteScalar<string>("SELECT \n" +
                                                            "  CASE (SELECT action FROM [zumero_sync_" + tableName + "] WHERE syncid = " + syncid + " LIMIT 1) \n" +
                                                            "  WHEN 'r' THEN\n" +
                                                            "'All rows in the table were replaced'\n" +
                                                            "  ELSE\n" +
                                                            "    'Sync updated ' || (SELECT count(*) FROM [zumero_sync_" + tableName + "] WHERE syncid = " + syncid + " AND action = 'u') || ' row(s)' || x'0a'\n" +
                                                            "    || 'Sync deleted ' || (SELECT count(*) FROM [zumero_sync_" + tableName + "] WHERE syncid = " + syncid + " AND action = 'd') || ' row(s)' || x'0a'\n" +
                                                            "    || 'Sync added ' || (SELECT count(*) FROM [zumero_sync_" + tableName + "] WHERE syncid = " + syncid + " AND action = 'i') || ' row(s)' || x'0a'\n" +
                                                            "END", new object[] { });
                        if (syncDescription != "")
                            sb.AppendLine(tableName + " -- " + syncDescription);
                    }
                    catch (Exception)
                    {
                        //Ignore any exceptions.
                    }
                }
                return sb.ToString();
            });

        }

        public async Task<int> Sync(SyncParams syncParams, ZumeroClient.callback_progress_handler callback)
        {
            int syncid = -1;
            await Dispatcher.ExecuteSync(_ =>
            {

                string jsOptions = "{\"sync_details\":true}";
                if (syncParams.SendAuth)
                    ZumeroClient.Sync(Dispatcher.GetDatabasePath(), null, syncParams.URL, syncParams.DBFile, syncParams.Scheme, syncParams.User, syncParams.Password, jsOptions, out syncid, callback);
                else
                    ZumeroClient.Sync(Dispatcher.GetDatabasePath(), null, syncParams.URL, syncParams.DBFile, null, null, null, jsOptions, out syncid, callback);
            });
            return syncid;
        }

        public void CreateConnections()
        {
            Dispatcher.ResetAllConnections();
        }

        public void CloseConnections()
        {
            Dispatcher.CloseAllConnections();
        }

        public async Task<SyncParams> LoadSyncParams()
        {

            SyncParams param = new SyncParams(); ;
            //This is a write transaction, since it may need to create the table.
            await Dispatcher.ExecuteWriteTransaction(db =>
            {
                db.CreateTable<SyncParams>();
                if (db.Table<SyncParams>().Count() != 0)
                    param = db.Table<SyncParams>().First();
            });
            return param;
        }

        public Task SaveSyncParams(SyncParams syncParams)
        {
            return Dispatcher.ExecuteWriteTransaction(db =>
            {

                db.CreateTable<SyncParams>();
                syncParams.id = 1;
                if (db.Table<SyncParams>().Count() != 0)
                    db.Update(syncParams);
                else
                    db.Insert(syncParams);
            });
        }
    }
}

using SQLite;
using System;
using System.Runtime.CompilerServices;
using Zumero;

namespace sample.DbDispatcher
{
    /// <summary>
    /// This is an implementation of a data-access layer buillt on Sqlite-Net-Pcl.
    /// If you need a different layer, you can adapt this to your own needs.
    /// </summary>
    public class SqliteNetPclDatabaseHandle : IDatabaseHandle<SQLiteConnection>
    {
        private readonly string dbPath;
        private bool _readOnly;
        private SQLiteConnection _connection;
        public SqliteNetPclDatabaseHandle(string dbPath, bool readOnly)
        {
            this.dbPath = dbPath;
            _readOnly = readOnly;
            TryToReestablishConnection();

        }
        private void TryToReestablishConnection()
        {
            //It's better to let Zumero be the one to create the database. Only open up a connection if 
            //the database already exists.
            if (_connection == null && System.IO.File.Exists(dbPath) == true)
            {
                _connection = new SQLiteConnection(dbPath, (_readOnly ? SQLiteOpenFlags.ReadOnly : SQLiteOpenFlags.ReadWrite));

                //Turning on WAL mode is important if you're going to be 
                //doing any concurrent access to the database.
                _connection.Query<object>("PRAGMA journal_mode=WAL");

                //This would be the spot to PRAGMA key = cryptokey if you are using sqlcipher.

                //These pragma statements are very important to ensure
                //that Zumero can track changes correctly.
                _connection.Execute("PRAGMA foreign_keys=ON");
                _connection.Execute("PRAGMA recursive_triggers=ON");
            }
        }
        public void BeginTransaction()
        {
            TryToReestablishConnection();
            _connection?.Execute("BEGIN IMMEDIATE TRANSACTION");
        }

        public void CloseConnection()
        {
            _connection?.Close();
            _connection = null;
        }

        public void CommitTransaction()
        {
            _connection?.Execute("COMMIT");
        }

        public void RollbackTransaction()
        {
            _connection?.Execute("ROLLBACK");
        }

        public void Dispose()
        {
            _connection?.Close();
        }

        public void ExecuteSqlPing()
        {
            TryToReestablishConnection();
            _connection?.ExecuteScalar<int>("select 1;");
        }

        public SQLiteConnection GetHandle()
        {
            TryToReestablishConnection();
            return _connection;
        }

        public int SaveChanges()
        {
            if (_readOnly)
                throw new Exception("Attempted to save changes on a read-only connection");
            else
                return 1;
        }

        public bool ShouldRetry(Exception e)
        {
            //Busy results should be retried.
            var sqlException = e as SQLiteException;
            if (sqlException != null && (sqlException.Result == SQLite3.Result.Busy || sqlException.Result == SQLite3.Result.Locked))
                return true;
            var zException = e as ZumeroException;
            if (zException != null && (zException.ErrorCode == (int)SQLite3.Result.Busy || zException.ErrorCode == (int)SQLite3.Result.Locked))
                return true;
            return false;
        }
    }
    public class SqliteNetPclFactory : IDbFactory<SQLiteConnection>
    {
        private string _dbPath;

        public SqliteNetPclFactory()
        {
        }
        public SqliteNetPclFactory(string dbPath)
        {
            _dbPath = dbPath;
        }

        public string GetDatabasePath()
        {
            return _dbPath;
        }

        public IDatabaseHandle<SQLiteConnection> CreateReadOnly()
        {
            return new SqliteNetPclDatabaseHandle(_dbPath, true);
        }

        public IDatabaseHandle<SQLiteConnection> Create()
        {
            return new SqliteNetPclDatabaseHandle(_dbPath, false);
        }
    }
}

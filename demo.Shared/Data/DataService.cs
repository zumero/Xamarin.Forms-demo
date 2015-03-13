using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLite;


namespace demo.Data
{
    public interface IDataService
    {
        IList<T> LoadAll<T>() where T : new();
        void Update(object item);
        void Insert(object item);
        bool HasNeverBeenSynced();
    }

    public class DataService : IDataService
    {
        
        private SQLiteConnection _connection;
        public DataService()
        {
            _connection = new SQLiteConnection(App.DatabasePath);
        
        
            //Turning on WAL mode is important if you're going to be 
            //doing any concurrent access to the database.
            _connection.Query<object>("PRAGMA journal_mode=WAL");

            //These pragma statements are very important to ensure
            //that Zumero can track changes correctly.
            _connection.Execute("PRAGMA foreign_keys=ON");
            _connection.Execute("PRAGMA recursive_triggers=ON");
            

            // In a normal SQLite-Net app, you would call CreateTable() here.
            // Do not do that!! Zumero will be in charge of creating tables
            // and updating schemas.
        }

        private void StartReadTransaction()
        {
            _connection.Execute("BEGIN TRANSACTION");
        }

        private void StartWriteTransaction()
        {
            //This method handles cases where a Zumero Sync may be going on in a 
            //background thread.  It will block until the database is unlocked after
            //the sync completes. The other benefit is that starting a write
            //transaction will cause any future Sync calls to block until
            //the write transaction is complete.
            bool txBegun = false;
            
            while (txBegun == false)
            {
                try
                {
                    _connection.Execute("BEGIN IMMEDIATE TRANSACTION");
                    txBegun = true;
                }
                catch (SQLiteException e)
                {
                    if (e.Result == SQLite3.Result.Busy || 
                        e.Result == SQLite3.Result.Locked)
                        //This is a portable way of doing Thread.Sleep().
                        Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                        
                    else
                        throw;
                }
            }
        }

        private void Commit()
        {
            _connection.Execute("COMMIT");
        }

        private void Rollback()
        {
            _connection.Execute("ROLLBACK");
        }

        public bool HasNeverBeenSynced()
        {
            try
            {
                _connection.Query<int>("SELECT COUNT(*) FROM t$v;");
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public IList<T> LoadAll<T>() where T : new()
        {
            // If you wanted to filter the results, you would do something 
            // like this:
            //     return _connection.Table<Models.presidents>().Where(v => v.full_name == "George Washington").ToList();
            // For columns like datetime, numeric, and guid fields (which 
            // are stored as strings or ints), you will only be able to 
            // query on the raw storage column.  ZAG has generated a 
            // conversion function to help.  The additional wrinkle is that 
            // you need to cache the search term in a local variable before 
            // querying.  It will look something like this:
            //    long? searchNum = Models.chemical_elements.electronegativity_ConvertToInt(1.0m);
            //    return (IList<T>)_connection.Table<Models.chemical_elements>().Where(v => v.electronegativity_raw < searchNum).ToList();
            // or, using the linq syntax:
            //    long? searchNum = Models.chemical_elements.electronegativity_ConvertToInt(1.0m);
            //    return (IList<T>)(from results in _connection.Table<Models.chemical_elements>()
            //        where results.electronegativity_raw > searchNum
            //        select results).ToList();
            this.StartReadTransaction();
            try
            {
                IList<T> results = _connection.Table<T>().ToList();
                this.Commit();
                return results;
            }
            catch (Exception)
            {
                this.Rollback();
                throw;
            }
        }

        public void Update(object item)
        {
            this.StartWriteTransaction();
            try
            {
                _connection.Update(item);
                this.Commit();
            }
            catch (Exception)
            {
                this.Rollback();
                throw;
            }
        }

        public void Insert(object item)
        {
            this.StartWriteTransaction();
            try
            {
                _connection.Insert(item);
                this.Commit();
            }
            catch (Exception)
            {
                this.Rollback();
                throw;
            }
        }
    }
}

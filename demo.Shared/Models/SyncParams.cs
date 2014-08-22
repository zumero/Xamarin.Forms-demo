using System;
using System.Text;
using Xamarin.Forms;
using demo;
using SQLite;

namespace demo.Models
{
	[Table("zumero_sync_settings")]
    public class SyncParams : BaseModel
	{
		[PrimaryKey, AutoIncrement]
		public int id { get; set; }
		
		private string _URL = @"http://demo.zumero.com:8080";
		public string URL
		{
			get { return _URL; }
            set { SetProperty(ref _URL, value, "URL"); }
		}
		
		private string _DBFile = @"demo";
		public string DBFile
		{
			get { return _DBFile; }
            set { SetProperty(ref _DBFile, value, "DBFile"); }
		}

		private string _scheme = "";
		public string Scheme
		{
			get { return _scheme; }
            set { SetProperty(ref _scheme, value, "Scheme"); }
		}

		private string _user = "";
		public string User
		{
			get { return _user; }
            set { SetProperty(ref _user, value, "User"); }
		}

		private string _password = @"";
		[Ignore]
		public string Password
		{
			get { return _password; }
            set { SetProperty(ref _password, value, "Password"); }
		}		

		private bool _sendAuth = false;
		public bool SendAuth
		{
			get { return _sendAuth; }
            set { SetProperty(ref _sendAuth, value, "SendAuth"); }
		}

        public static SyncParams LoadSavedSyncParams()
        {
            SyncParams param = null;
            SQLiteConnection db = new SQLiteConnection(App.DatabasePath);
            db.CreateTable<SyncParams>();
            if (db.Table<SyncParams>().Count() != 0)
                param = db.Table<SyncParams>().First();
            else
                param = new SyncParams();
            db.Close();
            return param;
        }

        public void SaveSyncParam()
        {
            SQLiteConnection db = new SQLiteConnection(App.DatabasePath);

            db.CreateTable<SyncParams>(); 
            this.id = 1;
            if (db.Table<SyncParams>().Count() != 0)
                db.Update(this);
            else
                db.Insert(this);
            db.Close();
        }
	}
}

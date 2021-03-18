using System;
using System.Text;
using demo;
using SQLite;

namespace demo.Models
{
	[Table("zumero_sync_settings")]
    public class SyncParams : BaseModel
	{
		[PrimaryKey, AutoIncrement]
		public int id { get; set; }
		
		[Ignore]
		public string SyncDescription { get; set; }
		
		private string _URL = @"https://demo.zumero.com";
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

		private string _scheme = @"";
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
	}
}

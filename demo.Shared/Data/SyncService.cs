using demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Zumero;

namespace demo.Data
{
    //The sync operation is abstracted out as a service,
    //in order for Android to wrap the sync in a background
    //service.  That ensures that a sync operation will not
    //be terminated by the OS if the user leaves the sync activity
    public interface ISyncService
    {
		bool IsSyncRunning();
        void StartBackgroundSync(SyncParams syncParams);
		void RevertLocalChanges();
    }
    
    public class BaseSyncService : ISyncService
    {
        //This default version is used on iOS and WinPhone
        public void StartBackgroundSync(SyncParams syncParams)
        {
            Task.Run(() => { Sync(syncParams); });
        }
		
		bool _isSyncRunning = false;
        public bool IsSyncRunning()
        {
            return _isSyncRunning;
        }

		public void RevertLocalChanges()
		{
			if (DependencyService.Get<IDataService>().HasNeverBeenSynced())
				return;
			ZumeroClient.QuarantineSinceLastSync(App.DatabasePath, null);
		}
		
        //This blocking version is called by Android from the
        //background service.
        public void Sync(SyncParams syncParams)
        {
			_isSyncRunning = true;
            try
            {
                if (syncParams.SendAuth)
                    ZumeroClient.Sync(App.DatabasePath, null, syncParams.URL, syncParams.DBFile, syncParams.Scheme, syncParams.User, syncParams.Password);
                else
                    ZumeroClient.Sync(App.DatabasePath, null, syncParams.URL, syncParams.DBFile, null, null, null);
				_isSyncRunning = false;
                App.Current.NotifySyncCompleted(syncParams);
            }
            catch (Exception e)
            {
				if (e is ZumeroException)
				{
					//if you need to react to a certain result, you can check
					//the result of the sync like this:
					
					//ZumeroException ze = (ZumeroException)e;
					//
					//if (ze.ErrorCode == (int)ZumeroResult.AuthenticationFailed)
					//...
					
					//The error code could also be a SQLite error code
					//
					//if (ze.ErrorCode == (int)SQLite3.Result.Error)
					//...
				}
				_isSyncRunning = false;
                App.Current.NotifySyncFailed(e);
            }
            
        }
    }
}

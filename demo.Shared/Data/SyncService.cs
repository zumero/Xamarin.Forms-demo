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
		void Cancel();
		void Cancel(int cancelToken);
    }
    
    public class BaseSyncService : ISyncService
    {
	    int _cancellationToken;
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

        public void Cancel()
        {
            ZumeroClient.Cancel(_cancellationToken);
        }
		
		public void Cancel(int cancelToken)
		{
			ZumeroClient.Cancel(cancelToken);
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
			ZumeroClient.callback_progress_handler callback = (cancellationToken, phase, bytesSoFar, bytesTotal) =>
			{
				_cancellationToken = cancellationToken;
				App.CancelToken = cancellationToken;
				string syncProgressString = "";
				if (phase == (int)ZumeroPhase.Preparing)
					syncProgressString = "Preparing";
				else if (phase == (int)ZumeroPhase.Uploading)
					syncProgressString = "Uploading " + bytesSoFar + " of " + bytesTotal;
				else if (phase == (int)ZumeroPhase.WaitingForResponse)
					syncProgressString = "Waiting for response";
				else if (phase == (int)ZumeroPhase.Downloading)
					syncProgressString = "Downloading " + bytesSoFar + " of " + bytesTotal;
				else if (phase == (int)ZumeroPhase.Applying)
					syncProgressString = "Applying";
				((demo.App)Xamarin.Forms.Application.Current).NotifySyncProgress(syncProgressString);
			};
            try
            {
		string jsOptions = "{\"sync_details\":true}";
                int syncid = -1;
                if (syncParams.SendAuth)
                    ZumeroClient.Sync(App.DatabasePath, null, syncParams.URL, syncParams.DBFile, syncParams.Scheme, syncParams.User, syncParams.Password, jsOptions, out syncid, callback);
                else
                    ZumeroClient.Sync(App.DatabasePath, null, syncParams.URL, syncParams.DBFile, null, null, null, jsOptions, out syncid, callback);
				_isSyncRunning = false;

                string syncDescription =  DependencyService.Get<IDataService>().DescribeSync(syncid);
                syncParams.SyncDescription = syncDescription;
                ((demo.App)Xamarin.Forms.Application.Current).NotifySyncCompleted(syncParams);
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
                ((demo.App)Xamarin.Forms.Application.Current).NotifySyncFailed(e);
            }
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using demo.Data;
using demo.Models;
using Android.Util;


[assembly: Dependency(typeof(demo.Droid.AndroidSyncService))]
namespace demo.Droid
{
    [Service]
    class AndroidBackgroundSyncService : IntentService
    {
        #region Set up for AndroidSyncService

        // The name of our published service.  This 
        // must match the value listed in the manifest.
        public const string SERVICE_BROADCAST = "com.example.demo.ZumeroSyncService";


        public AndroidBackgroundSyncService()
            : base(SERVICE_BROADCAST)
        {
        }

        public AndroidBackgroundSyncService(string name)
            : base(name)
        {
        }
        #endregion

        #region Handle A Sync Request

        protected override void OnHandleIntent(Intent i)
        {
            Log.Debug(Class.Name, "OnHandleIntent starting");
            SyncParams p = new SyncParams();

            p.URL = i.GetStringExtra("url");
            p.DBFile = i.GetStringExtra("dbfile");
            p.SendAuth = i.GetBooleanExtra("sendAuth", false);
            p.Scheme = i.GetStringExtra("scheme");
            p.User = i.GetStringExtra("user");
            p.Password = i.GetStringExtra("password");

            //Call the BaseSyncService in the portable Core.
            //Since we're already in the background, call the blocking version
            //of sync.
            new BaseSyncService().Sync(p);
            Log.Debug(Class.Name, "OnHandleIntent ending");
            this.StopSelf();
        }

        #endregion

    }

    public class AndroidSyncService : ISyncService
    {
        public static Activity SyncActivity = null;

		public bool IsSyncRunning()
        {
            //This is not used on Android.
            throw new NotImplementedException("Android does not have yet have a way to tell if the sync is still running.");
        }
		
        public void Cancel()
        {
            new BaseSyncService().Cancel(App.CancelToken);
            
        }

        public void Cancel(int cancelToken)
        {
            new BaseSyncService().Cancel(cancelToken);

        }

		public void RevertLocalChanges()
		{
			new BaseSyncService().RevertLocalChanges();
		}
		
        public void StartBackgroundSync(SyncParams p)
        {
            if (SyncActivity == null)
                return;
            Intent syncIntent = new Intent(SyncActivity, typeof(AndroidBackgroundSyncService));
            syncIntent.PutExtra("url", p.URL);
            syncIntent.PutExtra("dbfile", p.DBFile);
            syncIntent.PutExtra("sendAuth", p.SendAuth);
            syncIntent.PutExtra("scheme", p.Scheme);
            syncIntent.PutExtra("user", p.User);
            syncIntent.PutExtra("password", p.Password);
            //Starting a service can take a while, do that in a background
            //thread as well.
            Task.Run(() =>
            {
                SyncActivity.StartService(syncIntent);
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using demo.Views;
using demo.Models;
using demo.Data;

[assembly: Xamarin.Forms.Dependency(typeof(demo.Data.DataService))]

namespace demo
{
    public class SharedApp : Application
    {
		public static int CancelToken { get; set; }
        public SharedApp()
        {
            MainPage = new RootNavigationPage(new demo.xaml.WelcomePage());
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            DependencyService.Get<IDataService>().CloseConnections();
        }
        protected override void OnResume()
        {
            base.OnResume();
            DependencyService.Get<IDataService>().CreateConnections();
        }

        public event EventHandler<Exception> SyncFailed = delegate { };

        public void NotifySyncFailed(Exception e)
        {
            if (SyncFailed != null)
                SyncFailed(this, e);
        }

        public event EventHandler<SyncParams> SyncCompleted = delegate { };

        public void NotifySyncCompleted(SyncParams p)
        {
            if (SyncCompleted != null)
                SyncCompleted(this, p);
        }

        public event EventHandler<string> SyncProgress = delegate { };

        public void NotifySyncProgress(string progressString)
        {
            if (SyncProgress != null)
                SyncProgress(this, progressString);
        }
		
        public static string DatabasePath { get; set; }

    }
}

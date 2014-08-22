using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using demo.Views;
using demo.Models;

namespace demo
{
    public class App
    {
		public static App Current
        {
            get { return current; }
        } 
        private static App current;

        static App()
        {
            current = new App();
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
		
        public static string DatabasePath { get; set; }

        public static Page GetMainPage()
        {
            return new RootNavigationPage(new demo.xaml.WelcomePage());
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using demo.Views;
using demo.Models;

[assembly: Xamarin.Forms.Dependency(typeof(demo.Data.DataService))]

namespace demo
{
    public class App : Application
    {
        public App()
        {
            MainPage = new RootNavigationPage(new demo.xaml.WelcomePage());
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

    }
}

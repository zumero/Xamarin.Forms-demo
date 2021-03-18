﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using demo.Views;
using Xamarin.Forms;

namespace demo.xaml
{
    public partial class WelcomePage
    {
        ToolbarItem tablesItem = new ToolbarItem() { Text = "Tables"
			, IconImageSource = (Device.RuntimePlatform == Device.UWP) ? "tables.png" : null
			, Order = ToolbarItemOrder.Primary };

        private string _welcomeString = "<html>" +
        "<head>" +
        "  <style>" +
        "    body { background: white; }" +
        "  </style>" +
        "</head>" +
        "<body>" +
        "<h1>Welcome Page</h1>" +
        "<p>" +
        "Welcome to your custom-generated Xamarin.Forms app." +
        "</p>" +
        "<p>" +
        "This is a demonstration app and intended to help you get started using ZSS." +
        " It will allow you to sync your database from the ZSS Server to this device" +
        " and then do some simple queries, inserts and updates." +
        "</p>" +
        "<p>" +
        "More importantly, the source code for this app can be used as a basis for your real app." +
        "</p>" +
        "<h2>Usage</h2>" +
        "<ul>" +
        "<li>" +
        "The SYNC button will start a sync with the ZSS Server." +
        " The first use of this may take a while as the entire database (as exposed" +
        " in a DBFile in ZSS Manager) is downloaded." +
        " Subsequent uses should be quicker as only the local and remote changes are exchanged." +
        "</li>" +
        "<li>" +
        "The TABLES button will open a " +
        " list of the prepared tables in your database." +
        " Clicking on one of them will run a local query on that" +
        " table and display the rows in a scrollable list." +
        " Clicking on one of these rows will open a detail page." +
        "</li>" +
		"<li>" +
        "Data rows can be INSERTED and UPDATED " +
        " in the local database. Those changes will not be sent to " +
        " the server until the next time that the SYNC button is pushed." +
        "</li>" +
		"<li>" +
        "To undo all changes that have been made in the local database since " +
        " the last SYNC operation, use the REVERT CHANGES button on the TABLES page. " +
        "</li>" +
        "</ul>" +
        "<hr/>\n" +
        "<font size=-1>\n" +
        "<p>Generated by ZAG version: 3.3.0.1129 <tt>(220dcd40a75f29a14a2748049529112c764d1b43)</tt></p>\n" +
        "<p>Using ZAG template: <tt>demo1__xamarin.forms</tt></p>\n" +
        "<p>DBFile signature: 45D8F49EECE418BADFF877D5ED79711D8EA22C1C</p>\n" +
        "</font>\n" +
        "</body>" +
        "</html>";

        public HtmlWebViewSource LocalHtmlWebViewSource
        {
            get
            {
                return new HtmlWebViewSource
                {
                    Html = _welcomeString
                };
            }
            set { ; }
        }
        public WelcomePage()
        {
            InitializeComponent();
			if (Device.RuntimePlatform == Device.UWP)
                this.Padding = new Xamarin.Forms.Thickness(this.Padding.Left, this.Padding.Top, this.Padding.Right, 95);
			ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Sync"
				, IconImageSource = (Device.RuntimePlatform == Device.UWP) ? "refresh.png" : null
                , Order = ToolbarItemOrder.Primary,
                Command = new Command(() =>
                {
                    this.Navigation.PushAsync(new SyncPage());
                })
            });
			
            ToolbarItems.Add(tablesItem);
            tablesItem.Command = new Command(() =>
            {
                this.Navigation.PushAsync(new TablesPage());
            });
            BindingContext = this;
            
        }
    }
}

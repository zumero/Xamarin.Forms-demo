using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace demo.Views
{
    class RootNavigationPage : NavigationPage
    {
        public RootNavigationPage(Page page) : base(page)
        {
            this.Popped += RootNavigationPage_Popped;
            this.Pushed += RootNavigationPage_Pushed;
        }

        void RootNavigationPage_Pushed(object sender, NavigationEventArgs e)
        {
			//This is here to work around a bug on Windows Phone that was retaining
			//old toolbaritems.
            ToolbarItems.Clear();
        }

	void RootNavigationPage_Popped(object sender, NavigationEventArgs e)
        {
			//This is here to work around a bug on Windows Phone that was retaining
			//old toolbaritems.
            ToolbarItems.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using demo.Data;
using demo.Views;
using Xamarin.Forms;
using demo.Models;

namespace demo.xaml
{
    public partial class chemical_elementsPage : ContentPage
    {
        public chemical_elementsPage()
        {
            InitializeComponent();
			if (Device.OS == TargetPlatform.Windows)
                this.Padding = new Xamarin.Forms.Thickness(this.Padding.Left, this.Padding.Top, this.Padding.Right, 95);
			
            this.BindingContext = this;

			ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Sync"
				, Icon = (Device.OS == TargetPlatform.WinPhone) ? "refresh.png" : null
                , Order = ToolbarItemOrder.Primary,
                Command = new Command(() =>
                {
                    this.Navigation.PushAsync(new SyncPage());
                })
            });
			
            ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Insert",
				Icon = (Device.OS == TargetPlatform.WinPhone) ? "new.png" : null,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() =>
                {
                    this.Navigation.PushAsync(new chemical_elementsDetailPage(null));
                })
            });
        }

        public void ItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem == null)
                return;
            else
            {
                this.Navigation.PushAsync(new chemical_elementsDetailPage(args.SelectedItem as Models.chemical_elements));
            }
        }
		
		bool alreadyRedirectedOnce = false;
        protected override async void OnAppearing()
        {
            base.OnAppearing();
	    Exception ex = null;
            try
            {
                this.FindByName<ListView>("itemsList").ItemsSource = await Task<IList<Models.chemical_elements>>.Run(() =>
                {
                    return DependencyService.Get<IDataService>().LoadAll<Models.chemical_elements>();
                });
            }
	    catch (Exception e)
            {
                ex = e;
            }
            if (ex != null)
            {
                if (ex.Message.StartsWith("no such table"))
                {
                    await this.DisplayAlert("Exception", "A sync hasn't been performed yet.  Please sync.", "Ok");
                    if (!alreadyRedirectedOnce)
                    {
                        alreadyRedirectedOnce = true;
                        await this.Navigation.PushAsync(new SyncPage());
                    }
                }
                else
                    await this.DisplayAlert("Exception", ex.Message, "Ok");
            }
        }
    }
}

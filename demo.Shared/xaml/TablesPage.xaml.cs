using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using demo.Data;

namespace demo.xaml
{
    public partial class TablesPage : ContentPage
    {
        public TablesPage()
        {
            InitializeComponent();
			ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Sync",
				Icon = (Device.OS == TargetPlatform.WinPhone) ? "refresh.png" : null,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() =>
                {
                    this.Navigation.PushAsync(new SyncPage());
                })
            });
			
			ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Revert Changes",
				Icon = (Device.OS == TargetPlatform.WinPhone) ? "undo.png" : null,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(async () =>
                {
					bool revert = await this.DisplayAlert("Confirm", "Are you sure that you want to revert all changes made to the local database since the last sync?", "Revert", "Cancel");
                    if (revert)
					{
						try
						{
							DependencyService.Get<ISyncService>().RevertLocalChanges();
							this.DisplayAlert("Reverted", "All local changes have been reverted.", "Ok");
						}
						catch (Exception e)
						{
							this.DisplayAlert("Exception", e.Message, "Ok");
						}
					}
                })
            });
			
            this.BindingContext = this;
        }

        private string[] _tables = new string[] { 
		"countries",
		"presidents",
		"scratch",
		"chemical_elements",
		};
        public string[] Tables
        {
            get { return _tables; }
            set { ; }
        }

        public void ItemTapped(object sender, ItemTappedEventArgs args)
        {
            if (args.Item == null)
                return;
            else
            {
				Page target = null;
				if (((string)args.Item) == "countries") target = new countriesPage();
				if (((string)args.Item) == "presidents") target = new presidentsPage();
				if (((string)args.Item) == "scratch") target = new scratchPage();
				if (((string)args.Item) == "chemical_elements") target = new chemical_elementsPage();
                this.Navigation.PushAsync(target);
            }
        }
    }
}

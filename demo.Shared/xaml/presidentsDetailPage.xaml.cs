using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using demo.Data;
using Xamarin.Forms;
using demo.Models;

namespace demo.xaml
{
    public partial class presidentsDetailPage : ContentPage
    {
        public presidentsDetailPage(Models.presidents data)
        {
            InitializeComponent();

            bool insertMode = false;
            string saveButtonText = "Save";
            if (data == null)
            {
                data = new Models.presidents();
                insertMode = true;
            }

            this.BindingContext = data;
			
			this.FindByName<VisualElement>("id_column").IsEnabled = insertMode;
			if (data.portrait != null) {
				this.FindByName<Image>("portrait_column").Source = ImageSource.FromStream(() => { return new System.IO.MemoryStream(data.portrait); });
			}

            
			ToolbarItems.Add(new ToolbarItem()
            {
                Name = saveButtonText,
				Icon = (Device.OS == TargetPlatform.WinPhone) ? "save.png" : null,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() =>
                {
                    try
                    {
                        if (insertMode)
                            DependencyService.Get<IDataService>().Insert(data);
                        else
                            DependencyService.Get<IDataService>().Update(data);
                        this.Navigation.PopAsync();
                    }
                    catch (Exception e)
                    {
                        this.DisplayAlert("Exception", e.Message, "Ok");
                    }
                })
            });

            this.BindingContext = data;
        }
    }
}

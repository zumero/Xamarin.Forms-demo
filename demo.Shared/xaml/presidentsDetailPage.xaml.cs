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
			if (Device.RuntimePlatform == Device.UWP)
                this.Padding = new Xamarin.Forms.Thickness(this.Padding.Left, this.Padding.Top, this.Padding.Right, 95);
			
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
                Text = saveButtonText,
				IconImageSource = (Device.RuntimePlatform == Device.UWP) ? "save.png" : null,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(async () =>
                {
                    try
                    {
                        if (insertMode)
                            await DependencyService.Get<IDataService>().Insert(data);
                        else
                            await DependencyService.Get<IDataService>().Update(data);
                        await this.Navigation.PopAsync();
                    }
                    catch (Exception e)
                    {
                        await this.DisplayAlert("Exception", e.Message, "Ok");
                    }
                })
            });

            this.BindingContext = data;
        }
    }
}

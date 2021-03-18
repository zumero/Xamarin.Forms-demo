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
    public partial class chemical_elementsDetailPage : ContentPage
    {
        public chemical_elementsDetailPage(Models.chemical_elements data)
        {
            InitializeComponent();
			if (Device.RuntimePlatform == Device.UWP)
                this.Padding = new Xamarin.Forms.Thickness(this.Padding.Left, this.Padding.Top, this.Padding.Right, 95);
			
            bool insertMode = false;
            string saveButtonText = "Save";
            if (data == null)
            {
                data = new Models.chemical_elements();
                insertMode = true;
            }

            this.BindingContext = data;
			
			this.FindByName<VisualElement>("atomic_number_column").IsEnabled = insertMode;

            
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

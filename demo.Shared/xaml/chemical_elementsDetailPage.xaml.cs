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

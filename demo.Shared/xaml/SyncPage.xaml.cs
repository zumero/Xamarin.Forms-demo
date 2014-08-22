using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using demo.Data;
using demo.Views;
using demo.Models;
using Zumero;
using Xamarin.Forms;

namespace demo.xaml
{
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public partial class SyncPage : ContentPage
    {
        SyncParams _syncParams = null;

        public SyncParams Params
        {
            get { return _syncParams; }
            set { ; }
        }

        public SyncPage()
        {
            InitializeComponent();
			
            _syncParams = SyncParams.LoadSavedSyncParams();
            this.BindingContext = this;
        }

		protected override void OnAppearing()
        {
            base.OnAppearing();
            App.Current.SyncCompleted += SyncCompleted;
            App.Current.SyncFailed += SyncFailed;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            App.Current.SyncCompleted -= SyncCompleted;
            App.Current.SyncFailed -= SyncFailed;
        }
		
        private bool _syncInProgress = false;
        public bool IsSyncInProgress {
            get { return _syncInProgress; }
            set { _syncInProgress = value; OnPropertyChanged("IsSyncInProgress");  } 
        }
		
		void SyncCompleted(object sender, SyncParams e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!DependencyService.Get<IDataService>().VerifySchema())
                    this.DisplayAlert("Database Schema Changed", "The schema for the database has been changed since this application was generated.  This may cause failures if columns have been removed.  You may want to use Zumero Application Generator to recreate this app, based on the new schema.", "Ok");
                _syncParams.SaveSyncParam();
                this.IsSyncInProgress = false;
                this.Navigation.PushAsync(new TablesPage());
            });
        }

        void SyncFailed(object sender, Exception e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.DisplayAlert("Exception", e.Message, "Ok");
                this.IsSyncInProgress = false;
            });
        }

		public void OnSyncButtonClicked(object sender, EventArgs e)
        {
            Sync();
        }
			
        public void Sync()
        {
            this.IsSyncInProgress = true;
            DependencyService.Get<ISyncService>().StartBackgroundSync(_syncParams);
        }
    }
}

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
			if (Device.RuntimePlatform == Device.UWP)
                this.Padding = new Xamarin.Forms.Thickness(this.Padding.Left, this.Padding.Top, this.Padding.Right, 95);
			
            this.BindingContext = this;
        }

		protected override async void OnAppearing()
        {
            base.OnAppearing();
            _syncParams = await DependencyService.Get<IDataService>().LoadSyncParams();
            OnPropertyChanged(nameof(Params));
            ((demo.SharedApp)Xamarin.Forms.Application.Current).SyncCompleted += SyncCompleted;
            ((demo.SharedApp)Xamarin.Forms.Application.Current).SyncFailed += SyncFailed;
            ((demo.SharedApp)Xamarin.Forms.Application.Current).SyncProgress += SyncProgress;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ((demo.SharedApp)Xamarin.Forms.Application.Current).SyncCompleted -= SyncCompleted;
            ((demo.SharedApp)Xamarin.Forms.Application.Current).SyncFailed -= SyncFailed;
            ((demo.SharedApp)Xamarin.Forms.Application.Current).SyncProgress -= SyncProgress;
        }
		
        private bool _syncInProgress = false;
        public bool IsSyncInProgress {
            get { return _syncInProgress; }
            set { _syncInProgress = value; OnPropertyChanged("IsSyncInProgress");  } 
        }
		
		private string _syncProgressMessage = "";
		public string SyncProgressMessage
		{
			get { return _syncProgressMessage; }
			set { _syncProgressMessage = value; OnPropertyChanged("SyncProgressMessage"); }
		}
		
	private string _syncDescription = "";
        public string SyncDescription
        {
            get { return _syncDescription; }
            set {
                    _syncDescription = value;
                    OnPropertyChanged("SyncDescription");
            }
        }

        void SyncCompleted(object sender, SyncParams e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (!ZagDebugSchemaVersionCheck.VerifySchema())
                    await this.DisplayAlert("Database Schema Changed", "The schema for the database has been changed since this application was generated.  This may cause failures if columns have been removed.  You may want to use Zumero Application Generator to recreate this app, based on the new schema.", "Ok");
                await DependencyService.Get<IDataService>().SaveSyncParams(e);
                this.IsSyncInProgress = false;
				this.SyncDescription = e.SyncDescription;
            });
        }

        void SyncFailed(object sender, Exception e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.DisplayAlert("Exception", e.ToString(), "Ok");
                this.IsSyncInProgress = false;
            });
        }

		void SyncProgress(object sender, string message)
		{
			Device.BeginInvokeOnMainThread(() =>
            {
                SyncProgressMessage = message;
            });
		}
		
        public void OnSyncButtonClicked(object sender, EventArgs e)
        {
            Sync();
        }
			
        public async void Sync()
        {
            this.IsSyncInProgress = true;
            this.SyncDescription = _syncParams.SyncDescription = "";
            await DependencyService.Get<ISyncService>().Sync(_syncParams);
        }
		
		public void OnCancelButtonClicked(object sender, EventArgs e)
		{
			DependencyService.Get<ISyncService>().Cancel();
		}
    }
}

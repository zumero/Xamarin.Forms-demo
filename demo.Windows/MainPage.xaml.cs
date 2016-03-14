using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Cryptography;

using Xamarin.Forms;
using Windows.Storage.Streams;
using Windows.Security.Cryptography.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
[assembly: Dependency(typeof(demo.WinRT.SHA1Service))]
[assembly: Dependency(typeof(demo.Data.BaseSyncService))]
namespace demo.WinRT
{

    public class SHA1Service : demo.Data.ISHA1Service
    {
        public string HashString(string input)
        {
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            IBuffer hashBuffer = hashAlgorithm.HashData(buffer);

            var sb = new StringBuilder();
            for (uint i = 0; i < hashBuffer.Length; i++ )
            {
                var hex = hashBuffer.GetByte(i).ToString("X2");
                sb.Append(hex);
            }
            return sb.ToString(); ;
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage 
    {
        public MainPage()
        {
            this.InitializeComponent();
            var sqliteFilename = "data.db3";

            string documentsPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;// System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
            var path = Path.Combine(documentsPath, sqliteFilename);
            demo.App.DatabasePath = path;

            this.LayoutUpdated += MainPage_LayoutUpdated;
            LoadApplication(new demo.App());
        }

        void MainPage_LayoutUpdated(object sender, object e)
        {
            try
            {
                this.BottomAppBar.IsOpen = true;
                this.BottomAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                this.BottomAppBar.IsSticky = true;
            }
            catch (Exception ex) { }
        }
    }
}

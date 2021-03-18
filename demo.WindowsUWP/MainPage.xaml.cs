using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Forms;

[assembly: Dependency(typeof(demo.UWP.SHA1Service))]
[assembly: Dependency(typeof(demo.Data.BaseSyncService))]
namespace demo.UWP
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

    public sealed partial class MainPage 
    {
        public MainPage()
        {
            this.InitializeComponent();
            var sqliteFilename = "data.db3";

            string documentsPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;// System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
            var path = Path.Combine(documentsPath, sqliteFilename);
            demo.SharedApp.DatabasePath = path;

            LoadApplication(new demo.SharedApp());
        }
    }
}

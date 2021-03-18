using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Text;
using System.Security.Cryptography;
using Xamarin.Forms;
using System.IO;

[assembly: Dependency(typeof(demo.Droid.SHA1Service))]
namespace demo.Droid
{

	public class SHA1Service : demo.Data.ISHA1Service
    {
        public string HashString(string input)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(input);
            var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(jsonBytes);

            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                var hex = b.ToString("X2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }
    [Activity(Label = "demo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            var sqliteFilename = "data.db3";
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
            var path = Path.Combine(documentsPath, sqliteFilename);
            SharedApp.DatabasePath = path;

            LoadApplication(new SharedApp());
        }
    }
}


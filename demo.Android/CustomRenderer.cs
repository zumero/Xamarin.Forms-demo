using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(demo.xaml.SyncPage), typeof(demo.Droid.CustomRenderer))]
namespace demo.Droid
{
    //This customer renderer exists only to make certain that we cache the activity
    //hosting the syncPage.  That activity is then used to start the background
    //service.
    public class CustomRenderer : PageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            AndroidSyncService.SyncActivity = this.Context as Activity;
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            AndroidSyncService.SyncActivity = null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Webkit;

namespace Challonger
{
    [Activity(Label = "TournamentLiveImageActivity", Theme = "@android:style/Theme.NoTitleBar")]
    public class TournamentLiveImageActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);

            // Create your application here
            SetContentView(Resource.Layout.TournamentLiveImage);
            WebView webTournamentLiveImage = FindViewById<WebView>(Resource.Id.webTournamentLiveImage);

            string url = Intent.GetStringExtra("url") ?? "no_url";

            webTournamentLiveImage.LoadUrl(url);
            webTournamentLiveImage.Settings.BuiltInZoomControls = true;
        }
    }
}


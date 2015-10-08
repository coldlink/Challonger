
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
using System.Json;
using Android.Util;
using System.Net;
using System.IO;
using Android.Net;
using System.Collections;
using Android.Content.PM;

namespace Challonger
{
    [Activity(Label = "@string/searchResults", ScreenOrientation = ScreenOrientation.Portrait)]			
    public class TournamentListActivity : Activity
    {
        List<TournamentListInfo> listInfo = new List<TournamentListInfo>();
        List<string> listUrl = new List<string>();
        List<string> listtId = new List<string>();

        string url;
        bool flagSingle;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);

            // Create your application here
            SetContentView(Resource.Layout.TournamentList);
            url = Intent.GetStringExtra("url") ?? "no_url";
            flagSingle = Intent.GetBooleanExtra("flag", false);
        }

        protected override async void OnResume()
        {
            base.OnResume();

            TextView responseView = FindViewById<TextView>(Resource.Id.responseView);
            ListView linLay = FindViewById<ListView>(Resource.Id.listViewLayViewTournament);
            ProgressBar progTournamentListActivityIndicator = FindViewById<ProgressBar>(Resource.Id.progTournamentListActivityIndicator);
            progTournamentListActivityIndicator.Visibility = ViewStates.Visible;

            listInfo.Clear();
            listUrl.Clear();
            listtId.Clear();

            if (url == "FAV")
            {
                DisplayFavTournaments(responseView, linLay, progTournamentListActivityIndicator);
            }
            else
            {
                var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                //check for internet connection
                var activeConnection = connectivityManager.ActiveNetworkInfo;
                if ((activeConnection != null) && activeConnection.IsConnected)
                {
                    //check for web exceptions
                    try
                    {
                        JsonValue json = await Json.GetJson(url);
                        DisplayFoundTournaments(json, responseView, linLay, flagSingle);
                        progTournamentListActivityIndicator.Visibility = ViewStates.Gone;
                    }
                    catch (WebException we)
                    {
                        if (we.Status == WebExceptionStatus.ProtocolError)
                        {

                            var dlgbox = new AlertDialog.Builder(this);

                            switch (((HttpWebResponse)we.Response).StatusCode)
                            {
                                case (HttpStatusCode)401:
                                    dlgbox.SetMessage(Resource.String.dlg401);
                                    break;
                                case (HttpStatusCode)404:
                                    dlgbox.SetMessage(Resource.String.dlg404);
                                    break;
                                case (HttpStatusCode)406:
                                    dlgbox.SetMessage(Resource.String.dlg406);
                                    break;
                                case (HttpStatusCode)422:
                                    dlgbox.SetMessage(Resource.String.dlg422);
                                    break;
                                case (HttpStatusCode)500:
                                    dlgbox.SetMessage(Resource.String.dlg500);
                                    break;
                                default:
                                    dlgbox.SetMessage("This is a generic error. Contact " +
                                        "the developer with the following information.\nStatus Code: " + ((HttpWebResponse)we.Response).StatusCode.ToString()
                                        + "\nStatus Message: " + ((HttpWebResponse)we.Response).StatusDescription.ToString());
                                    break;
                            }
                            dlgbox.SetNegativeButton(Resource.String.ok, delegate
                                {
                                    this.Finish();
                                }).Show();
                        }
                    }
                }
                else
                {
                    var dlgerrNoConnection = new AlertDialog.Builder(this).SetMessage(Resource.String.errNoConnection).
                        SetNegativeButton(Resource.String.ok, delegate
                        {
                            this.Finish();
                        }).Show();
                }
            }
        }

        private async void DisplayFavTournaments(TextView responseView, ListView linLay, ProgressBar progTournamentListActivityIndicator)
        {
            var prefs = this.GetSharedPreferences("Challonger.preferences", FileCreationMode.Private);

            if (prefs.Contains("favs"))
            {
                ICollection<string> favs = prefs.GetStringSet("favs", null);

                if (favs.Count == 0)
                {
                    //no bookmarks found
                    var dlgerrNoFavs = new AlertDialog.Builder(this).SetMessage(Resource.String.errNoFavs).
                        SetNegativeButton(Resource.String.ok, delegate
                        {
                            this.Finish();
                        }).Show();
                }
                else
                {
                    //bookmarks found
                    responseView.Text = "Bookmarked tournaments:";
                    foreach (object fav in favs)
                    {
                        Console.Out.WriteLine("FROM PREFS: " + fav.ToString());
                        string favUrl = gVar.URL_ + "tournaments/" + fav + ".json?api_key=" + gVar.apiKey_;

                        var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                        //check for internet connection
                        var activeConnection = connectivityManager.ActiveNetworkInfo;
                        if ((activeConnection != null) && activeConnection.IsConnected)
                        {
                            //check for web exceptions
                            try
                            {
                                JsonValue json = await Json.GetJson(favUrl);
                                JsonValue tournament = json["tournament"];

                                TournamentListInfo info = new TournamentListInfo();
                                info.tName = tournament["name"];
                                info.tCount = tournament["participants_count"];
                                info.tType = tournament["tournament_type"];
                                info.tProgress = tournament["progress_meter"];

                                string state = tournament["state"];
                                switch (state)
                                {
                                    case "pending":
                                        info.tState = 0;
                                        break;
                                    case "underway":
                                        info.tState = 1;
                                        break;
                                    case "awaiting_review":
                                        info.tState = 2;
                                        break;
                                    case "complete":
                                        info.tState = 3;
                                        break;
                                }

                                string url = gVar.URL_ + "tournaments/";
                                if (tournament["subdomain"] != null)
                                {
                                    url += tournament["subdomain"] + "-" + tournament["url"] + ".json?" + "api_key=" + gVar.apiKey_ + "&include_participants=1&include_matches=1";
                                }
                                else
                                {
                                    url += tournament["url"] + ".json?" + "api_key=" + gVar.apiKey_ + "&include_participants=1&include_matches=1";
                                }

                                //disabled swiss type and 2 stage tournaments
                                if (!tournament["group_stages_enabled"] && tournament["tournament_type"] != "swiss")
                                {
                                    listInfo.Add(info);
                                    listUrl.Add(url);
                                    listtId.Add(tournament["id"].ToString());
                                }
                            }
                            catch (WebException we)
                            {
                                if (we.Status == WebExceptionStatus.ProtocolError)
                                {

                                    var dlgbox = new AlertDialog.Builder(this);

                                    switch (((HttpWebResponse)we.Response).StatusCode)
                                    {
                                        case (HttpStatusCode)401:
                                            dlgbox.SetMessage(Resource.String.dlg401).SetNegativeButton(Resource.String.ok, delegate
                                                {
                                                    this.Finish();
                                                }).Show();
                                            ;
                                            break;
                                        case (HttpStatusCode)404:
                                            break;
                                        case (HttpStatusCode)406:
                                            dlgbox.SetMessage(Resource.String.dlg406).SetNegativeButton(Resource.String.ok, delegate
                                                {
                                                    this.Finish();
                                                }).Show();
                                            ;
                                            break;
                                        case (HttpStatusCode)422:
                                            dlgbox.SetMessage(Resource.String.dlg422).SetNegativeButton(Resource.String.ok, delegate
                                                {
                                                    this.Finish();
                                                }).Show();
                                            ;
                                            break;
                                        case (HttpStatusCode)500:
                                            dlgbox.SetMessage(Resource.String.dlg500).SetNegativeButton(Resource.String.ok, delegate
                                                {
                                                    this.Finish();
                                                }).Show();
                                            ;
                                            break;
                                        default:
                                            dlgbox.SetMessage("This is a generic error. Contact " +
                                                "the developer with the following information.\nStatus Code: " + ((HttpWebResponse)we.Response).StatusCode.ToString()
                                                + "\nStatus Message: " + ((HttpWebResponse)we.Response).StatusDescription.ToString());
                                            break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var dlgerrNoConnection = new AlertDialog.Builder(this).SetMessage(Resource.String.errNoConnection).
                                SetNegativeButton(Resource.String.ok, delegate
                                {
                                    this.Finish();
                                }).Show();
                        }
                    }
                   
                    linLay.Adapter = new TournamentListInfoListAdapter(this, listInfo);
                    linLay.ItemClick -= LinLay_ItemClick;
                    linLay.ItemLongClick -= LinLay_ItemLongClick;
                    linLay.ItemClick += LinLay_ItemClick;
                    linLay.ItemLongClick += LinLay_ItemLongClick;

                    progTournamentListActivityIndicator.Visibility = ViewStates.Gone;
                }    
            }
            else
            {
                //no bookmarks exist
                var dlgerrNoFavs = new AlertDialog.Builder(this).SetMessage(Resource.String.errNoFavs).
                    SetNegativeButton(Resource.String.ok, delegate
                    {
                        this.Finish();
                    }).Show();
            }
        }

        private void DisplayFoundTournaments(JsonValue json, TextView responseView, ListView linLay, bool flagSingle)
        {
            int count = json.Count;
            responseView.Text = count + " " + this.GetString(Resource.String.searchFound);

            for (int i = 0; i < count; i++)
            {
                JsonValue tournament;

                if (flagSingle)
                    tournament = json["tournament"];
                else
                    tournament = json[i]["tournament"];

                TournamentListInfo info = new TournamentListInfo();
                info.tName = tournament["name"];
                info.tCount = tournament["participants_count"];
                info.tType = tournament["tournament_type"];
                info.tProgress = tournament["progress_meter"];

                string state = tournament["state"];
                switch (state)
                {
                    case "pending":
                        info.tState = 0;
                        break;
                    case "underway":
                        info.tState = 1;
                        break;
                    case "awaiting_review":
                        info.tState = 2;
                        break;
                    case "complete":
                        info.tState = 3;
                        break;
                }

                string url = gVar.URL_ + "tournaments/";
                if (tournament["subdomain"] != null)
                {
                    url += tournament["subdomain"] + "-" + tournament["url"] + ".json?" + "api_key=" + gVar.apiKey_ + "&include_participants=1&include_matches=1";
                }
                else
                {
                    url += tournament["url"] + ".json?" + "api_key=" + gVar.apiKey_ + "&include_participants=1&include_matches=1";
                }

                //disabled swiss type and 2 stage tournaments
                if (!tournament["group_stages_enabled"] && tournament["tournament_type"] != "swiss")
                {
                    listInfo.Add(info);
                    listUrl.Add(url);
                    listtId.Add(tournament["id"].ToString());
                }
            }
                
            linLay.Adapter = new TournamentListInfoListAdapter(this, listInfo);
            linLay.ItemClick -= LinLay_ItemClick;
            linLay.ItemLongClick -= LinLay_ItemLongClick;
            linLay.ItemClick += LinLay_ItemClick;
            linLay.ItemLongClick += LinLay_ItemLongClick;
        }

        void LinLay_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var intent = new Intent(this, typeof(ViewTournamentInfoActivity));

            intent.PutExtra("url", listUrl[e.Position]);
            gVar.current_tId = listtId[e.Position];
            gVar.boolTournamentEditModeEnabled = false;
            gVar.lastViewTournamentInfoTabSelected = 0;
            StartActivity(intent);
        }

        void LinLay_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var prefs = this.GetSharedPreferences("Challonger.preferences", FileCreationMode.Private);
            var editor = prefs.Edit();

            if (prefs.Contains("favs"))
            {
                ICollection<string> favs = prefs.GetStringSet("favs", null);
                if (favs.Contains(listtId[e.Position]))
                {
                    AlertDialog.Builder dlgRemFav = new AlertDialog.Builder(this);
                    dlgRemFav.SetPositiveButton("Remove from bookmarks", delegate
                        {
                            favs.Remove(listtId[e.Position]);
                            editor.PutStringSet("favs", favs).Commit();
                            return;
                        });
                    dlgRemFav.SetNegativeButton(this.GetString(Resource.String.cancel), delegate
                        {
                            return;
                        });
                    dlgRemFav.Show();
                }
                else
                {
                    AlertDialog.Builder dlgAddFav = new AlertDialog.Builder(this);
                    dlgAddFav.SetPositiveButton("Add to bookmarks", delegate
                        {
                            favs.Add(listtId[e.Position]);
                            editor.PutStringSet("favs", favs).Commit();
                            return;
                        });
                    dlgAddFav.SetNegativeButton(this.GetString(Resource.String.cancel), delegate
                        {
                            return;
                        });
                    dlgAddFav.Show();
                }
            }
            else
            {
                ICollection<string> favs = new List<string>();
                AlertDialog.Builder dlgAddFav = new AlertDialog.Builder(this);
                dlgAddFav.SetPositiveButton("Add to bookmarks", delegate
                    {
                        favs.Add(listtId[e.Position]);
                        editor.PutStringSet("favs", favs).Commit();
                        return;
                    });
                dlgAddFav.SetNegativeButton(this.GetString(Resource.String.cancel), delegate
                    {
                        return;
                    });
                dlgAddFav.Show();
            }
        }
    }
}


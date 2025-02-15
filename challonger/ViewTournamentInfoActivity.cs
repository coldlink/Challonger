﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Json;
using System.Net;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Runtime.InteropServices;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Net;
using Android.Provider;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.Database.Sqlite;
using Android.Hardware;
using Android.Text;
using System.Threading.Tasks;
using Android.Nfc.Tech;
using System.Globalization;
using Dalvik.SystemInterop;
using Java.Util;

namespace Challonger
{
    [Activity(Label = "", ScreenOrientation = ScreenOrientation.Portrait)]			
    public class ViewTournamentInfoActivity : Activity
    {
        List<TournamentInfo> itemsInfo = new List<TournamentInfo>();
        List<MatchInfo> itemsMatches = new List<MatchInfo>();
        List<ParticipantInfo> itemsParticipants = new List<ParticipantInfo>();

        string url;
        string tId;

        JsonValue json = null;
        JsonValue jsonTournament = null;
        JsonValue jsonMatches = null;
        JsonValue jsonParticipants = null;

        ActionBar.Tab tab1;

        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);

            // Create your application here
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            SetContentView(Resource.Layout.ViewTournamentInfo);
            ListView listViewLayViewTournamentInfo = FindViewById<ListView>(Resource.Id.listViewLayViewTournamentInfo);
            if (listViewLayViewTournamentInfo.ChildCount != 0)
                listViewLayViewTournamentInfo.RemoveAllViews();

            url = Intent.GetStringExtra("url") ?? "";
            //check for internet connection
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var activeConnection = connectivityManager.ActiveNetworkInfo;
            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                //check for web exceptions
                try
                {
                    JsonValue jsont = await Json.GetJson(url);
                    JsonValue jsonTournamentt = jsont["tournament"];
                    JsonValue jsonMatchest = jsonTournamentt["matches"];
                    JsonValue jsonParticipantst = jsonTournamentt["participants"];

                    json = jsont;
                    jsonTournament = jsonTournamentt;
                    jsonMatches = jsonMatchest;
                    jsonParticipants = jsonParticipantst;
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

            this.Title = jsonTournament["name"];

            //populate arrays/lists
            PopulateItemsInfo();
            PopulateParticipantsInfo();
            PopulateMatchInfo();

            //add tabs
            AddTab(Resource.String.viewTournamentInfo_Info);
            AddTab(Resource.String.viewTournamentInfo_Matches);
            AddTab(Resource.String.viewTournamentInfo_Participants);

            if (jsonTournament["state"] == "complete")
            {
                AddTab(Resource.String.viewTournamentInfo_FinalRank);
                gVar.lastViewTournamentInfoTabSelected = 0;
            }
				
            ActionBar.SetSelectedNavigationItem(gVar.lastViewTournamentInfoTabSelected);
        }

        //create menu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            IMenu _menu = menu;
            MenuInflater.Inflate(Resource.Menu.ViewTournamentInfoMenu, _menu);
            if (gVar.boolTournamentEditModeEnabled)
                _menu.FindItem(Resource.Id.ViewTournamentInfoMenuEdit).SetChecked(true);
            else
                _menu.FindItem(Resource.Id.ViewTournamentInfoMenuEdit).SetChecked(false);

            //add bookmark enabled/disabled code
            var prefs = this.GetSharedPreferences("Challonger.preferences", FileCreationMode.Private);
            if (prefs.Contains("favs"))
            {
                ICollection<string> favs = prefs.GetStringSet("favs", null);
                if (favs.Contains(gVar.current_tId))
                    _menu.FindItem(Resource.Id.ViewTournamentInfoMenuFav).SetChecked(true);
                else
                    _menu.FindItem(Resource.Id.ViewTournamentInfoMenuFav).SetChecked(false);
            }

            return base.OnCreateOptionsMenu(_menu);
        }

        //menu items handler
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.ViewTournamentInfoMenuRefresh:
                    gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                    this.Recreate();
                    return true;
                case Resource.Id.ViewTournamentInfoMenuEdit:
                    if (item.IsChecked)
                    {
                        gVar.boolTournamentEditModeEnabled = false;
                        Toast.MakeText(this, this.GetString(Resource.String.tstEditDisabled), ToastLength.Long).Show();
                        item.SetChecked(false);
                    }
                    else
                    {
                        try
                        {
                            CheckAPIPermission();
                            item.SetChecked(true);
                        }
                        catch (WebException we)
                        {
                            WebExHandler(we);
                        }
                    }
                    return true;
                case Resource.Id.ViewTournamentInfoMenuAddParticipant:
                    if (!gVar.boolTournamentEditModeEnabled)
                        Toast.MakeText(this, this.GetString(Resource.String.tstAddParticipantEditDisabled), ToastLength.Long).Show();
                    else if (jsonTournament["state"] != "pending")
                        Toast.MakeText(this, this.GetString(Resource.String.tstAddParticipantNotPending), ToastLength.Long).Show();
                    else if (jsonTournament["signup_cap"] != null)
                    {
                        if (jsonTournament["signup_cap"].ToString() == jsonTournament["participants_count"].ToString())
                            Toast.MakeText(this, this.GetString(Resource.String.tstParticipantCap), ToastLength.Long).Show();
                        else
                        {
                            var dialog2 = AddParticipantInfoDialog.Initalize(jsonTournament, this);
                            dialog2.Show(FragmentManager, "dialog");
                        }
                    }
                    else
                    {
                        var dialog2 = AddParticipantInfoDialog.Initalize(jsonTournament, this);
                        dialog2.Show(FragmentManager, "dialog");
                    }
                    return true;
                case Resource.Id.ViewTournamentInfoMenuFav:
                    //add bookmarking code
                    var prefs = this.GetSharedPreferences("Challonger.preferences", FileCreationMode.Private);
                    var editor = prefs.Edit();
                    if (item.IsChecked)
                    {
                        if (prefs.Contains("favs"))
                        {
                            ICollection<string> favs = prefs.GetStringSet("favs", null);
                            foreach (object fav in favs)
                                Console.Out.WriteLine("FROM PREFS: " + fav.ToString());
                            favs.Remove(jsonTournament["id"].ToString());
                            editor.PutStringSet("favs", favs).Commit();
                            foreach (object fav in favs)
                                Console.Out.WriteLine("FROM PREFS AFTER: " + fav.ToString());

                            item.SetChecked(false);
                        }
                    }
                    else
                    {
                        if (prefs.Contains("favs"))
                        {
                            ICollection<string> favs = prefs.GetStringSet("favs", null);
                            foreach (object fav in favs)
                                Console.Out.WriteLine("FROM PREFS: " + fav.ToString());
                            favs.Add(jsonTournament["id"].ToString());
                            editor.PutStringSet("favs", favs).Commit();
                            foreach (object fav in favs)
                                Console.Out.WriteLine("FROM PREFS AFTER: " + fav.ToString());

                            item.SetChecked(true);
                        }
                        else
                        {
                            ICollection<string> favs = new List<string>();
                            favs.Add(jsonTournament["id"].ToString());
                            foreach (object fav in favs)
                                Console.Out.WriteLine("TO PREFS: " + fav.ToString());

                            editor.PutStringSet("favs", favs).Commit();

                            item.SetChecked(true);
                        }
                    }
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        //add tab to actionbar
        void AddTab(int resourceID)
        {
            ActionBar.Tab tab = ActionBar.NewTab().SetText(resourceID);
            tab.TabSelected += TabOnTabSelected;
            ActionBar.AddTab(tab);
        }

        //tab selected handler
        void TabOnTabSelected(object sender, ActionBar.TabEventArgs tabEventArgs)
        {
            tab1 = (ActionBar.Tab)sender;
            ListView listViewLayViewTournamentInfo = FindViewById<ListView>(Resource.Id.listViewLayViewTournamentInfo);

            switch (tab1.Position)
            {
                case 0:
                    listViewLayViewTournamentInfo.Adapter = new TournamentInfoListAdapter(this, itemsInfo);
                    listViewLayViewTournamentInfo.ItemClick -= ListViewLayViewTournamentInfo_ItemClick;
                    listViewLayViewTournamentInfo.ItemLongClick -= ListViewLayViewTournamentInfo_ItemLongClick;
                    listViewLayViewTournamentInfo.ItemClick += ListViewLayViewTournamentInfo_ItemClick;
                    listViewLayViewTournamentInfo.ItemLongClick += ListViewLayViewTournamentInfo_ItemLongClick;
                    break;
                case 1:
                    listViewLayViewTournamentInfo.Adapter = new MatchInfoListAdapter(this, itemsMatches);
                    listViewLayViewTournamentInfo.ItemClick -= ListViewLayViewTournamentInfo_ItemClick;
                    listViewLayViewTournamentInfo.ItemLongClick -= ListViewLayViewTournamentInfo_ItemLongClick;
                    listViewLayViewTournamentInfo.ItemClick += ListViewLayViewTournamentInfo_ItemClick;
                    listViewLayViewTournamentInfo.ItemLongClick += ListViewLayViewTournamentInfo_ItemLongClick;
                    gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                    break;
                case 2:
                    listViewLayViewTournamentInfo.Adapter = new ParticipantInfoListAdapter(this, itemsParticipants);
                    listViewLayViewTournamentInfo.ItemLongClick -= ListViewLayViewTournamentInfo_ItemLongClick;
                    listViewLayViewTournamentInfo.ItemClick += ListViewLayViewTournamentInfo_ItemClick;
                    listViewLayViewTournamentInfo.ItemLongClick += ListViewLayViewTournamentInfo_ItemLongClick;
                    gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                    break;
                case 3:
                    listViewLayViewTournamentInfo.Adapter = new FinalRankListAdapter(this, itemsParticipants);
                    listViewLayViewTournamentInfo.ItemClick -= ListViewLayViewTournamentInfo_ItemClick;
                    listViewLayViewTournamentInfo.ItemLongClick -= ListViewLayViewTournamentInfo_ItemLongClick;
                    listViewLayViewTournamentInfo.ItemClick += ListViewLayViewTournamentInfo_ItemClick;
                    listViewLayViewTournamentInfo.ItemLongClick += ListViewLayViewTournamentInfo_ItemLongClick;
                    gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                    break;
            }
        }

        //list item click handlers
        void ListViewLayViewTournamentInfo_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            if (gVar.boolTournamentEditModeEnabled)
            {
                switch (tab1.Position)
                {
                    case 0:
                        switch (e.Position)
                        {
                            case 0:
                                AlertDialog.Builder dlg0 = new AlertDialog.Builder(this);
                                dlg0.SetTitle(this.GetString(Resource.String.editTournamentName));
                                dlg0.SetMessage(this.GetString(Resource.String.editTournamentNameMsg));
                                EditText name0 = new EditText(this);
                                name0.Text = jsonTournament["name"];
                                dlg0.SetView(name0);
                                dlg0.SetPositiveButton(this.GetString(Resource.String.ok), delegate
                                    {/*(object sender0, DialogClickEventArgs e0)*/
                                        string jsonPut = "{\"tournament\":{\"name\":\"" + name0.Text + "\"}}";
                                        gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                                        dlgEditTournamentInfo(jsonPut);
                                        return;
                                    });
                                dlg0.SetNegativeButton(this.GetString(Resource.String.cancel), delegate
                                    {
                                        return;
                                    });
                                dlg0.Show();
                                break;
                            case 1:
                                var dialog = EditTournamentStateDialog.Initalize(jsonTournament, this);
                                gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                                dialog.Show(FragmentManager, "dialog");
                                break;
                            case 2:
                                AlertDialog.Builder dlg1 = new AlertDialog.Builder(this);
                                dlg1.SetTitle(this.GetString(Resource.String.editTournamentGame));
                                dlg1.SetMessage(this.GetString(Resource.String.editTournamentGameMsg));
                                EditText name1 = new EditText(this);
                                name1.Text = jsonTournament["game_name"];
                                dlg1.SetView(name1);
                                dlg1.SetPositiveButton(this.GetString(Resource.String.ok), delegate
                                    {/*(object sender0, DialogClickEventArgs e0)*/
                                        string jsonPut = "{\"tournament\":{\"game_name\":\"" + name1.Text + "\"}}";
                                        gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                                        dlgEditTournamentInfo(jsonPut);
                                        return;
                                    });
                                dlg1.SetNegativeButton(this.GetString(Resource.String.cancel), delegate
                                    {
                                        return;
                                    });
                                dlg1.Show();
                                break;
                            case 3:
                                if (jsonTournament["state"] != "pending")
                                    Toast.MakeText(this, Resource.String.tstEditType, ToastLength.Long).Show();
                                else
                                {
                                    var dialog2 = EditTournamentTypeDialog.Initalize(jsonTournament, this);
                                    gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                                    dialog2.Show(FragmentManager, "dialog");
                                }
                                break;
                            case 4:
                                AlertDialog.Builder dlg3 = new AlertDialog.Builder(this);
                                dlg3.SetTitle(this.GetString(Resource.String.editTournamentDescription));
                                dlg3.SetMessage(this.GetString(Resource.String.editTournamentDescriptionMsg));
                                EditText name3 = new EditText(this);
                                name3.Text = jsonTournament["description"];
                                dlg3.SetView(name3);
                                dlg3.SetPositiveButton(this.GetString(Resource.String.ok), delegate
                                    {/*(object sender0, DialogClickEventArgs e0)*/
                                        string jsonPut = "{\"tournament\":{\"description\":\"" + name3.Text + "\"}}";
                                        gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                                        dlgEditTournamentInfo(jsonPut);
                                        return;
                                    });
                                dlg3.SetNegativeButton(this.GetString(Resource.String.cancel), delegate
                                    {
                                        return;
                                    });
                                dlg3.Show();
                                break;
                            case 5:
                                Toast.MakeText(this, Resource.String.tstTournamentOrganisation, ToastLength.Long).Show();
                                break;
                            case 6:
                                ActionBar.SetSelectedNavigationItem(2);
                                break;
                            case 7:
                                Toast.MakeText(this, Resource.String.tstTournamentOrganisation, ToastLength.Long).Show();
                                break;
                            case 9:
                                if (jsonTournament["state"] == "pending")
                                {
                                    AlertDialog.Builder dlg9 = new AlertDialog.Builder(this);
                                    dlg9.SetTitle(this.GetString(Resource.String.editTournamentSignup));
                                    CheckBox chk9 = new CheckBox(this);
                                    chk9.Text = this.GetString(Resource.String.editTournamentSignupMsg);
                                    if (jsonTournament["open_signup"] == true)
                                        chk9.Checked = true;
                                    else
                                        chk9.Checked = false;
                                    dlg9.SetView(chk9);
                                    dlg9.SetPositiveButton(this.GetString(Resource.String.ok), delegate
                                        {
                                            string jsonPut;
                                            if (chk9.Checked)
                                                jsonPut = "{\"tournament\":{\"open_signup\": true}}";
                                            else
                                                jsonPut = "{\"tournament\":{\"open_signup\": false}}";

                                            dlgEditTournamentInfo(jsonPut);
                                            return;
                                        });
                                    dlg9.SetNegativeButton(this.GetString(Resource.String.cancel), delegate
                                        {
                                            return;
                                        });
                                    dlg9.Show();
                                }
                                else
                                    Toast.MakeText(this, Resource.String.tstTournamentSignup, ToastLength.Long);
                                break;
                            case 11:
                                AlertDialog.Builder dlg11 = new AlertDialog.Builder(this);
                                dlg11.SetTitle(this.GetString(Resource.String.editTournamentCap));
                                dlg11.SetMessage(this.GetString(Resource.String.editTournamentCapMsg));
                                EditText name11 = new EditText(this);
                                name11.InputType = InputTypes.ClassNumber;
                                if (jsonTournament["signup_cap"] != null)
                                    name11.Text = jsonTournament["signup_cap"].ToString();
                                else
                                    name11.Text = "";
                                dlg11.SetView(name11);
                                dlg11.SetPositiveButton(this.GetString(Resource.String.ok), delegate
                                    {
                                        string jsonPut;
                                        if (name11.Text != "" && name11.Text != "0")
                                            jsonPut = "{\"tournament\":{\"signup_cap\": " + name11.Text + "}}";
                                        else
                                            jsonPut = "{\"tournament\":{\"signup_cap\": null}}";

                                        gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                                        dlgEditTournamentInfo(jsonPut);
                                        return;
                                    });
                                dlg11.SetNegativeButton(this.GetString(Resource.String.cancel), delegate
                                    {
                                        return;
                                    });
                                dlg11.Show();
                                break;
                        }
                        break;
                    case 2:
                        if (jsonTournament["state"] == "pending")
                        {
                            var dialog2 = EditParticipantInfoDialog.Initalize(itemsParticipants[e.Position], jsonTournament, this);
                            gVar.lastViewTournamentInfoTabSelected = ActionBar.SelectedNavigationIndex;
                            dialog2.Show(FragmentManager, "dialog");
                        }
                        break;
                }
            }
        }

        void ListViewLayViewTournamentInfo_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            switch (tab1.Position)
            {
                case 0:
                    switch (e.Position)
                    {
                        case 5:
                            if (jsonTournament["subdomain"] != null)
                            {
                                var intent4 = new Intent(this, typeof(TournamentListActivity));
                                string url = gVar.URL_ + "tournaments.json?" + "api_key=" + gVar.apiKey_ + "&subdomain=" + jsonTournament["subdomain"];
                                intent4.PutExtra("url", url);
                                StartActivity(intent4);
                            }
                            break;
                        case 7:
                            var uri = Android.Net.Uri.Parse(jsonTournament["full_challonge_url"]);
                            var intent6 = new Intent(Intent.ActionView, uri);
                            StartActivity(intent6);
                            break;
                        case 8:
                            var intent7 = new Intent(this, typeof(TournamentLiveImageActivity));

                            string iurl = "http://images.challonge.com/";
                            if (jsonTournament["subdomain"] != null)
                                iurl += jsonTournament["subdomain"] + "-" + jsonTournament["url"] + ".png?";
                            else
                                iurl += jsonTournament["url"] + ".png";

                            intent7.PutExtra("url", iurl);
                            StartActivity(intent7);
                            break;
                        case 9:
                            if (jsonTournament["sign_up_url"] != null)
                            {    
                                var uri9 = Android.Net.Uri.Parse(jsonTournament["sign_up_url"]);
                                var intent9 = new Intent(Intent.ActionView, uri9);
                                StartActivity(intent9);
                            }
                            else
                                Toast.MakeText(this, Resource.String.tstNoSignUpUrl, ToastLength.Long).Show();
                            break;
                    }
                    break;
            }
        }

        void PopulateItemsInfo()
        {
            itemsInfo.Add(new TournamentInfo(jsonTournament["name"], this.GetString(Resource.String.viewTournamentInfo_Info_name)));

            string state = jsonTournament["state"];
            itemsInfo.Add(new TournamentInfo(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(state), this.GetString(Resource.String.viewTournamentInfo_State)));

            if (jsonTournament["game_name"] == null)
                itemsInfo.Add(new TournamentInfo("N/A", this.GetString(Resource.String.viewTournamentInfo_Info_gameName)));
            else
                itemsInfo.Add(new TournamentInfo(jsonTournament["game_name"], this.GetString(Resource.String.viewTournamentInfo_Info_gameName)));

            string type = jsonTournament["tournament_type"];
            itemsInfo.Add(new TournamentInfo(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(type), this.GetString(Resource.String.viewTournamentInfo_Info_type)));

            if (jsonTournament["description"] != "")
                itemsInfo.Add(new TournamentInfo(jsonTournament["description"], this.GetString(Resource.String.viewTournamentInfo_Info_description)));
            else
                itemsInfo.Add(new TournamentInfo("N/A", this.GetString(Resource.String.viewTournamentInfo_Info_description)));

            if (jsonTournament["subdomain"] == null)
                itemsInfo.Add(new TournamentInfo("N/A", this.GetString(Resource.String.viewTournamentInfo_Info_subdomain)));
            else
                itemsInfo.Add(new TournamentInfo(jsonTournament["subdomain"], this.GetString(Resource.String.viewTournamentInfo_Info_subdomain)));

            itemsInfo.Add(new TournamentInfo(jsonTournament["participants_count"].ToString(), this.GetString(Resource.String.viewTournamentInfo_Info_participantCount)));
            itemsInfo.Add(new TournamentInfo(jsonTournament["full_challonge_url"], this.GetString(Resource.String.viewTournamentInfo_Info_fullURL)));
            itemsInfo.Add(new TournamentInfo(this.GetString(Resource.String.viewTournamentInfo_LiveImage), this.GetString(Resource.String.viewTournamentInfo_LiveImageClick)));


            if (jsonTournament["sign_up_url"] != null)
                itemsInfo.Add(new TournamentInfo(jsonTournament["sign_up_url"], this.GetString(Resource.String.viewTournamentInfo_Signup)));
            else
                itemsInfo.Add(new TournamentInfo("N/A", this.GetString(Resource.String.viewTournamentInfo_Signup)));

            if (jsonTournament["hold_third_place_match"] && jsonTournament["tournament_type"] == "single elimination")
                itemsInfo.Add(new TournamentInfo(this.GetString(Resource.String.True), this.GetString(Resource.String.viewTournamentInfo_3rdPlace)));
            else if (!jsonTournament["hold_third_place_match"] && jsonTournament["tournament_type"] == "single elimination")
                itemsInfo.Add(new TournamentInfo(this.GetString(Resource.String.False), this.GetString(Resource.String.viewTournamentInfo_3rdPlace)));
            else
                itemsInfo.Add(new TournamentInfo("N/A", this.GetString(Resource.String.viewTournamentInfo_3rdPlace)));

            if (jsonTournament["signup_cap"] != null)
                itemsInfo.Add(new TournamentInfo(jsonTournament["signup_cap"].ToString(), this.GetString(Resource.String.viewTournamentInfoCap)));
            else
                itemsInfo.Add(new TournamentInfo("N/A", this.GetString(Resource.String.viewTournamentInfoCap)));
            
        }

        void PopulateMatchInfo()
        {
            gVar.dictMIdIdent = new Dictionary<int, string>();

            for (int i = 0; i < jsonMatches.Count; i++)
            {
                string score = jsonMatches[i]["match"]["scores_csv"];
                int p1score = 0;
                int p2score = 0;
                if (score == "" || score == null)
                {
                    p1score = 0;
                    p2score = 0;
                }
                else
                {
                    if (score.Contains(','))
                    {
                        string[] scores = score.Split(',');

                        for (int j = 0; j < scores.Length; j++)
                        {
                            string[] scores1 = scores[j].Split('-');
                            int temp1;
                            int temp2;

                            int.TryParse(scores1[0], out temp1);
                            int.TryParse(scores1[1], out temp2);

                            if (temp1 > temp2)
                                p1score++;
                            else
                                p2score++;
                        }
                    }
                    else
                    {
                        string[] scores1 = score.Split('-');
                        int.TryParse(scores1[0], out p1score);
                        int.TryParse(scores1[1], out p2score);
                    }
                }

                MatchInfo _info = new MatchInfo();
                _info.id = jsonMatches[i]["match"]["id"];
                _info.state = jsonMatches[i]["match"]["state"];

                string state = jsonMatches[i]["match"]["state"];
                switch (state)
                {
                    case "open":
                        _info.stateId = 0;
                        break;
                    case "pending":
                        _info.stateId = 1;
                        break;
                    case "complete":
                        _info.stateId = 2;
                        break;
                }

                _info.ident = jsonMatches[i]["match"]["identifier"];

                if (jsonTournament["subdomain"] == null)
                    _info.subdomain = "";
                else
                    _info.subdomain = jsonTournament["subdomain"];

                _info.url = jsonTournament["url"];

                if (jsonMatches[i]["match"]["player1_id"] != null)
                {
                    _info.p1 = gVar.dictIdName[jsonMatches[i]["match"]["player1_id"]];
                    _info.p1id = jsonMatches[i]["match"]["player1_id"];
                }
                else
                {
                    _info.p1 = "";
                    _info.p1id = -1;
                }

                if (jsonMatches[i]["match"]["player2_id"] != null)
                {
                    _info.p2 = gVar.dictIdName[jsonMatches[i]["match"]["player2_id"]];
                    _info.p2id = jsonMatches[i]["match"]["player2_id"];
                }
                else
                {
                    _info.p2 = "";
                    _info.p2id = -1;
                }


                _info.round = jsonMatches[i]["match"]["round"];
                _info.p1s = p1score;
                _info.p2s = p2score;

                if (jsonMatches[i]["match"]["player1_prereq_match_id"] != null)
                    _info.p1prematchid = jsonMatches[i]["match"]["player1_prereq_match_id"];
                else
                    _info.p1prematchid = -1;


                _info.p1isprematchloser = jsonMatches[i]["match"]["player1_is_prereq_match_loser"];

                if (jsonMatches[i]["match"]["player2_prereq_match_id"] != null)
                    _info.p2prematchid = jsonMatches[i]["match"]["player2_prereq_match_id"];
                else
                    _info.p2prematchid = -1;

                _info.p2isprematchloser = jsonMatches[i]["match"]["player2_is_prereq_match_loser"];

                if (jsonMatches[i]["match"]["winner_id"] != null)
                    _info.winId = jsonMatches[i]["match"]["winner_id"];
                else
                    _info.winId = -1;

                if (jsonMatches[i]["match"]["loser_id"] != null)
                    _info.losId = jsonMatches[i]["match"]["loser_id"];
                else
                    _info.losId = -1;

                itemsMatches.Add(_info);

                gVar.dictMIdIdent.Add(jsonMatches[i]["match"]["id"], jsonMatches[i]["match"]["identifier"]);
            }
        }

        void PopulateParticipantsInfo()
        {
            gVar.dictIdName = new Dictionary<int, string>();
            for (int i = 0; i < jsonParticipants.Count; i++)
            {

                ParticipantInfo _info = new ParticipantInfo();

                _info.displayName = jsonParticipants[i]["participant"]["display_name"];
                _info.userName = jsonParticipants[i]["participant"]["challonge_username"];
                _info.seed = jsonParticipants[i]["participant"]["seed"];
                _info.imgURL = jsonParticipants[i]["participant"]["attached_participatable_portrait_url"];
                _info.id = jsonParticipants[i]["participant"]["id"];

                if (jsonTournament["state"] == "complete")
                    _info.rank = jsonParticipants[i]["participant"]["final_rank"];
                else
                    _info.rank = -1;

                itemsParticipants.Add(_info);

                gVar.dictIdName.Add(jsonParticipants[i]["participant"]["id"], jsonParticipants[i]["participant"]["display_name"]);
            }
        }

        async void dlgEditTournamentInfo(string jsonPut)
        {
            string puturl = gVar.URL_;

            if (jsonTournament["subdomain"] == null)
                puturl += "tournaments/" + jsonTournament["url"] + ".json?api_key=" + gVar.apiKey_;
            else
                puturl += "tournaments/" + jsonTournament["subdomain"] + "-" + jsonTournament["url"] + ".json?api_key=" + gVar.apiKey_;

            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            //check for internet connection
            var activeConnection = connectivityManager.ActiveNetworkInfo;
            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                //check for web exceptions
                try
                {
                    await Json.PutJson(puturl, jsonPut);
                    Toast.MakeText(this, this.GetString(Resource.String.editTournamentTstSuccess), ToastLength.Long).Show();
                    this.Recreate();
                }
                catch (WebException we)
                {
                    WebExHandlerEdit(we);
                }
            }
            else
            {
                var dlgerrNoConnection = new AlertDialog.Builder(this).SetMessage(Resource.String.errNoConnection).
					SetNegativeButton(Resource.String.ok, delegate
                    {
                        return;
                    }).Show();
            }
        }

        async void CheckAPIPermission()
        {
            string jsonPut = "{\"tournament\":{\"name\":\"" + jsonTournament["name"] + "\"}}";
            string puturl = gVar.URL_;
            if (jsonTournament["subdomain"] == null)
                puturl += "tournaments/" + jsonTournament["url"] + ".json?api_key=" + gVar.apiKey_;
            else
                puturl += "tournaments/" + jsonTournament["subdomain"] + "-" + jsonTournament["url"] + ".json?api_key=" + gVar.apiKey_;

            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);

            //check for internet connection
            var activeConnection = connectivityManager.ActiveNetworkInfo;
            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                //check for web exceptions
                try
                {
                    await Json.PutJson(puturl, jsonPut);
                    gVar.boolTournamentEditModeEnabled = true;
                    Toast.MakeText(this, this.GetString(Resource.String.tstEditEnabled), ToastLength.Long).Show();
                }
                catch (WebException we)
                {
                    WebExHandlerEdit(we);
                }
            }
            else
            {
                var dlgerrNoConnection = new AlertDialog.Builder(this).SetMessage(Resource.String.errNoConnection).
					SetNegativeButton(Resource.String.ok, delegate
                    {
                        return;
                    }).Show();
            }
        }

        async void dlgEditTournamentInfo(string jsonPut, string puturl)
        {
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            //check for internet connection
            var activeConnection = connectivityManager.ActiveNetworkInfo;
            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                //check for web exceptions
                try
                {
                    await Json.PutJson(puturl, jsonPut);
                    Toast.MakeText(this, this.GetString(Resource.String.editTournamentTstSuccess), ToastLength.Long).Show();
                    this.Recreate();
                }
                catch (WebException we)
                {
                    WebExHandlerEdit(we);
                }
            }
            else
            {
                var dlgerrNoConnection = new AlertDialog.Builder(this).SetMessage(Resource.String.errNoConnection).
					SetNegativeButton(Resource.String.ok, delegate
                    {
                        return;
                    }).Show();
            }
        }

        void WebExHandlerEdit(WebException we)
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
                        return;
                    }).Show();
            }
        }

        void WebExHandler(WebException we)
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
}
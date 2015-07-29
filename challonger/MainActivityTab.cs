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
using Android.Content.PM;
using Android.Net;
using System.Json;
using System.Net;

namespace Challonger
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivityTab : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
            SetContentView(Resource.Layout.MainTabLayout);

            AddTab(Resource.String.tabMain, new MainTabFragment(this));
            AddTab(Resource.String.tabCreate, new CreateTabFragment(this));
            AddTab(Resource.String.tabSettings, new SettingsTabFragment(this));

            LoadPreferences();
            if (gVar.apiKey_ == "")
            {
                var dlgNoApi = new AlertDialog.Builder(this);
                dlgNoApi.SetMessage(Resource.String.noAPI);
                dlgNoApi.SetNegativeButton(Resource.String.ok, delegate
                    {
                        ActionBar.SetSelectedNavigationItem(2);
                    });
                dlgNoApi.Show();
            }	
        }


        void AddTab(int resourceID, Fragment view)
        {
            ActionBar.Tab tab = ActionBar.NewTab().SetText(resourceID);
            tab.TabSelected += (object sender, ActionBar.TabEventArgs e) =>
            {
                var fragment = FragmentManager.FindFragmentById(Resource.Id.linLayMainTab);
                if (fragment != null)
                    e.FragmentTransaction.Remove(fragment);
                e.FragmentTransaction.Add(Resource.Id.linLayMainTab, view);
            };

            tab.TabUnselected += (object sender, ActionBar.TabEventArgs e) =>
            {
                e.FragmentTransaction.Remove(view);

            };

            ActionBar.AddTab(tab);
        }

        class MainTabFragment : Fragment
        {
            Activity context;

            public MainTabFragment(Activity _context)
            {
                context = _context;
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                base.OnCreateView(inflater, container, savedInstanceState);

                var view = inflater.Inflate(Resource.Layout.MainTabLayout_Main, container, false);

                TextView txtInfoURL = view.FindViewById<TextView>(Resource.Id.txtInfoURL);
                EditText txtSubdomainURL = view.FindViewById<EditText>(Resource.Id.txtSubdomainURL);
                EditText txtURL = view.FindViewById<EditText>(Resource.Id.txtURL);
                EditText txtSubdomain = view.FindViewById<EditText>(Resource.Id.txtSubdomain);
                Button btnSearch = view.FindViewById<Button>(Resource.Id.btnSearch);

                TextView txtSpinnerText = view.FindViewById<TextView>(Resource.Id.txtSpinnerText);

                TextView txtInfoSubdomain = view.FindViewById<TextView>(Resource.Id.txtInfoSubdomain);

                RadioButton radMainView = view.FindViewById<RadioButton>(Resource.Id.radMainView);
                RadioButton radMainSub = view.FindViewById<RadioButton>(Resource.Id.radMainSub);
                RadioButton radMainUrl = view.FindViewById<RadioButton>(Resource.Id.radMainURL);
                RadioGroup radMain = view.FindViewById<RadioGroup>(Resource.Id.radMain);

                txtSubdomain.Text = "";
                txtSubdomainURL.Text = "";
                txtURL.Text = "";
                txtInfoSubdomain.Visibility = ViewStates.Gone;
                txtSubdomain.Visibility = ViewStates.Gone;
                txtInfoURL.Visibility = ViewStates.Gone;
                txtSubdomainURL.Visibility = ViewStates.Gone;
                txtURL.Visibility = ViewStates.Gone;
                btnSearch.Text = this.GetString(Resource.String.btnSearch);

                radMainSub.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) =>
                {
                    txtSubdomain.Text = "";
                    txtSubdomainURL.Text = "";
                    txtURL.Text = "";
                    txtInfoSubdomain.Visibility = ViewStates.Visible;
                    txtSubdomain.Visibility = ViewStates.Visible;
                    txtInfoURL.Visibility = ViewStates.Gone;
                    txtSubdomainURL.Visibility = ViewStates.Gone;
                    txtURL.Visibility = ViewStates.Gone;
                    btnSearch.Text = this.GetString(Resource.String.btnSearch);
                };

                radMainUrl.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) =>
                {
                    txtSubdomain.Text = "";
                    txtURL.Text = "";
                    txtSubdomainURL.Text = "";
                    txtSubdomain.Visibility = ViewStates.Gone;
                    txtInfoSubdomain.Visibility = ViewStates.Gone;
                    txtInfoURL.Visibility = ViewStates.Visible;
                    txtSubdomainURL.Visibility = ViewStates.Visible;
                    txtURL.Visibility = ViewStates.Visible;
                    btnSearch.Text = this.GetString(Resource.String.btnSearch);	
                };

                radMainView.Click += (object sender, EventArgs e) =>
                {
                    txtSubdomain.Text = "";
                    txtSubdomainURL.Text = "";
                    txtURL.Text = "";
                    txtSubdomain.Visibility = ViewStates.Gone;
                    txtInfoSubdomain.Visibility = ViewStates.Gone;
                    txtInfoURL.Visibility = ViewStates.Gone;
                    txtSubdomainURL.Visibility = ViewStates.Gone;
                    txtURL.Visibility = ViewStates.Gone;
                    btnSearch.Text = this.GetString(Resource.String.btnSearch);
                };

                radMainSub.Click += (object sender, EventArgs e) =>
                {
                    txtSubdomain.Text = "";
                    txtSubdomainURL.Text = "";
                    txtURL.Text = "";
                    txtSubdomain.Visibility = ViewStates.Visible;
                    txtInfoURL.Visibility = ViewStates.Gone;
                    txtInfoSubdomain.Visibility = ViewStates.Visible;
                    txtSubdomainURL.Visibility = ViewStates.Gone;
                    txtURL.Visibility = ViewStates.Gone;
                    btnSearch.Text = this.GetString(Resource.String.btnSearch);
                };

                radMainUrl.Click += (object sender, EventArgs e) =>
                {
                    txtSubdomain.Text = "";
                    txtURL.Text = "";
                    txtSubdomainURL.Text = "";
                    txtSubdomain.Visibility = ViewStates.Gone;
                    txtInfoSubdomain.Visibility = ViewStates.Gone;
                    txtInfoURL.Visibility = ViewStates.Visible;
                    txtSubdomainURL.Visibility = ViewStates.Visible;
                    txtURL.Visibility = ViewStates.Visible;
                    btnSearch.Text = this.GetString(Resource.String.btnSearch);					
                };
					

                btnSearch.Click += (object sender, EventArgs e) =>
                {

                    var intent = new Intent(context, typeof(TournamentListActivity));

                    string url = gVar.URL_;

                    if (radMainView.Checked)
                    {
                        url += "tournaments.json?" + "api_key=" + gVar.apiKey_;
                        intent.PutExtra("url", url);
                        StartActivity(intent);
                    }

                    if (radMainSub.Checked)
                    {
                        if (txtSubdomain.Text == "")
                        {
                            var dlgtxtSubdomain = new AlertDialog.Builder(context);
                            dlgtxtSubdomain.SetMessage(Resource.String.dlgtxtSubdomain);
                            dlgtxtSubdomain.SetNegativeButton(Resource.String.ok, delegate
                                {
                                });
                            dlgtxtSubdomain.Show();
                        }
                        else
                        {
                            url += "tournaments.json?" + "api_key=" + gVar.apiKey_ + "&subdomain=" + txtSubdomain.Text;
                            intent.PutExtra("url", url);
                            StartActivity(intent);
                        }
                    }

                    if (radMainUrl.Checked)
                    {
                        if (txtURL.Text == "")
                        {
                            var dlgtxtURL = new AlertDialog.Builder(context);
                            dlgtxtURL.SetMessage(Resource.String.dlgtxtURL).SetNegativeButton(Resource.String.ok, delegate
                                {
                                }).Show();
                        }
                        else
                        {
                            url += "tournaments/";
                            if (txtSubdomainURL.Text != "")
                                url += txtSubdomainURL.Text + "-" + txtURL.Text;
                            else
                                url += txtURL.Text;
                            url += ".json?" + "api_key=" + gVar.apiKey_;
                            intent.PutExtra("url", url);
                            intent.PutExtra("flag", true);
                            StartActivity(intent);
                        }
                    }
                };

                return view;
            }
        }

        class CreateTabFragment : Fragment
        {
            protected Activity context;
            protected EditText txtName;
            protected RadioGroup radGrpType;
            protected RadioButton radBtnSingle;
            protected RadioButton radBtnDouble;
            protected RadioButton radBtnRound;
            protected RadioButton radBtnSwiss;
            protected EditText txtUrl;
            protected EditText txtSubdomain;
            protected EditText txtDiscription;
            protected CheckBox chkSignup;
            protected CheckBox chkPrivate;
            protected CheckBox chkSE3rdPlace;
            protected Button btnCreate;
            protected EditText txtCap;

            public CreateTabFragment(Activity _context)
            {
                context = _context;
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                base.OnCreateView(inflater, container, savedInstanceState);

                var view = inflater.Inflate(Resource.Layout.CreateNewTournamentDialogLayout, container, false);

                txtName = view.FindViewById<EditText>(Resource.Id.txtCreateTournament_Name_Edit);
                radGrpType = view.FindViewById<RadioGroup>(Resource.Id.radGrpCreateTournamentType);
                radBtnSingle = view.FindViewById<RadioButton>(Resource.Id.radBtnCreateTournamentType_Single);
                radBtnDouble = view.FindViewById<RadioButton>(Resource.Id.radBtnCreateTournamentType_Double);
                radBtnRound = view.FindViewById<RadioButton>(Resource.Id.radBtnCreateTournamentType_Round);
                radBtnSwiss = view.FindViewById<RadioButton>(Resource.Id.radBtnCreateTournamentType_Swiss);
                txtUrl = view.FindViewById<EditText>(Resource.Id.txtCreateTournament_URL_Edit);
                txtSubdomain = view.FindViewById<EditText>(Resource.Id.txtCreateTournament_Subdomain_Edit);
                txtDiscription = view.FindViewById<EditText>(Resource.Id.txtCreateTournament_Description_Edit);
                chkSignup = view.FindViewById<CheckBox>(Resource.Id.chkCreateTournament_Signup);
                chkPrivate = view.FindViewById<CheckBox>(Resource.Id.chkCreateTournament_Private);
                btnCreate = view.FindViewById<Button>(Resource.Id.btnCreateTournament);
                chkSE3rdPlace = view.FindViewById<CheckBox>(Resource.Id.chkCreateTournament_SE_3rdPlaceMatch);
                txtCap = view.FindViewById<EditText>(Resource.Id.txtCreateTournament_Cap_Edit);

                //disable creation of swiss tournament
                radBtnSwiss.Visibility = ViewStates.Gone;

                btnCreate.Click += BtnCreate_Click;
                ;
			
                return view;
            }

            async void BtnCreate_Click(object sender, EventArgs e)
            {
                string postUrl = "https://challonge.com/api/tournaments.json?api_key=" + gVar.apiKey_;

                if (txtName.Text == "")
                    Toast.MakeText(context, Resource.String.tstCreateTournament_NameReq, ToastLength.Short).Show();
                else if (txtUrl.Text == "")
                    Toast.MakeText(context, Resource.String.tstCreateTournament_URLReq, ToastLength.Short).Show();
                else
                {
                    string jsonPost = "{\"tournament\":{";
                    string type;

                    if (radBtnSingle.Checked)
                        type = "single elimination";
                    else if (radBtnDouble.Checked)
                        type = "double elimination";
                    else if (radBtnRound.Checked)
                        type = "round robin";
                    else if (radBtnSwiss.Checked)
                        type = "swiss";
                    else
                        type = "";

                    jsonPost += "\"name\":\"" + txtName.Text + "\",\"tournament_type\":\"" + type + "\",\"url\":\"" + txtUrl.Text + "\"";

                    if (txtSubdomain.Text != "")
                        jsonPost += ",\"subdomain\":\"" + txtSubdomain.Text + "\"";

                    if (txtDiscription.Text != "")
                        jsonPost += ",\"description\":\"" + txtDiscription.Text + "\"";

                    if (txtCap.Text != "" && txtCap.Text != "0")
                        jsonPost += ",\"signup_cap\": " + txtCap.Text;

                    if (chkSE3rdPlace.Checked && type == "single elimination")
                        jsonPost += ",\"hold_third_place_match\": true";

                    if (chkSignup.Checked)
                        jsonPost += ",\"open_signup\": true";

                    if (chkPrivate.Checked)
                        jsonPost += ",\"private\": true";

                    jsonPost += "}}";

                    Console.Out.WriteLine(jsonPost);

                    var connectivityManager = (ConnectivityManager)context.GetSystemService("connectivity");
                    var activeConnection = connectivityManager.ActiveNetworkInfo;

                    if (activeConnection != null && activeConnection.IsConnected)
                    {
                        try
                        {
                            JsonValue json = await Json.PostRetJson(postUrl, jsonPost);
                            Toast.MakeText(context, context.GetString(Resource.String.tstCreateTournament_Created), ToastLength.Long).Show();
                            var dialog = NewTournamentDialog.Initalize(context, json, chkSignup.Checked);
                            dialog.Show(FragmentManager, "dialog");
                        }
                        catch (WebException we)
                        {
                            if (we.Status == WebExceptionStatus.ProtocolError)
                            {

                                var dlgbox = new AlertDialog.Builder(context);

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
                    }
                }
            }
        }

        class SettingsTabFragment : Fragment
        {
            Activity context;

            public SettingsTabFragment(Activity _context)
            {
                context = _context;
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                base.OnCreateView(inflater, container, savedInstanceState);

                var view = inflater.Inflate(Resource.Layout.MainTabLayout_Settings, container, false);

                EditText txtAPIKey = view.FindViewById<EditText>(Resource.Id.txtAPIKey);
                Button btnSaveAPIKey = view.FindViewById<Button>(Resource.Id.btnSaveAPIKey);
                TextView txtGetAPI = view.FindViewById<TextView>(Resource.Id.txtGetAPI);
                Button btnHow = view.FindViewById<Button>(Resource.Id.btnHowToUse);
                Button btnAbout = view.FindViewById<Button>(Resource.Id.btnAbout);

                txtAPIKey.Text = gVar.apiKey_;

                txtGetAPI.Click += delegate
                {
                    var uri = Android.Net.Uri.Parse("https://challonge.com/settings/developer");
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                };

                btnSaveAPIKey.Click += (object IntentSender, EventArgs e) =>
                {
                    if (txtAPIKey.Text == "")
                    {
                        var dlgAPIEmpty = new AlertDialog.Builder(context);
                        dlgAPIEmpty.SetMessage(Resource.String.dlgAPIEmpty);
                        dlgAPIEmpty.SetNegativeButton(Resource.String.ok, delegate
                            {
                            });
                        dlgAPIEmpty.Show();
                    }
                    else if (txtAPIKey.Text.Length < 40)
                    {
                        var dlgAPIShort = new AlertDialog.Builder(context);
                        dlgAPIShort.SetMessage(Resource.String.dlgAPIShort);
                        dlgAPIShort.SetNegativeButton(Resource.String.ok, delegate
                            {
                            });
                        dlgAPIShort.Show();
                    }
                    else if (txtAPIKey.Text.Length > 40)
                    {
                        var dlgAPILong = new AlertDialog.Builder(context);
                        dlgAPILong.SetMessage(Resource.String.dlgAPILong);
                        dlgAPILong.SetNegativeButton(Resource.String.ok, delegate
                            {
                            });
                        dlgAPILong.Show();
                    }
                    else
                    {
                        gVar.apiKey_ = txtAPIKey.Text;
                        SavePreferences(gVar.apiKey_, context);
                        Toast.MakeText(context, Resource.String.tstSaveAPI, ToastLength.Long).Show();
                    }
                };

                btnHow.Click += (object sender, EventArgs e) =>
                {
                    var dlgHow = new AlertDialog.Builder(context);
                    dlgHow.SetTitle(Resource.String.btnHowToUse);
                    dlgHow.SetMessage(Resource.String.txtHowToUse);
                    dlgHow.SetNegativeButton(Resource.String.ok, delegate
                        {
                            return;
                        });
                    dlgHow.Show();
                };

                btnAbout.Click += (object sender, EventArgs e) =>
                {
                    var dlgAbout = new AlertDialog.Builder(context);
                    dlgAbout.SetTitle(Resource.String.btnAbout);
                    dlgAbout.SetMessage(Resource.String.txtAbout);
                    dlgAbout.SetNegativeButton(Resource.String.ok, delegate
                        {
                            return;
                        });
                    dlgAbout.Show();
                };

                return view;
            }
        }

        private void LoadPreferences()
        {
            var prefs = this.GetSharedPreferences("Challonger.preferences", FileCreationMode.Private);

            if (prefs.Contains("api_key"))
                gVar.apiKey_ = prefs.GetString("api_key", "");
            else
                gVar.apiKey_ = "";
        }

        private static void SavePreferences(string api_key, Activity context)
        {
            var prefs = context.GetSharedPreferences("Challonger.preferences", FileCreationMode.Private);
            var editor = prefs.Edit();
            editor.PutString("api_key", api_key).Commit();
        }
    }
}


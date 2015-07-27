using System;
using Android.Widget;
using Android.App;
using Android.OS;
using System.Json;
using System.Collections;
using Android.Content;
using Android.Net;
using System.Net;
using Android.Views;

namespace Challonger
{
    public class EditTournamentStateDialog : DialogFragment
    {
        protected Button btnStart;
        protected Button btnFinalize;
        protected Button btnReset;
        protected Button btnDestroy;
        protected JsonValue json;
        protected Activity context;

        public static  EditTournamentStateDialog Initalize(JsonValue _json, Activity _context)
        {
            var dialogFragment = new EditTournamentStateDialog();
            dialogFragment.json = _json;
            dialogFragment.context = _context;
            return dialogFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var builder = new AlertDialog.Builder(Activity);

            var inflater = Activity.LayoutInflater;

            var view = inflater.Inflate(Resource.Layout.TournamentInfoStateDialogLayout, null);

            if (view != null)
            {
                btnStart = view.FindViewById<Button>(Resource.Id.btnEditTournamentState_Start);
                btnFinalize = view.FindViewById<Button>(Resource.Id.btnEditTournamentState_Finalize);
                btnReset = view.FindViewById<Button>(Resource.Id.btnEditTournamentState_Reset);
                btnDestroy = view.FindViewById<Button>(Resource.Id.btnEditTournamentState_Destroy);

                btnStart.Enabled = false;
                btnFinalize.Enabled = false;
                btnReset.Enabled = false;
                string state = json["state"];
                switch (state)
                {
                    case "complete":
                        btnReset.Enabled = true;
                        break;
                    case "awaiting_review":
                        btnReset.Enabled = true;
                        btnFinalize.Enabled = true;
                        break;
                    case "underway":
                        btnReset.Enabled = true;
                        break;
                    case "pending":
                        btnStart.Enabled = true;
                        break;
                }

                btnStart.Click += BtnStart_Click;
                btnFinalize.Click += BtnFinalize_Click;
                btnReset.Click += BtnReset_Click;
                btnDestroy.Click += BtnDestroy_Click;

                builder.SetTitle(Resource.String.editTournamentState);

                builder.SetView(view);
            }

            var dialog = builder.Create();
            return dialog;
        }

        async void BtnDestroy_Click(object sender, EventArgs e)
        {
            string delUrl = gVar.URL_;

            if (json["subdomain"] != null)
                delUrl += "tournaments/" + json["subdomain"] + "-" + json["url"] + ".json?api_key=" + gVar.apiKey_;
            else
                delUrl += "tournaments/" + json["url"] + ".json?api_key=" + gVar.apiKey_;

            var connectivityManager = (ConnectivityManager)context.GetSystemService("connectivity");
            var activeConnection = connectivityManager.ActiveNetworkInfo;

            if (activeConnection != null && activeConnection.IsConnected)
            {
                try
                {
                    await Json.DeleteJson(delUrl);
                    Toast.MakeText(context, context.GetString(Resource.String.tstStateDestroy), ToastLength.Long).Show();
                    this.Dismiss();
                    context.Finish();
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

        void BtnReset_Click(object sender, EventArgs e)
        {
            PostUrl("reset");
        }

        void BtnFinalize_Click(object sender, EventArgs e)
        {
            PostUrl("finalize");
        }

        void BtnStart_Click(object sender, EventArgs e)
        {
            PostUrl("start");
        }

        private async void PostUrl(string type)
        {
            string postUrl = gVar.URL_;

            if (json["subdomain"] != null)
                postUrl += "tournaments/" + json["subdomain"] + "-" + json["url"] + "/" + type + ".json?api_key=" + gVar.apiKey_;
            else
                postUrl += "tournaments/" + json["url"] + "/" + type + ".json?api_key=" + gVar.apiKey_;

            var connectivityManager = (ConnectivityManager)context.GetSystemService("connectivity");
            var activeConnection = connectivityManager.ActiveNetworkInfo;

            if (activeConnection != null && activeConnection.IsConnected)
            {
                try
                {
                    await Json.PostJson(postUrl);
                    switch (type)
                    {
                        case "start":
                            Toast.MakeText(context, context.GetString(Resource.String.tstStateStart), ToastLength.Long).Show();
                            break;
                        case "finalize":
                            Toast.MakeText(context, context.GetString(Resource.String.tstStateFinalize), ToastLength.Long).Show();
                            break;
                        case "reset":
                            Toast.MakeText(context, context.GetString(Resource.String.tstStateReset), ToastLength.Long).Show();
                            if (context.ActionBar.TabCount == 4)
                                context.ActionBar.RemoveTabAt(3);
                            break;
                    }
                    this.Dismiss();
                    context.Recreate();
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

    public class EditTournamentTypeDialog : DialogFragment
    {
        protected RadioButton radSin;
        protected RadioButton radDou;
        protected RadioButton radRou;
        protected RadioButton radSwi;
        protected Button btnSave;
        protected JsonValue json;
        protected Activity context;

        public static  EditTournamentTypeDialog Initalize(JsonValue _json, Activity _context)
        {
            var dialogFragment = new EditTournamentTypeDialog();
            dialogFragment.json = _json;
            dialogFragment.context = _context;
            return dialogFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var builder = new AlertDialog.Builder(Activity);

            var inflater = Activity.LayoutInflater;

            var view = inflater.Inflate(Resource.Layout.EditTournamentTypeLayout, null);

            if (view != null)
            {
                radSin = view.FindViewById<RadioButton>(Resource.Id.radBtnEditTournamentType_Single);
                radDou = view.FindViewById<RadioButton>(Resource.Id.radBtnEditTournamentType_Double);
                radRou = view.FindViewById<RadioButton>(Resource.Id.radBtnEditTournamentType_Round);
                radSwi = view.FindViewById<RadioButton>(Resource.Id.radBtnEditTournamentType_Swiss);
                btnSave = view.FindViewById<Button>(Resource.Id.btnEditTournamentType);

                //disable swiss
                radSwi.Visibility = ViewStates.Gone;
               
                string curType = json["tournament_type"];

                switch (curType)
                {
                    case "single elimination":
                        radSin.Checked = true;
                        break;
                    case "double elimination":
                        radDou.Checked = true;
                        break;
                    case "round robin":
                        radRou.Checked = true;
                        break;
                    case "swiss":
                        radSwi.Checked = true;
                        break;
                }

                btnSave.Click += (object sender, EventArgs e) =>
                {
                    string type;
                    if (radSin.Checked)
                        type = "single elimination";
                    else if (radDou.Checked)
                        type = "double elimination";
                    else if (radRou.Checked)
                        type = "round robin";
                    else if (radSwi.Checked)
                        type = "swiss";
                    else
                        type = "";

                    string jsonPut = "{\"tournament\":{\"tournament_type\":\"" + type + "\"}}";
                    dlgEditTournamentInfo(jsonPut);
                };

                builder.SetTitle(Resource.String.editTournamentType);

                builder.SetView(view);
            }

            var dialog = builder.Create();
            return dialog;
        }

        async void dlgEditTournamentInfo(string jsonPut)
        {
            string puturl = gVar.URL_;

            if (json["subdomain"] == null)
                puturl += "tournaments/" + json["url"] + ".json?api_key=" + gVar.apiKey_;
            else
                puturl += "tournaments/" + json["subdomain"] + "-" + json["url"] + ".json?api_key=" + gVar.apiKey_;

            var connectivityManager = (ConnectivityManager)context.GetSystemService("connectivity");
            //check for internet connection
            var activeConnection = connectivityManager.ActiveNetworkInfo;
            if ((activeConnection != null) && activeConnection.IsConnected)
            {
                //check for web exceptions
                try
                {
                    await Json.PutJson(puturl, jsonPut);
                    Toast.MakeText(context, context.GetString(Resource.String.editTournamentTypeTstSuccess), ToastLength.Long).Show();
                    this.Dismiss();
                    context.Recreate();
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
            else
            {
                var dlgerrNoConnection = new AlertDialog.Builder(context).SetMessage(Resource.String.errNoConnection).
                    SetNegativeButton(Resource.String.ok, delegate
                    {
                        return;
                    }).Show();
            }
        }

    }

}


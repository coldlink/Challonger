using System;
using Android.App;
using Android.Widget;
using System.Json;
using Android.OS;
using Android.Net;
using System.Net;

namespace Challonger
{
    public class EditParticipantInfoDialog : DialogFragment
    {
        protected TextView txtName;
        protected TextView txtUserName;
        protected TextView txtSeed;
        protected Button btnSave;
        protected Button btnDelete;
        protected ParticipantInfo partInfo;
        protected Activity context;
        protected JsonValue json;

        public static  EditParticipantInfoDialog Initalize(ParticipantInfo _partInfo, JsonValue _json, Activity _context)
        {
            var dialogFragment = new EditParticipantInfoDialog();
            dialogFragment.json = _json;
            dialogFragment.context = _context;
            dialogFragment.partInfo = _partInfo;
            return dialogFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var builder = new AlertDialog.Builder(Activity);

            var inflater = Activity.LayoutInflater;

            var view = inflater.Inflate(Resource.Layout.EditParticipantInfoDialogLayout, null);

            if (view != null)
            {
                txtName = view.FindViewById<TextView>(Resource.Id.txtEditParticipantInfoName_Edit);
                txtUserName = view.FindViewById<TextView>(Resource.Id.txtEditParticipantInfoUserName_Edit);
                txtSeed = view.FindViewById<TextView>(Resource.Id.txtEditParticipantInfoSeed_Edit);
                btnSave = view.FindViewById<Button>(Resource.Id.btnEditParticipantInfo_Save);
                btnDelete = view.FindViewById<Button>(Resource.Id.btnEditParticipantInfo_Delete);

                txtName.Text = partInfo.displayName;
                if (partInfo.userName != null)
                    txtUserName.Text = partInfo.userName;
                else
                    txtUserName.Text = "";

                txtSeed.Text = partInfo.seed.ToString();

                btnSave.Click += BtnSave_Click;
                btnDelete.Click += BtnDelete_Click;

                builder.SetTitle(Resource.String.txtEditParticipantInfo);

                builder.SetView(view);
            }

            var dialog = builder.Create();
            return dialog;
        }

        async void BtnDelete_Click(object sender, EventArgs e)
        {
            string delUrl = gVar.URL_;

            if (json["subdomain"] != null)
                delUrl += "tournaments/" + json["subdomain"] + "-" + json["url"] + "/participants/" + partInfo.id + ".json?api_key=" + gVar.apiKey_;
            else
                delUrl += "tournaments/" + json["url"] + "/participants/" + partInfo.id + ".json?api_key=" + gVar.apiKey_;

            var connectivityManager = (ConnectivityManager)context.GetSystemService("connectivity");
            var activeConnection = connectivityManager.ActiveNetworkInfo;

            if (activeConnection != null && activeConnection.IsConnected)
            {
                try
                {
                    await Json.DeleteJson(delUrl);
                    Toast.MakeText(context, context.GetString(Resource.String.tstParticipantDestroy), ToastLength.Long).Show();
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

        async void BtnSave_Click(object sender, EventArgs e)
        {
            string putUrl = gVar.URL_;

            if (json["subdomain"] != null)
                putUrl += "tournaments/" + json["subdomain"] + "-" + json["url"] + "/participants/" + partInfo.id + ".json?api_key=" + gVar.apiKey_;
            else
                putUrl += "tournaments/" + json["url"] + "/participants/" + partInfo.id + ".json?api_key=" + gVar.apiKey_;

            string jsonPut = "{\"participant\":{";

            if (txtName.Text == "" && txtUserName.Text == "")
                Toast.MakeText(context, context.GetString(Resource.String.tstParticipantNoName), ToastLength.Long).Show();
            else
            {
                if (txtName.Text != "")
                    jsonPut += "\"name\":\"" + txtName.Text + "\"";

                if (txtName.Text != "" && txtUserName.Text != "")
                    jsonPut += ",\"challonge_username\":\"" + txtUserName.Text + "\"";
                else if (txtName.Text == "" && txtUserName.Text != "")
                    jsonPut += "\"challonge_username\":\"" + txtUserName.Text + "\"";
                else if (txtName.Text != "" && txtUserName.Text == "")
                    jsonPut += ",\"challonge_username\": null";

                if (txtSeed.Text == "")
                    jsonPut += "}}";
                else
                    jsonPut += ",\"seed\":" + txtSeed.Text + "}}";

                //check for internet connection
                var connectivityManager = (ConnectivityManager)context.GetSystemService("connectivity");
                var activeConnection = connectivityManager.ActiveNetworkInfo;

                if (activeConnection != null && activeConnection.IsConnected)
                {
                    try
                    {
                        await Json.PutJson(putUrl, jsonPut);
                        Toast.MakeText(context, context.GetString(Resource.String.tstParticipantSave), ToastLength.Long).Show();
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
    }

    public class AddParticipantInfoDialog : DialogFragment
    {
        protected TextView txtName;
        protected TextView txtUserName;
        protected TextView txtSeed;
        protected Button btnAdd;
        protected Activity context;
        protected JsonValue json;

        public static  AddParticipantInfoDialog Initalize(JsonValue _json, Activity _context)
        {
            var dialogFragment = new AddParticipantInfoDialog();
            dialogFragment.json = _json;
            dialogFragment.context = _context;
            return dialogFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var builder = new AlertDialog.Builder(Activity);

            var inflater = Activity.LayoutInflater;

            var view = inflater.Inflate(Resource.Layout.AddParticipantInfoDialogLayout, null);

            if (view != null)
            {
                txtName = view.FindViewById<TextView>(Resource.Id.txtAddParticipantInfoName_Edit);
                txtUserName = view.FindViewById<TextView>(Resource.Id.txtAddParticipantInfoUserName_Edit);
                txtSeed = view.FindViewById<TextView>(Resource.Id.txtAddParticipantInfoSeed_Edit);
                btnAdd = view.FindViewById<Button>(Resource.Id.btnAddParticipantInfo_Add);

                btnAdd.Click += BtnAdd_Click;

                builder.SetTitle(Resource.String.txtAddParticipantInfo);

                builder.SetView(view);
            }

            var dialog = builder.Create();
            return dialog;
        }

        async void BtnAdd_Click(object sender, EventArgs e)
        {
            string postUrl = gVar.URL_;

            if (json["subdomain"] != null)
                postUrl += "tournaments/" + json["subdomain"] + "-" + json["url"] + "/participants.json?api_key=" + gVar.apiKey_;
            else
                postUrl += "tournaments/" + json["url"] + "/participants.json?api_key=" + gVar.apiKey_;

            string jsonPOST = "{\"participant\":{";

            if (txtName.Text == "" && txtUserName.Text == "")
                Toast.MakeText(context, context.GetString(Resource.String.tstParticipantNoName), ToastLength.Long).Show();
            else
            {
                if (txtName.Text != "")
                    jsonPOST += "\"name\":\"" + txtName.Text + "\"";

                if (txtName.Text != "" && txtUserName.Text != "")
                    jsonPOST += ",\"challonge_username\":\"" + txtUserName.Text + "\"";
                else if (txtName.Text == "" && txtUserName.Text != "")
                    jsonPOST += "\"challonge_username\":\"" + txtUserName.Text + "\"";
                else if (txtName.Text != "" && txtUserName.Text == "")
                    jsonPOST += ",\"challonge_username\": null";

                if (txtSeed.Text == "")
                    jsonPOST += "}}";
                else
                    jsonPOST += ",\"seed\":" + txtSeed.Text + "}}";

                //check for internet connection
                var connectivityManager = (ConnectivityManager)context.GetSystemService("connectivity");
                var activeConnection = connectivityManager.ActiveNetworkInfo;

                if (activeConnection != null && activeConnection.IsConnected)
                {
                    try
                    {
                        await Json.PostJson(postUrl, jsonPOST);
                        Toast.MakeText(context, context.GetString(Resource.String.tstParticipantAdd), ToastLength.Long).Show();
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
    }
}

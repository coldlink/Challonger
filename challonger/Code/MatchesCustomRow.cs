using System;
using Android.Widget;
using Android.App;
using System.Collections.Generic;
using Android.Views;
using Android.Graphics;
using Android.Runtime;
using Java.Text;
using Android.Content;
using System.Net;
using Android.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using System.Globalization;

namespace Challonger
{
    public class MatchInfo
    {
        public int id{ get; set; }

        public string ident{ get; set; }

        public string state{ get; set; }

        public int stateId { get; set; }

        public string p1{ get; set; }

        public string p2{ get; set; }

        public int round{ get; set; }

        public int p1s{ get; set; }

        public int p2s{ get; set; }

        public int p1prematchid{ get; set; }

        public bool p1isprematchloser{ get; set; }

        public int p2prematchid{ get; set; }

        public bool p2isprematchloser{ get; set; }

        public int winId{ get; set; }

        public int losId{ get; set; }

        public int p1id{ get; set; }

        public int p2id{ get; set; }

        public string subdomain{ get; set; }

        public string url{ get; set; }
    }


    public class MatchInfoListAdapter : BaseAdapter<MatchInfo>
    {
        Activity context;

        List<MatchInfo> list;

        public MatchInfoListAdapter(Activity _context, List<MatchInfo> _list)
            : base()
        {
            this.context = _context;
            this.list = _list.OrderBy(x => x.stateId).ToList();
        }

        public override int Count
        {
            get{ return list.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override MatchInfo this [int index]
        {
            get{ return list[index]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder vh;

            var view = convertView;

            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.MatchInfoListLayout, parent, false);
                vh = new ViewHolder();
                vh.Initalize(view);
                view.Tag = vh;
            }

            var item = this[position];

            vh = (ViewHolder)view.Tag;

            vh.Bind(this.context, item.id, item.ident, item.state, item.p1, item.p2, item.round, item.p1s, item.p2s, 
                item.p1prematchid, item.p1isprematchloser, item.p2prematchid, item.p2isprematchloser, item.winId, 
                item.losId, item.p1id, item.p2id, item.subdomain, item.url);

            return view;
        }

        private class ViewHolder : Java.Lang.Object
        {
            Activity Context;
            Button btnP1;
            Button btnP2;
            Button btnP1s;
            Button btnP2s;
            Button btnSave;
            TextView txtIdent;
            TextView txtRound;
            TextView txtState;

            int id;
            string ident;
            string state;
            string p1;
            string p2;
            int round;
            int p1s;
            int p2s;
            int p1prematchid;
            bool p1isprematchloser;
            int p2prematchid;
            bool p2isprematchloser;
            int winId;
            int losId;
            int p1id;
            int p2id;
            string subdomain;
            string url;

            public void Initalize(View view)
            {
                btnP1 = view.FindViewById<Button>(Resource.Id.btnPlayer1ViewMatch);
                btnP2 = view.FindViewById<Button>(Resource.Id.btnPlayer2ViewMatch);
                btnP1s = view.FindViewById<Button>(Resource.Id.btnP1ScoreViewMatch);
                btnP2s = view.FindViewById<Button>(Resource.Id.btnP2ScoreViewMatch);
                btnSave = view.FindViewById<Button>(Resource.Id.btnMatchInfoSave);
                txtRound = view.FindViewById<TextView>(Resource.Id.txtRoundViewMatch);
                txtIdent = view.FindViewById<TextView>(Resource.Id.txtMatchInfoIdent);
                txtState = view.FindViewById<TextView>(Resource.Id.txtMatchInfoStatus);

                btnP1.Click += new EventHandler(this.btnP1_Click);
                btnP2.Click += new EventHandler(this.btnP2_Click);
                btnP1s.Click += new EventHandler(this.btnP1s_Click);
                btnP2s.Click += new EventHandler(this.btnP2s_Click);
                btnSave.Click += new EventHandler(this.btnSave_Click);

                btnP1s.LongClick += (object sender, View.LongClickEventArgs e) =>
                {
                    btnP1s_LongClick(sender, e);
                };

                btnP2s.LongClick += (object sender, View.LongClickEventArgs e) =>
                {
                    btnP2s_LongClick(sender, e);
                };
            }

            public void Bind(Activity _context, int _id, string _ident, string _state, string _p1, string _p2, 
                     int _round, int _p1s, int _p2s, int _p1prematchid, bool _p1isprematchloser, int _p2prematchid, 
                     bool _p2isprematchloser, int _winId, int _losId, int _p1id, int _p2id, string _subdomain, string _url)
            {
                Context = _context;

                id = _id;
                ident = _ident;
                state = _state;
                p1 = _p1;
                p2 = _p2;
                round = _round;
                p1s = _p1s;
                p2s = _p2s;
                p1prematchid = _p1prematchid;
                p1isprematchloser = _p1isprematchloser;
                p2prematchid = _p2prematchid;
                p2isprematchloser = _p2isprematchloser;
                winId = _winId;
                losId = _losId;
                p1id = _p1id;
                p2id = _p2id;
                subdomain = _subdomain;
                url = _url;

                string rstring = round.ToString();
                if (round < 0)
                {
                    rstring = rstring.Replace("-", "L");
                }

                txtRound.Text = Context.GetString(Resource.String.strRound) + rstring;

                btnP1s.Text = p1s.ToString();

                btnP2s.Text = p2s.ToString();

                btnSave.Visibility = ViewStates.Gone;

                txtIdent.Text = Context.GetString(Resource.String.strMatch) + ident;

                txtState.Text = Context.GetString(Resource.String.strState) + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(state);

                switch (state)
                {
                    case "complete":
                        btnP1.Text = p1;
                        btnP2.Text = p2;

                        if (winId == p1id)
                        {
                            btnP1.SetTextColor(Color.Aqua);
                            btnP2.SetTextColor(Color.White);
                        }
                        else if (winId == p2id)
                        {
                            btnP1.SetTextColor(Color.White);
                            btnP2.SetTextColor(Color.Aqua);
                        }
                        else
                        {
                            setWhiteText();
                        }
                        break;
                    case "pending":
                        if (p1 == "")
                        {
                            if (p1isprematchloser)
                            {
                                btnP1.Text = Context.GetString(Resource.String.strLoserOf) + gVar.dictMIdIdent[p1prematchid];
                                setWhiteText();
                            }
                            else
                            {
                                btnP1.Text = Context.GetString(Resource.String.strWinnerOf) + gVar.dictMIdIdent[p1prematchid];
                                setWhiteText();
                            }
                        }
                        else
                        {
                            btnP1.Text = p1;
                            setWhiteText();
                        }

                        if (p2 == "")
                        {
                            if (p2isprematchloser)
                            {
                                btnP2.Text = Context.GetString(Resource.String.strLoserOf) + gVar.dictMIdIdent[p2prematchid];
                                setWhiteText();
                            }
                            else
                            {
                                btnP2.Text = Context.GetString(Resource.String.strWinnerOf) + gVar.dictMIdIdent[p2prematchid];
                                setWhiteText();
                            }
                        }
                        else
                        {
                            btnP2.Text = p2;
                            setWhiteText();
                        }
                        break;
                    case "open":
                        btnP1.Text = p1;
                        btnP2.Text = p2;
                        setWhiteText();
                        break;
                }
            }

            private void setWhiteText()
            {
                btnP1.SetTextColor(Color.White);
                btnP2.SetTextColor(Color.White);
            }

            void btnP1_Click(object sender, EventArgs e)
            {
                if (gVar.boolTournamentEditModeEnabled)
                {
                    switch (state)
                    {
                        case "open":
                            winId = p1id;
                            btnP1.SetTextColor(Color.Aqua);
                            btnP2.SetTextColor(Color.White);
                            btnSave.Visibility = ViewStates.Visible;
                            break;
                        case "complete":
                            AlertDialog.Builder dlgComp = new AlertDialog.Builder(Context);
                            dlgComp.SetTitle(Context.GetString(Resource.String.dlgWarningCompleteMatchTitle));
                            dlgComp.SetMessage(Context.GetString(Resource.String.dlgWarningCompleteMatchTxt));
                            dlgComp.SetPositiveButton(Context.GetString(Resource.String.ok), delegate
                                {
                                    winId = p1id;
                                    btnP1.SetTextColor(Color.Aqua);
                                    btnP2.SetTextColor(Color.White);
                                    btnSave.Visibility = ViewStates.Visible;
                                    return;
                                });
                            dlgComp.SetNegativeButton(Context.GetString(Resource.String.cancel), delegate
                                {
                                    return;
                                });
                            dlgComp.Show();
                            break;
                    }
                }
            }

            void btnP2_Click(object sender, EventArgs e)
            {

                if (gVar.boolTournamentEditModeEnabled)
                {
                    switch (state)
                    {
                        case "open":
                            winId = p2id;
                            btnP2.SetTextColor(Color.Aqua);
                            btnP1.SetTextColor(Color.White);
                            btnSave.Visibility = ViewStates.Visible;
                            break;
                        case "complete":
                            AlertDialog.Builder dlgComp = new AlertDialog.Builder(Context);
                            dlgComp.SetTitle(Context.GetString(Resource.String.dlgWarningCompleteMatchTitle));
                            dlgComp.SetMessage(Context.GetString(Resource.String.dlgWarningCompleteMatchTxt));
                            dlgComp.SetPositiveButton(Context.GetString(Resource.String.ok), delegate
                                {
                                    winId = p2id;
                                    btnP2.SetTextColor(Color.Aqua);
                                    btnP1.SetTextColor(Color.White);
                                    btnSave.Visibility = ViewStates.Visible;
                                    return;
                                });
                            dlgComp.SetNegativeButton(Context.GetString(Resource.String.cancel), delegate
                                {
                                    return;
                                });
                            dlgComp.Show();
                            break;
                    }
                }
            }

            void btnP1s_Click(object sender, EventArgs e)
            {
                if (gVar.boolTournamentEditModeEnabled)
                {
                    switch (state)
                    {
                        case "open":
                            p1s += 1;
                            btnP1s.Text = p1s.ToString();
                            btnSave.Visibility = ViewStates.Visible;
                            break;
                        case "complete":
                            AlertDialog.Builder dlgComp = new AlertDialog.Builder(Context);
                            dlgComp.SetTitle(Context.GetString(Resource.String.dlgWarningCompleteMatchTitle));
                            dlgComp.SetMessage(Context.GetString(Resource.String.dlgWarningCompleteMatchTxt));
                            dlgComp.SetPositiveButton(Context.GetString(Resource.String.ok), delegate
                                {
                                    p1s += 1;
                                    btnP1s.Text = p1s.ToString();
                                    btnSave.Visibility = ViewStates.Visible;
                                    return;
                                });
                            dlgComp.SetNegativeButton(Context.GetString(Resource.String.cancel), delegate
                                {
                                    return;
                                });
                            dlgComp.Show();
                            break;
                    }
                }
            }

            void btnP2s_Click(object sender, EventArgs e)
            {
                if (gVar.boolTournamentEditModeEnabled)
                {
                    switch (state)
                    {
                        case "open":
                            p2s += 1;
                            btnP2s.Text = p2s.ToString();
                            btnSave.Visibility = ViewStates.Visible;
                            break;
                        case "complete":
                            AlertDialog.Builder dlgComp = new AlertDialog.Builder(Context);
                            dlgComp.SetTitle(Context.GetString(Resource.String.dlgWarningCompleteMatchTitle));
                            dlgComp.SetMessage(Context.GetString(Resource.String.dlgWarningCompleteMatchTxt));
                            dlgComp.SetPositiveButton(Context.GetString(Resource.String.ok), delegate
                                {
                                    p2s += 1;
                                    btnP2s.Text = p2s.ToString();
                                    btnSave.Visibility = ViewStates.Visible;
                                    return;
                                });
                            dlgComp.SetNegativeButton(Context.GetString(Resource.String.cancel), delegate
                                {
                                    return;
                                });
                            dlgComp.Show();
                            break;
                    }
                }
            }

            void btnP1s_LongClick(object sender, View.LongClickEventArgs e)
            {
                if (gVar.boolTournamentEditModeEnabled)
                {
                    switch (state)
                    {
                        case "open":
                            p1s -= 1;
                            btnP1s.Text = p1s.ToString();
                            btnSave.Visibility = ViewStates.Visible;
                            break;
                        case "complete":
                            AlertDialog.Builder dlgComp = new AlertDialog.Builder(Context);
                            dlgComp.SetTitle(Context.GetString(Resource.String.dlgWarningCompleteMatchTitle));
                            dlgComp.SetMessage(Context.GetString(Resource.String.dlgWarningCompleteMatchTxt));
                            dlgComp.SetPositiveButton(Context.GetString(Resource.String.ok), delegate
                                {
                                    p1s -= 1;
                                    btnP1s.Text = p1s.ToString();
                                    btnSave.Visibility = ViewStates.Visible;
                                    return;
                                });
                            dlgComp.SetNegativeButton(Context.GetString(Resource.String.cancel), delegate
                                {
                                    return;
                                });
                            dlgComp.Show();
                            break;
                    }
                }
            }

            void btnP2s_LongClick(object sender, View.LongClickEventArgs e)
            {
                if (gVar.boolTournamentEditModeEnabled)
                {
                    switch (state)
                    {
                        case "open":
                            p2s -= 1;
                            btnP2s.Text = p2s.ToString();
                            btnSave.Visibility = ViewStates.Visible;
                            break;
                        case "complete":
                            AlertDialog.Builder dlgComp = new AlertDialog.Builder(Context);
                            dlgComp.SetTitle(Context.GetString(Resource.String.dlgWarningCompleteMatchTitle));
                            dlgComp.SetMessage(Context.GetString(Resource.String.dlgWarningCompleteMatchTxt));
                            dlgComp.SetPositiveButton(Context.GetString(Resource.String.ok), delegate
                                {
                                    p2s -= 1;
                                    btnP2s.Text = p2s.ToString();
                                    btnSave.Visibility = ViewStates.Visible;
                                    return;
                                });
                            dlgComp.SetNegativeButton(Context.GetString(Resource.String.cancel), delegate
                                {
                                    return;
                                });
                            dlgComp.Show();
                            break;
                    }
                }
            }

            async void btnSave_Click(object sender, EventArgs e)
            {
                string puturl = gVar.URL_;
                if (subdomain == "")
                    puturl += "tournaments/" + url + "/matches/" + id + ".json?api_key=" + gVar.apiKey_;
                else
                    puturl += "tournaments/" + subdomain + "-" + url + "/matches/" + id + ".json?api_key=" + gVar.apiKey_;

                string score = p1s.ToString() + "-" + p2s.ToString();

                string jsonPut = "{\"match\":{\"scores_csv\":\"" + score + "\",\"winner_id\":" + winId + "}}";

                //check for internet connection
                var connectivityManager = (ConnectivityManager)Context.GetSystemService("connectivity");
                var activeConnection = connectivityManager.ActiveNetworkInfo;

                if (activeConnection != null && activeConnection.IsConnected)
                {
                    try
                    {
                        await Json.PutJson(puturl, jsonPut);
                        Toast.MakeText(Context, Context.GetString(Resource.String.tstMatchSaved), ToastLength.Long).Show();
                        Context.Recreate();
                    }
                    catch (WebException we)
                    {
                        if (we.Status == WebExceptionStatus.ProtocolError)
                        {

                            var dlgbox = new AlertDialog.Builder(Context);

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
                btnSave.Visibility = ViewStates.Gone;
            }
        }
    }
}
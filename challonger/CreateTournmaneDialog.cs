using System;
using Android.App;
using Android.Widget;
using Android.Content;
using Android.OS;
using Android.Net;
using System.Net;
using System.Json;
using Android.Views;

namespace Challonger
{
	public class NewTournamentDialog : DialogFragment
	{
		protected EditText txtUrl;
		protected EditText txtSignup;
		protected TextView txtSignup_Text;
		protected Button btnView;
		protected Activity context;
		protected JsonValue json;
		protected bool chkSignup;

		public static NewTournamentDialog Initalize (Activity _context, JsonValue _json, bool _chkSignup)
		{
			var dialogFragment = new NewTournamentDialog ();
			dialogFragment.context = _context;
			dialogFragment.json = _json;
			dialogFragment.chkSignup = _chkSignup;
			return dialogFragment;
		}

		public override Dialog OnCreateDialog (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			var builder = new AlertDialog.Builder (Activity);
			var inflater = Activity.LayoutInflater;
			var view = inflater.Inflate (Resource.Layout.NewTournamentDialogLayout, null);

			if (view != null) {
				txtUrl = view.FindViewById<EditText> (Resource.Id.txtNewTournamentURL_Edit);
				txtSignup = view.FindViewById<EditText> (Resource.Id.txtNewTournamentSignup_Edit);
				txtSignup_Text = view.FindViewById<TextView> (Resource.Id.txtNewTournamentSignup);
				btnView = view.FindViewById<Button> (Resource.Id.btnNewTournament_View);

				txtUrl.Text = json ["tournament"] ["full_challonge_url"];

				if (json ["tournament"] ["sign_up_url"] == null) {
					txtSignup.Visibility = ViewStates.Gone;
					txtSignup_Text.Visibility = ViewStates.Gone;
				} else
					txtSignup.Text = json ["tournament"] ["sign_up_url"];

				btnView.Click += BtnView_Click;

				builder.SetTitle (json ["tournament"] ["name"].ToString ());

				builder.SetView (view);
			}

			var dialog = builder.Create ();
			return dialog;
		}

		void BtnView_Click (object sender, EventArgs e)
		{
			string url = gVar.URL_ + "tournaments/";
			//string urlm = url;
			if (json ["tournament"] ["subdomain"] != null) {
				url += json ["tournament"] ["subdomain"] + "-" + json ["tournament"] ["url"] + ".json?" + "api_key=" + gVar.apiKey_ + "&include_participants=1&include_matches=1";
			} else {
				url += json ["tournament"] ["url"] + ".json?" + "api_key=" + gVar.apiKey_ + "&include_participants=1&include_matches=1";
			}

			var intent = new Intent (context, typeof(ViewTournamentInfoActivity));

			intent.PutExtra ("url", url);
			gVar.boolTournamentEditModeEnabled = false;
			gVar.lastViewTournamentInfoTabSelected = 0;
			this.Dismiss ();
			StartActivity (intent);
		}
	}
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Net;
using Android.Graphics;
using Java.Security;
using Android.Graphics.Drawables;

namespace Challonger
{
	public class dictImgIdParticipant
	{
		public static Dictionary<int, Bitmap> idParticipantImageCache = new Dictionary<int, Bitmap> ();
	}

	public class ParticipantInfo
	{
		public string displayName{ get; set; }

		public string userName{ get; set; }

		public int seed{ get; set; }

		public string imgURL{ get; set; }

		public int id{ get; set; }

		public int rank{ get; set; }
	}

	public class ParticipantInfoListAdapter : BaseAdapter<ParticipantInfo>
	{
		Activity context;

		List<ParticipantInfo> list;

		public ParticipantInfoListAdapter (Activity _context, List<ParticipantInfo> _list) : base ()
		{
			this.context = _context;
			this.list = _list;
		}

		public override int Count {
			get{ return list.Count; }
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override ParticipantInfo this [int index] {
			get{ return list [index]; }
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{

			ViewHolder vh;

			View view = convertView;

			if (view == null) {
				view = context.LayoutInflater.Inflate (Resource.Layout.ParticipantInfoListLayout, parent, false);
				vh = new ViewHolder ();
				vh.Initalize (view);
				view.Tag = vh;
			}				

			var item = this [position];

			vh = (ViewHolder)view.Tag;

			vh.Bind (this.context, item.displayName, item.userName, item.seed, item.imgURL, item.id, item.rank);

			return view;
		}

		private class ViewHolder : Java.Lang.Object
		{
			Activity Context;
			TextView txtDisplayName;
			TextView txtUsername;
			TextView txtSeed;
			ImageView imgParticipant;

			string displayName;
			string userName;
			int seed;
			string imgURL;
			int id;
			int rank;

			public void Initalize (View view)
			{
				txtDisplayName = view.FindViewById<TextView> (Resource.Id.txtParticipantViewDisplayName);
				txtUsername = view.FindViewById<TextView> (Resource.Id.txtParticipantViewUsername);
				txtSeed = view.FindViewById<TextView> (Resource.Id.txtParticipantViewSeed);
				imgParticipant = view.FindViewById<ImageView> (Resource.Id.imgParticipantView);
			}

			public void Bind (Activity _context, string _displayName, string _username, int _seed, string _imgURL, 
			                  int _id, int _rank)
			{
				Context = _context;

				displayName = _displayName;
				userName = _username;
				seed = _seed;
				imgURL = _imgURL;
				id = _id;
				rank = _rank;


				txtDisplayName.Text = displayName;
				if (userName != null)
					txtUsername.Text = Context.GetString (Resource.String.strUsername) + userName;
				else
					txtUsername.Text = Context.GetString (Resource.String.strUsername) + "N/A";

				txtSeed.Text = seed.ToString ();
				if (userName == null)
					imgParticipant.SetImageBitmap (BitmapFactory.DecodeResource (Context.Resources, Resource.Drawable.ic_challonge));
				if (dictImgIdParticipant.idParticipantImageCache.ContainsKey (id))
					imgParticipant.SetImageBitmap (dictImgIdParticipant.idParticipantImageCache [id]);
				else {
					if (imgURL != null) {
						string url = imgURL;

						if (url.Substring (0, 2) == "//")
							url = "http:" + url;

						var webClient = new WebClient ();
						var imageData = webClient.DownloadData (new Uri (url));

						Bitmap image = null;

						if (imageData != null && imageData.Length > 0) {
							image = BitmapFactory.DecodeByteArray (imageData, 0, imageData.Length);
						}

						imgParticipant.SetImageBitmap (image);

						dictImgIdParticipant.idParticipantImageCache.Add (id, image);
					}
				}
				
			}
		}
	}
}


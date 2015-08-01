using System;
using System.Reflection;
using Android.Widget;
using Android.App;
using System.Collections.Generic;
using Android.Views;
using Android.Graphics;
using Android.Content;

namespace Challonger
{
    public class TournamentListInfo
    {
        public string tName { get; set; }

        public int tCount { get; set; }

        public string tType{ get; set; }

        public int tState { get; set; }

        public int tProgress{ get; set; }
    }

    public class TournamentListInfoListAdapter : BaseAdapter<TournamentListInfo>
    {
        Activity context;

        List<TournamentListInfo> list;

        public TournamentListInfoListAdapter(Activity _context, List<TournamentListInfo> _list)
            : base()
        {
            this.context = _context;
            this.list = _list;
        }

        public override int Count
        {
            get{ return list.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override TournamentListInfo this [int index]
        {
            get { return list[index]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder vh;

            View view = convertView;

            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.TournamentListLayout, parent, false);
                vh = new ViewHolder();
                vh.Initalize(view);
                view.Tag = vh;
            }

            var item = this[position];

            vh = (ViewHolder)view.Tag;

            vh.Bind(this.context, item.tName, item.tCount, item.tType, item.tState, item.tProgress);

            return view;
        }

        private class ViewHolder : Java.Lang.Object
        {
            Activity Context;
            TextView txtName;
            TextView txtCount;
            TextView txtType;
            ImageView imgState;
            ProgressBar prog;

            public void Initalize(View view)
            {
                txtName = view.FindViewById<TextView>(Resource.Id.txtTournamentListName);
                txtCount = view.FindViewById<TextView>(Resource.Id.txtTournamentListCount);
                txtType = view.FindViewById<TextView>(Resource.Id.txtTournamentViewType);
                imgState = view.FindViewById<ImageView>(Resource.Id.imgTournamentViewState);
                prog = view.FindViewById<ProgressBar>(Resource.Id.progTournamentView);
            }

            public void Bind(Activity _context, string _name, int _count, string _type, int _state, int _prog)
            {
                Context = _context;
                txtName.Text = _name;
                txtCount.Text = _count.ToString();

                switch (_type)
                {
                    case "single elimination":
                        txtType.Text = "SE";
                        break;
                    case "double elimination":
                        txtType.Text = "DE";
                        break;
                    case "round robin":
                        txtType.Text = "RR";
                        break;
                    case "swiss":
                        txtType.Text = "SW";
                        break;
                }

                switch (_state)
                {
                    case 0:
                        imgState.SetImageBitmap(BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.ic_pending));
                        break;
                    case 1:
                        imgState.SetImageBitmap(BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.ic_tournament_inprogress));
                        break;
                    case 2:
                        imgState.SetImageBitmap(BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.ic_tournament_awaiting_review));
                        break;
                    case 3:
                        imgState.SetImageBitmap(BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.ic_tournament_complete));
                        break;
                }

                prog.Progress = _prog;
            }
        }
    }
}


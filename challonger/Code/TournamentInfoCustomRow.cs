using System;
using Android.Widget;
using Android.App;
using System.Collections.Generic;
using Android.Views;
using Android.Content;

namespace Challonger
{
    public class TournamentInfo
    {
        public string top{ get; set; }

        public string bottom{ get; set; }

        public TournamentInfo(string _top, string _bottom)
        {
            top = _top;
            bottom = _bottom;
        }
    }

    public class TournamentInfoListAdapter : BaseAdapter<TournamentInfo>
    {
        Activity context;

        List<TournamentInfo> list;

        public TournamentInfoListAdapter(Activity _context, List<TournamentInfo> _list)
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

        public override TournamentInfo this [int index]
        {
            get{ return list[index]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            ViewHolder vh;

            View view = convertView;

            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.TournamentInfoListLayout, parent, false);
                vh = new ViewHolder();
                vh.Initalize(view);
                view.Tag = vh;
            }				

            var item = this[position];

            vh = (ViewHolder)view.Tag;

            vh.Bind(item.top, item.bottom);

            return view;
        }

        private class ViewHolder : Java.Lang.Object
        {
            TextView txtTop;
            TextView txtBottom;

            public void Initalize(View view)
            {
                txtTop = view.FindViewById<TextView>(Resource.Id.txtTournamentInfoTop);
                txtBottom = view.FindViewById<TextView>(Resource.Id.txtTournamentInfoBottom);
            }

            public void Bind(string _top, string _bottom)
            {
                txtTop.Text = _top;
                txtBottom.Text = _bottom;
            }
        }
    }
}


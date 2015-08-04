/*SETTING UP CODE FOR EXPANDABLE LIST VIEW, CURRENTLY NOT IMPLEMENTED

using System;
using Android.Widget;
using Android.App;
using Android.Views;
using Android.Content;
using System.Text;
using System.Collections.Generic;

namespace Challonger
{
     public class ExpandableListAdapter : BaseExpandableListAdapter
    {
        protected Activity context;
        protected List<TestData> TestDataList;

        public ExpandableListAdapter(Activity _context, List<TestData> _list)
        {
            context = _context;
            TestDataList = _list;
        }

        public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
        {
            return null;
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return childPosition;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            return 1;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                var inflater = context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                view = inflater.Inflate(Resource.Layout.ExpandableListChild, null);
            }

            //setup childview here
            view.FindViewById<TextView>(Resource.Id.txtInfoSubdomain).Text = TestDataList[groupPosition].Id;

            return view;
        }

        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            return null;
        }

        public override long GetGroupId(int groupPosition)
        {
            return groupPosition;
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                var inflater = context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                view = inflater.Inflate(Resource.Layout.ExpandableListParent, null);
            }

            //setup groupview here
            view.FindViewById<TextView>(Resource.Id.txtExpandableTestParent).Text = TestDataList[groupPosition].Value;
            return view;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return true;
        }

        public override bool HasStableIds
        {
            get{ return true; }    
        }

        public override int GroupCount
        {
            get{ return TestDataList.Count; }
        }
    }

    public class TestData
    {
        public string Id { get; set; }

        public string Value { get; set; }

        public TestData(string _id, string _value)
        {
            Id = _id;
            Value = _value;
        }
       
    }

    public class TestDataAdapter : BaseAdapter<TestData>
    {
        protected Activity context;
        protected List<TestData> list;

        public TestDataAdapter(Activity _context, List<TestData> _list)
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

        public override TestData this [int index]
        {
            get{ return list[index]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;

            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.ExpandableListChild, null);
            }

            view.FindViewById<TextView>(Resource.Id.txtInfoSubdomain).Text = this[position].Id;
            return view;
        }
        
    }
}*/
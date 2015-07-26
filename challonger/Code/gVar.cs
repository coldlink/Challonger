using System;
using Android.App;
using Java.Util;
using System.Collections.Generic;

namespace Challonger
{
	public class gVar
	{
		public static string URL_ { get { return "https://api.challonge.com/v1/"; } }

		public static string apiKey_{ get; set; }

		public static Dictionary<int,string> dictMIdIdent;

		public static Dictionary<int,string> dictIdName;

		public static bool boolTournamentEditModeEnabled { get; set; }

		public static int lastViewTournamentInfoTabSelected { get; set; }

	}
}


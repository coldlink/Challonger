using System;
using System.Threading.Tasks;
using System.Json;
using System.Net;
using System.IO;
using Android.App;
using Android.Content;

namespace Challonger
{
	public class Json
	{
		static public async Task<JsonValue> GetJson (string url)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "GET";

			using (WebResponse response = await request.GetResponseAsync ()) {
				using (Stream stream = response.GetResponseStream ()) {
					JsonValue jsonDoc = await Task.Run (() => JsonObject.Load (stream));

					return jsonDoc;
				}
			}	

		}

		static public async Task<bool> PutJson (string url, string jsonPut)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "PUT";

			//streamdata
			using (StreamWriter streamWriter = new StreamWriter (request.GetRequestStream ())) {
				streamWriter.Write (jsonPut);
			}

			using (WebResponse response = await request.GetResponseAsync ()) {
				// Get a stream representation of the HTTP web response:
				using (StreamReader stream = new StreamReader (response.GetResponseStream ())) {
					var responseText = stream.ReadToEnd ();
					return true;
				}
			}
		}

		static public async Task<bool> PostJson (string url, string jsonPOST)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "POST";

			using (StreamWriter streamWriter = new StreamWriter (request.GetRequestStream ())) {
				streamWriter.Write (jsonPOST);
			}

			using (WebResponse response = await request.GetResponseAsync ()) {
				using (StreamReader stream = new StreamReader (response.GetResponseStream ())) {
					var responseText = stream.ReadToEnd ();
					return true;
				}
			}
		}

		static public async Task<JsonValue> PostRetJson (string url, string jsonPOST)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "POST";

			//streamdata
			using (StreamWriter streamWriter = new StreamWriter (request.GetRequestStream ())) {
				streamWriter.Write (jsonPOST);
			}

			using (WebResponse response = await request.GetResponseAsync ()) {
				using (Stream stream = response.GetResponseStream ()) {
					JsonValue jsonDoc = await Task.Run (() => JsonObject.Load (stream));

					return jsonDoc;
				}
			}
		}

		static public async Task<bool> PostJson (string url)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "POST";

			using (WebResponse response = await request.GetResponseAsync ()) {
				using (StreamReader stream = new StreamReader (response.GetResponseStream ())) {
					var responseText = stream.ReadToEnd ();
					return true;
				}
			}
		}

		static public async Task<bool> DeleteJson (string url)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "DELETE";

			using (WebResponse response = await request.GetResponseAsync ()) {
				using (StreamReader stream = new StreamReader (response.GetResponseStream ())) {
					var responseText = stream.ReadToEnd ();
					return true;
				}
			}
		}
	}
}



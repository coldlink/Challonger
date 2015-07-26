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
			// Create an HTTP web request using the URL:
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "GET";

			// Send the request to the server and wait for the response:
			using (WebResponse response = await request.GetResponseAsync ()) {
				// Get a stream representation of the HTTP web response:
				using (Stream stream = response.GetResponseStream ()) {
					// Use this stream to build a JSON document object:
					JsonValue jsonDoc = await Task.Run (() => JsonObject.Load (stream));
					//Console.Out.WriteLine("Response: {0}", jsonDoc.ToString ());

					// Return the JSON document:
					return jsonDoc;
				}
			}	

		}

		static public async Task<bool> PutJson (string url, string jsonPut)
		{
			// Create an HTTP web request using the URL:
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "PUT";

			//streamdata
			using (StreamWriter streamWriter = new StreamWriter (request.GetRequestStream ())) {
				streamWriter.Write (jsonPut);
			}

			// Send the request to the server and wait for the response:
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
			// Create an HTTP web request using the URL:
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "POST";

			//streamdata
			using (StreamWriter streamWriter = new StreamWriter (request.GetRequestStream ())) {
				streamWriter.Write (jsonPOST);
			}

			// Send the request to the server and wait for the response:
			using (WebResponse response = await request.GetResponseAsync ()) {
				// Get a stream representation of the HTTP web response:
				using (StreamReader stream = new StreamReader (response.GetResponseStream ())) {
					var responseText = stream.ReadToEnd ();
					return true;
				}
			}
		}

		static public async Task<JsonValue> PostRetJson (string url, string jsonPOST)
		{
			// Create an HTTP web request using the URL:
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "POST";

			//streamdata
			using (StreamWriter streamWriter = new StreamWriter (request.GetRequestStream ())) {
				streamWriter.Write (jsonPOST);
			}

			// Send the request to the server and wait for the response:
			using (WebResponse response = await request.GetResponseAsync ()) {
				// Get a stream representation of the HTTP web response:
				using (Stream stream = response.GetResponseStream ()) {
					// Use this stream to build a JSON document object:
					JsonValue jsonDoc = await Task.Run (() => JsonObject.Load (stream));
					//Console.Out.WriteLine("Response: {0}", jsonDoc.ToString ());

					// Return the JSON document:
					return jsonDoc;
				}
			}
		}

		static public async Task<bool> PostJson (string url)
		{
			// Create an HTTP web request using the URL:
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "POST";

			// Send the request to the server and wait for the response:
			using (WebResponse response = await request.GetResponseAsync ()) {
				// Get a stream representation of the HTTP web response:
				using (StreamReader stream = new StreamReader (response.GetResponseStream ())) {
					var responseText = stream.ReadToEnd ();
					return true;
				}
			}
		}

		static public async Task<bool> DeleteJson (string url)
		{
			// Create an HTTP web request using the URL:
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (new Uri (url));
			request.ContentType = "application/json";
			request.Method = "DELETE";

			// Send the request to the server and wait for the response:
			using (WebResponse response = await request.GetResponseAsync ()) {
				// Get a stream representation of the HTTP web response:
				using (StreamReader stream = new StreamReader (response.GetResponseStream ())) {
					var responseText = stream.ReadToEnd ();
					return true;
				}
			}
		}
	}
}



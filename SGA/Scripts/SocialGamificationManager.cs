using SocialGamification;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Experimental.Networking;
using UnityEngine.SocialPlatforms;

namespace SocialGamification
{
	#region Generic enums

	/// <summary>
	/// Contact type.
	/// </summary>
	public enum eContactType : int
	{
		Friend = 0,
		Request,
		Ignore
	}

	/// <summary>
	/// Mail list.
	/// </summary>
	public enum eMailList : int
	{
		Received = 0,
		Sent,
		Both
	}

	/// <summary>
	/// Custom search operator.
	/// </summary>
	public enum eSearchOperator
	{
		Equals,
		Disequals,
		Like,
		Greater,
		GreaterOrEquals,
		Lower,
		LowerOrEquals
	}

	/// <summary>
	/// Leaderboard interval.
	/// </summary>
	public enum eLeaderboardInterval : int
	{
		Total = 0,
		Month,
		Week,
		Today
	}

	/// <summary>
	/// Leaderboard time scope.
	/// </summary>
	public enum eLeaderboardTimeScope : int
	{
		None,
		Month
	}

	#endregion Generic enums

	#region Generic classes

	/// <summary>
	/// Search custom data.
	/// </summary>
	[Serializable]
	public class SearchCustomData
	{
		public string key;
		public eSearchOperator op;
		public string value;

		public SearchCustomData(string key, eSearchOperator op, string value)
		{
			this.key = key;
			this.op = op;
			this.value = value;
		}
	}

	/// <summary>
	/// SocialGamification server info.
	/// </summary>
	public class SocialGamificationServerInfo
	{
		public string version = string.Empty;
		public DateTime time = DateTime.MinValue;
		public Hashtable settings = new Hashtable();

		public SocialGamificationServerInfo()
		{
		}

		public SocialGamificationServerInfo(Hashtable data)
		{
			if (data.ContainsKey("version") && data["version"] != null)
				version = data["version"].ToString();
			if (data.ContainsKey("time") && data["time"] != null)
				DateTime.TryParseExact(data["time"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out time);
			if (data.ContainsKey("settings") && data["settings"] != null)
				settings = data["settings"].ToString().hashtableFromJson();
		}

		public override string ToString()
		{
			string versionCompare;
			switch (version.CompareTo(SocialGamificationManager.SocialGamification_VERSION))
			{
				case -1:
					versionCompare = "lower: " + SocialGamificationManager.SocialGamification_VERSION;
					break;

				case 1:
					versionCompare = "greater: " + SocialGamificationManager.SocialGamification_VERSION;
					break;

				default:
					versionCompare = "match";
					break;
			}
			return string.Format("[SocialGamification Server Info] Version: {0} | Time: {1}",
								  version + " (" + versionCompare + ")",
								  time.ToShortDateString() + " " + time.ToShortTimeString());
		}
	}

	#endregion Generic classes

	/// <summary>
	/// SocialGamification Manager class, it follows the Singleton pattern design (accessible through the 'instance' static property),
	/// it means that you shouldn't have more than one instance of this component in your scene.
	/// </summary>
	public class SocialGamificationManager : MonoBehaviour
	{
		/// <summary>
		/// Current API version.
		/// </summary>
		public const string SocialGamification_VERSION = "0.0.1";

		/// <summary>
		/// The singleton instance of SocialGamificationManager
		/// </summary>
		protected static SocialGamificationManager _instance;

		/// <summary>
		/// The singleton instance of SocialGamificationPlatform.
		/// </summary>
		protected static SocialGamificationPlatform _platform;

		/// <summary>
		/// Gets the current singleton instance.
		/// </summary>
		/// <value>The instance.</value>
		public static SocialGamificationManager instance { get { return _instance; } }

		/// <summary>
		/// Gets the SocialGamification ISocialPlatform implementation.
		/// </summary>
		/// <value>The platform.</value>
		public static SocialGamificationPlatform platform { get { return _platform; } }

		/// <summary>
		/// Gets the local user.
		/// </summary>
		/// <value>The local user.</value>
		public static User localUser { get { return (_platform == null ? null : (User)_platform.localUser); } }

		/// <summary>
		/// Gets a value indicating whether the Singleton instance of <see cref="SocialGamification.SocialGamificationManager"/> is initialized.
		/// </summary>
		/// <value><c>true</c> if is initialized; otherwise, <c>false</c>.</value>
		public static bool isInitialized { get { return (_instance != null && _instance._serverInfo != null); } }

		/// <summary>
		/// Should call DontDestroyOnLoad on the SocialGamificationManager gameObject?
		/// Recommended: set to true
		/// </summary>
		public bool dontDestroyOnLoad = true;

		/// <summary>
		/// Should set SocialGamification as the active social platform? The previous platform is accessible from defaultSocialPlatform
		/// </summary>
		public bool setAsDefaultSocialPlatform;

		/// <summary>
		/// The secret key: it must match the define <strong>SECRET_KEY</strong> configured on the web.
		/// </summary>
		public string secretKey;

		/// <summary>
		/// The URL root for the production environment.
		/// </summary>
		public string urlRootProduction;

		/// <summary>
		/// The URL root for the stage environment.
		/// </summary>
		public string urlRootStage;

		/// <summary>
		/// If <em>true</em> sets the stage as current environment (default: false for production).
		/// </summary>
		public bool useStage;

		/// <summary>
		/// Print debug info in the console log.
		/// </summary>
		public bool logDebugInfo;

		/// <summary>
		/// The ping interval in seconds (set 0 to disable automatic pings).
		/// Ping is currently used to mantain the online state of the local user
		/// and is automatically called only is the local user is authenticated.
		/// </summary>
		public float pingIntervalSeconds = 30f;

		/// <summary>
		/// The max seconds from now to a user's <strong>lastSeen</strong> to consider the online state.
		/// </summary>
		public int onlineSeconds = 120;

		/// <summary>
		/// The max seconds from now to a user's <strong>lastSeen</strong> to consider the playing state.
		/// </summary>
		public int playingSeconds = 120;

		/// <summary>
		/// Can be used to filter the current system date timezone (the value must be valid in PHP: http://www.php.net/manual/en/timezones.php).
		/// </summary>
		public string timezone;

		/// <summary>
		/// The achievement user interface object for SocialGamificationPlatform.ShowAchievementsUI().
		/// </summary>
		public GameObject achievementUIObject;

		/// <summary>
		/// The achievement user interface function for SocialGamificationPlatform.ShowAchievementsUI().
		/// </summary>
		public string achievementUIFunction;

		/// <summary>
		/// The leaderboard user interface object for SocialGamificationPlatform.ShowLeaderboardUI().
		/// </summary>
		public GameObject leaderboardUIObject;

		/// <summary>
		/// The leaderboard user interface function for SocialGamificationPlatform.ShowLeaderboardUI().
		/// </summary>
		public string leaderboardUIFunction;

		protected ISocialPlatform _defaultSocialPlatform;
		protected SocialGamificationServerInfo _serverInfo;
		protected bool downloading, cancelling;

		/// <summary>
		/// Gets the default social platform defined (this is set before SocialGamification is set as activate, eventually).
		/// </summary>
		/// <value>The default social platform.</value>
		public ISocialPlatform defaultSocialPlatform { get { return _defaultSocialPlatform; } }

		/// <summary>
		/// Gets a value indicating whether this <see cref="SocialGamification.SocialGamificationManager"/> is downloading from a webservice.
		/// </summary>
		/// <value><c>true</c> if is downloading; otherwise, <c>false</c>.</value>
		public bool isDownloading { get { return downloading; } }

		/// <summary>
		/// Gets a value indicating whether this <see cref="SocialGamification.SocialGamificationManager"/> is cancelling a webservice request.
		/// </summary>
		/// <value><c>true</c> if is cancelling; otherwise, <c>false</c>.</value>
		public bool isCancelling { get { return cancelling; } }

		/// <summary>
		/// Gets a value indicating whether <see cref="SocialGamification.SocialGamificationManager.localUser"/> is authenticated.
		/// </summary>
		/// <value><c>true</c> if is authenticated; otherwise, <c>false</c>.</value>
		public bool isAuthenticated { get { return (localUser != null && localUser.authenticated); } }

        /// <summary>
        /// Gets all the ItemTypes stored in the database
        /// </summary>
        public List<ItemType> ItemTypes { get; set; }

		/// <summary>
		/// Gets the server info.
		/// </summary>
		/// <value>The server info.</value>
		public SocialGamificationServerInfo serverInfo { get { return _serverInfo; } }

		public const string SessionHeaderName = "X-HTTP-Session";

		protected virtual void Awake()
		{
			// Ensure we have one only instance
			if (_instance != null)
			{
				Destroy(this);
			}
			else
			{
				if (dontDestroyOnLoad)
					DontDestroyOnLoad(gameObject);
				_instance = this;
				downloading = false;
				cancelling = false;
				_defaultSocialPlatform = Social.Active;
				_platform = new SocialGamificationPlatform();
				if (setAsDefaultSocialPlatform)
					Social.Active = _platform;
			}
		}

		private void Start()
		{
			if (!urlRootProduction.EndsWith("/"))
				urlRootProduction += "/";
			if (!urlRootStage.EndsWith("/"))
				urlRootStage += "/";

			// Get the server info
			GetServerInfo((bool success, SocialGamificationServerInfo loadedInfo) =>
			{
				if (success)
				{
					_serverInfo = loadedInfo;
					Debug.Log(_serverInfo.ToString());
				}
				else
				{
					_serverInfo = new SocialGamificationServerInfo();
					Debug.LogError("Failed to get SocialGamification server info");
				}
			});
		}

        /// <summary>
        /// Get all the relevant information from the server. Should be called once after login.
        /// </summary>
        public void Init()
        {
            GetItemTypes();
        }

        /// <summary>
        /// Gets the item types.
        /// </summary>
        public virtual void GetItemTypes()
        {
            if (ItemTypes == null)
            {
                ItemTypes = new List<ItemType>();
            }

            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/items/types"), null, (string text, string error) =>
            {
                Debug.Log("Get ItemTypes: " + text);
                if (string.IsNullOrEmpty(error))
                {
                    ArrayList list = text.arrayListFromJson();
                    if (list != null)
                    {
                        foreach (Hashtable itemhash in list)
                        {
                            if (itemhash.ContainsKey("name") && itemhash["name"] != null)
                            {
                                ItemType itemType = new ItemType(itemhash);
                                ItemTypes.Add(itemType);
                            }
                        }
                    }
                }

                Debug.Log(ItemTypes);
            });
        }

		private void OnEnable()
		{
			if (pingIntervalSeconds > 0)
				InvokeRepeating("AutoPing", 0, pingIntervalSeconds);
		}

		private void OnDisable()
		{
			if (pingIntervalSeconds > 0)
				CancelInvoke("AutoPing");
		}

		#region Utilities

		/// <summary>
		/// This is InvokeRepeating on enable and automatically pings the server. If the user was authenticated and the ping fails, the local user is automatically disconnected.
		/// </summary>
		private void AutoPing()
		{
			//Ping(true,  null);
			Ping(true, (bool success) =>
			{
				if (!success && localUser.authenticated)
					_platform.SetLocalUser(new User());
			});
		}

		/// <summary>
		/// Calls a webservice.
		/// </summary>
		/// <param name='url'>
		/// URL.
		/// </param>
		/// <param name='form'>
		/// Form.
		/// </param>
		/// <param name='onComplete'>
		/// On complete callback.
		/// </param>
		public void CallWebservice(string url, Dictionary<string, string> form, System.Action<string, string> onComplete, string method = "DEFAULT")
		{
			if (!string.IsNullOrEmpty(timezone) && form != null)
				form.Add("Timezone", timezone);
			StartCoroutine(DownloadUrl(url, form, onComplete, method));
		}

		/// <summary>
		/// Cancels the current request (next frame).
		/// </summary>
		public void CancelRequest()
		{
			if (downloading && !cancelling)
				cancelling = true;
		}

		/// <summary>
		/// Cancels all the current requests (immediately).
		/// </summary>
		//		public void CancelAllRequests ()
		//		{
		//			StopAllCoroutines();
		//		}

		/// <summary>
		/// Downloads the content of an URL with the specified form.
		/// </summary>
		/// <returns>The URL.</returns>
		/// <param name="url">URL.</param>
		/// <param name="form">Form.</param>
		/// <param name="onComplete">On complete.</param>
		protected IEnumerator DownloadUrl(string url, Dictionary<string, string> form, Action<string, string> onComplete, string method = "DEFAULT")
		{
			// Set the flag to know that it's downloading
			downloading = true;

			if (form == null)
			{
				form = new Dictionary<string, string>();
			}

			// Add secure checksum to the form
			//SecureRequest(form);
			if (logDebugInfo)
			{
				Debug.Log("Sending: " + method + " " + url + " with: " + form.toJson());
			}

			// Call the webservice
			if (method == "DEFAULT")
			{
				if (form.Count > 0)
				{
					return DownloadURLPOST(url, form, onComplete);
				}
				else
				{
					return DownloadURLGET(url, onComplete);
				}
			}
			else if (method == "GET")
			{
				return DownloadURLGET(url, onComplete);
			}
			else if (method == "POST")
			{
				return DownloadURLPOST(url, form, onComplete);
			}
			else if (method == "PUT")
			{
				return DownloadURLPUT(url, form, onComplete);
			}
			else if (method == "DELETE")
			{
				return DownloadURLDELETE(url, form, onComplete);
			}
			return null;
		}

		protected IEnumerator DownloadURLGET(string url, Action<string, string> onComplete)
		{
			UnityWebRequest www = null;
			www = UnityWebRequest.Get(url);

			www.SetRequestHeader("Content-Type", "text/json");
			if (localUser.authenticated)
			{
				www.SetRequestHeader(SessionHeaderName, localUser.sessionId);
			}
			yield return www.Send();
			bool cancelled = false;
			while (true)
			{
				if (www.isDone)
					break;
				if (cancelling)
				{
					cancelled = true;
					break;
				}
				yield return null;
			}
			downloading = false;
			cancelling = false;
			string error = (cancelled ? "Request cancelled" : www.error);
			string text = (string.IsNullOrEmpty(error) ? www.downloadHandler.text : "");
			if (logDebugInfo)
				Debug.Log(text);
			if (onComplete != null)
				onComplete(text, error);
		}

		protected IEnumerator DownloadURLPOST(string url, Dictionary<string, string> form, Action<string, string> onComplete)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers["Content-Type"] = "text/json";

			if (form.Count > 0)
			{
				headers["Content-Length"] = form.toJson().Length.ToString();
			}

			if (localUser.authenticated)
			{
				headers[SessionHeaderName] = localUser.sessionId;
			}

			byte[] formBytes = null;

			if (form == null)
			{
				form = new Dictionary<string, string>();
			}

			formBytes = System.Text.ASCIIEncoding.Default.GetBytes(form.toJson());

			WWW www = new WWW(url, formBytes, headers);

			bool cancelled = false;
			while (true)
			{
				if (www.isDone)
				{
					break;
				}

				if (cancelling)
				{
					cancelled = true;
					break;
				}

				yield return null;
			}

			downloading = false;
			cancelling = false;
			string error = (cancelled ? "Request cancelled" : www.error);
			string text = (string.IsNullOrEmpty(error) ? www.text : "");
			if (logDebugInfo)
			{
				Debug.Log(text);
			}

			if (onComplete != null)
			{
				onComplete(text, error);
			}
		}

		protected IEnumerator DownloadURLPUT(string url, Dictionary<string, string> form, Action<string, string> onComplete)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers["Content-Type"] = "text/json";
			headers["X-HTTP-Method-Override"] = "PUT";

			string jsonString = form.toJson();

			Debug.Log(jsonString);

			headers["X-HTTP-BODY"] = jsonString;

			if (localUser.authenticated)
			{
				headers[SessionHeaderName] = localUser.sessionId;
			}

			byte[] formBytes = null;

			if (form == null)
			{
				form = new Dictionary<string, string>();
			}
			formBytes = System.Text.ASCIIEncoding.Default.GetBytes(jsonString);

			WWW www = new WWW(url, formBytes, headers);

			bool cancelled = false;
			while (true)
			{
				if (www.isDone)
					break;
				if (cancelling)
				{
					cancelled = true;
					break;
				}
				yield return null;
			}
			downloading = false;
			cancelling = false;
			string error = (cancelled ? "Request cancelled" : www.error);
			string text = (string.IsNullOrEmpty(error) ? www.text : "");
			if (logDebugInfo)
				Debug.Log(text);
			if (onComplete != null)
				onComplete(text, error);
		}

		protected IEnumerator DownloadURLDELETE(string url, Dictionary<string, string> form, Action<string, string> onComplete)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers["Content-Type"] = "text/json";
			headers["X-HTTP-Method-Override"] = "DELETE";

			string jsonString = form.toJson();

			Debug.Log(jsonString);

			headers["X-HTTP-BODY"] = jsonString;

			if (localUser.authenticated)
			{
				headers[SessionHeaderName] = localUser.sessionId;
			}

			byte[] formBytes = null;

			if (form == null)
			{
				form = new Dictionary<string, string>();
			}
			formBytes = System.Text.ASCIIEncoding.Default.GetBytes(jsonString);

			WWW www = new WWW(url, formBytes, headers);

			bool cancelled = false;
			while (true)
			{
				if (www.isDone)
					break;
				if (cancelling)
				{
					cancelled = true;
					break;
				}
				yield return null;
			}
			downloading = false;
			cancelling = false;
			string error = (cancelled ? "Request cancelled" : www.error);
			string text = (string.IsNullOrEmpty(error) ? www.text : "");
			if (logDebugInfo)
				Debug.Log(text);
			if (onComplete != null)
				onComplete(text, error);
		}

		/// <summary>
		/// Creates a new form to be passed to a webservice.
		/// </summary>
		/// <returns>
		/// The form.
		/// </returns>
		public WWWForm CreateForm()
		{
			WWWForm form = new WWWForm();
			if (localUser.authenticated)
			{
				form.AddField("UID", localUser.id);
				form.AddField("UGUID", localUser.sessionToken);
			}
			return form;
		}

		/// <summary>
		/// Secure the WWWForm by signing the request.
		/// </summary>
		/// <param name='form'>
		/// Form.
		/// </param>
		protected void SecureRequest(WWWForm form)
		{
			// Exit if secret key is not defined
			if (string.IsNullOrEmpty(secretKey))
				return;

			// Get the current timestamp
			string data = System.DateTime.Now.Ticks.ToString();

			// Create the signature by merging and encrypting the timestamp, the user session token and the secret key
			string signature = EncryptSHA1(data + (localUser == null ? "" : localUser.sessionToken) + secretKey);

			// Add the signature parameters to the request
			form.AddField("sig_time", data);
			form.AddField("sig_crc", signature);
		}

		/// <summary>
		/// Gets the absolute URL from a relative.
		/// </summary>
		/// <returns>
		/// The URL.
		/// </returns>
		/// <param name='relativeUrl'>
		/// Relative URL.
		/// </param>
		public string GetUrl(string relativeUrl)
		{
			Uri uri = new Uri(new Uri(useStage ? urlRootStage : urlRootProduction), relativeUrl);
			return uri.ToString();
		}

		/// <summary>
		/// Captures the screen shot.
		/// </summary>
		/// <returns>The screen shot.</returns>
		public byte[] CaptureScreenShot()
		{
			return CaptureScreenShot(118, 85);
		}

		/// <summary>
		/// Captures the screen shot with specified size.
		/// </summary>
		/// <returns>The screen shot.</returns>
		/// <param name="thumbnailWidth">Thumbnail width.</param>
		/// <param name="thumbnailHeight">Thumbnail height.</param>
		public byte[] CaptureScreenShot(int thumbnailWidth, int thumbnailHeight)
		{
			byte[] result;

			RenderTexture rt = new RenderTexture(thumbnailWidth, thumbnailHeight, 24);
			Texture2D screenShot = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);

			Camera.main.targetTexture = rt;
			Camera.main.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
			screenShot.Apply();

			Camera.main.targetTexture = null;
			RenderTexture.active = null;

			result = screenShot.EncodeToPNG();

			DestroyImmediate(rt);
			DestroyImmediate(screenShot);

			return result;
		}

		/// <summary>
		/// Encrypts a string in MD5.
		/// </summary>
		/// <returns>The M d5.</returns>
		/// <param name="inputString">Input string.</param>
		public static string EncryptMD5(string inputString)
		{
			System.Security.Cryptography.MD5 encrypt = new System.Security.Cryptography.MD5CryptoServiceProvider();

			//compute hash from the bytes of text
			encrypt.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(inputString));

			//get hash result after compute it
			byte[] result = encrypt.Hash;
			System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
			for (int i = 0; i < result.Length; i++)
			{
				//change it into 2 hexadecimal digits for each byte
				strBuilder.Append(result[i].ToString("x2"));
			}
			return strBuilder.ToString();
		}

		/// <summary>
		/// Encrypts a string in SHA1.
		/// </summary>
		/// <returns>The SH a1.</returns>
		/// <param name="inputString">Input string.</param>
		public static string EncryptSHA1(string inputString)
		{
			System.Security.Cryptography.SHA1 encrypt = new System.Security.Cryptography.SHA1CryptoServiceProvider();

			//compute hash from the bytes of text
			encrypt.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(inputString));

			//get hash result after compute it
			byte[] result = encrypt.Hash;
			System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
			for (int i = 0; i < result.Length; i++)
			{
				//change it into 2 hexadecimal digits
				//for each byte
				strBuilder.Append(result[i].ToString("x2"));
			}
			return strBuilder.ToString();
		}

		#endregion Utilities

		/// <summary>
		/// Gets the server info.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void GetServerInfo(Action<bool, SocialGamificationServerInfo> callback)
		{
			CallWebservice(GetUrl("api/server"), null, (string text, string error) =>
			{
				bool success = false;
				Hashtable data = new Hashtable();
				if (string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(text))
				{
					data = text.hashtableFromJson();
					if (data != null)
						success = true;
				}
				SocialGamificationServerInfo info = null;
				if (success)
					info = new SocialGamificationServerInfo(data);
				if (callback != null)
					callback(success, info);
			});
		}

		/// <summary>
		/// Ping the server.
		/// </summary>
		/// <param name="onlyIfAuthenticated">If set to <c>true</c> then it runs only if local user is authenticated.</param>
		/// <param name="callback">Callback.</param>
		public void Ping(bool onlyIfAuthenticated = true, Action<bool> callback = null)
		{
			// Skip if SocialGamificationManager is not initialized
			if (!isInitialized)
				return;

			// If requested, do pings only if local user is authenticated
			if (onlyIfAuthenticated && !isAuthenticated)
			{
				if (callback != null)
					callback(false);
				return;
			}
			CallWebservice(GetUrl("api/server"), null, (string text, string error) =>
			{
				bool success = false;
				Hashtable data = new Hashtable();
				if (string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(text))
				{
					data = text.hashtableFromJson();
					if (data != null)
					{
						success = bool.Parse(data["success"].ToString());
					}
				}
				if (callback != null)
					callback(success);
			});
		}
	}
}

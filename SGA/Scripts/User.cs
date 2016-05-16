using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace SocialGamification
{
	/// <summary>
	/// User class implementing the Unity built-in Social interfaces (specialized IUserProfile, ILocalUser).
	/// </summary>
	[System.Serializable]
	public class User : Profile, ILocalUser
	{
		private static System.Exception ExceptionSocialGamificationNotInitialized = new System.Exception("SocialGamification Manager not initialized");
		private static System.Exception ExceptionOnlyLocalUser = new System.Exception("This method works only on localUser");

		public string password;

		private Profile[] _friends = new Profile[0];
		private Profile[] _ignored = new Profile[0];
		private Profile[] _requests = new Profile[0];
		private bool _authenticated = false;

		public string sessionId;

		public User()
		{
		}

		public User(bool authenticated)
		{
			_authenticated = authenticated;
		}

		public User(string jsonString)
		{
			FromJson(jsonString);
		}

		public User(Hashtable hash)
		{
			FromHashtable(hash);
		}

		#region ILocalUser implementation

		/// <summary>
		/// Authenticate the user.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void Authenticate(System.Action<bool> callback)
		{
			Authenticate(password, (bool success, string error) =>
			{
				if (callback != null)
					callback(success);
			});
		}

		/// <summary>
		/// Authenticate the user with the specified password.
		/// </summary>
		/// <param name="password">Password.</param>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="T">Type for User.</typeparam>
		public virtual void Authenticate(string password, System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("Username", userName);
			form.Add("Password", password);
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/sessions"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					sessionId = "";
					if (result != null)
					{
						if (result.ContainsKey("id"))
						{
							sessionId = result["id"].ToString();
							success = sessionId.Length > 0;
						}

						if (success && result.ContainsKey("player") && result["player"] != null)
						{
							_authenticated = true;
							FromJson(((Hashtable)result["player"]).toJson());
							SocialGamificationManager.platform.SetLocalUser(this);
						}
						else if (!success && result.ContainsKey("message"))
						{
							error = result["message"].ToString();
						}
					}
				}

				if (callback != null)
				{
					callback(success, error);
				}
			});
		}

		/// <summary>
		/// Loads the friends of the current logged user.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void LoadFriends(System.Action<bool> callback)
		{
			LoadFriends<User>(eContactType.Friend, callback);
		}

		/// <summary>
		/// Loads the friends of the current logged user.
		/// </summary>
		/// <param name="contactType">Contact type.</param>
		/// <param name="callback">Callback.</param>
		public virtual void LoadFriends(eContactType contactType, System.Action<bool> callback)
		{
			LoadFriends<User>(contactType, callback);
		}

		/// <summary>
		/// Loads the friends of the current logged user.
		/// </summary>
		/// <param name="contactType">Contact type.</param>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="T">Type for User.</typeparam>
		public virtual void LoadFriends<T>(eContactType contactType, System.Action<bool> callback) where T : User, new()
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			switch (contactType)
			{
				case eContactType.Friend:
					_friends = new Profile[0];
					break;

				case eContactType.Ignore:
					_ignored = new Profile[0];
					break;

				case eContactType.Request:
					_requests = new Profile[0];
					break;
			}

			//WWWForm form = SocialGamificationManager.instance.CreateForm();
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/friends"), null, (string text, string error) =>
			{
				List<T> users = new List<T>();
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null && result.ContainsKey("results"))
					{
						ArrayList list = (ArrayList)result["results"];
						if (list != null)
						{
							foreach (Hashtable data in list)
							{
								// Create a new user object from the result
								T friend = new T();
								friend.FromHashtable(data);

								// Add to the list
								users.Add(friend);
							}
						}
					}
				}
				switch (contactType)
				{
					case eContactType.Friend:
						_friends = users.ToArray();
						break;

					case eContactType.Ignore:
						_ignored = users.ToArray();
						break;

					case eContactType.Request:
						_requests = users.ToArray();
						break;
				}
				if (callback != null)
					callback(string.IsNullOrEmpty(error));
			});
		}

		public IUserProfile[] friends { get { return _friends; } }

		public IUserProfile[] ignored { get { return _ignored; } }

		public IUserProfile[] requests { get { return _requests; } }

		public bool authenticated { get { return _authenticated; } }

		public virtual bool underage { get { return false; } }

		#endregion ILocalUser implementation

		/// <summary>
		/// Update or Create this user to server, whether id is positive and greater than zero.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void Update(System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
			{
				throw ExceptionSocialGamificationNotInitialized;
			}

			Dictionary<string, string> form = new Dictionary<string, string>();

			string urlString = "api/players/";
			string requestType = "PUT";

			if (!string.IsNullOrEmpty(id))
			{
				// Update existing account
				urlString += id;
			}
			else
			{
				form.Add("Password", password);
				requestType = "POST";
			}

			form.Add("Username", userName);

			if (!string.IsNullOrEmpty(email))
			{
				form.Add("Email", email);
			}

			form.Add("CustomData", customData.toCustomDataJson());

			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl(urlString), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						success = result.ContainsKey("id") && result["id"] != null;
						if (success)
						{
							FromJson(result.ToString());
						}
						else
						{
							error = "API Response doesn't contact ID";
						}
					}
				}
				if (callback != null)
					callback(success, error);
			}, requestType);
		}

        /// <summary>
		/// Update or Create this user to server, whether id is positive and greater than zero.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public static void UpdateCustom(Hashtable providedData, string id, System.Action<bool, string> callback)
        {
            if (!SocialGamificationManager.isInitialized)
            {
                throw ExceptionSocialGamificationNotInitialized;
            }

            Dictionary<string, string> form = new Dictionary<string, string>();

            string urlString = "api/players/";
            string requestType = "PUT";

            urlString += id;

            form.Add("CustomData", providedData.toCustomDataJson());

            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl(urlString), form, (string text, string error) =>
            {
                bool success = false;
                if (string.IsNullOrEmpty(error))
                {
                    Hashtable result = text.hashtableFromJson();
                    if (result != null)
                    {
                        success = result.ContainsKey("id") && result["id"] != null;
                        if (!success)
                        {
                            error = "API Response doesn't contact ID";
                        }
                    }
                }
                if (callback != null)
                    callback(success, error);
            }, requestType);
        }

        /// <summary>
		/// Get custom data for this user by key
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void CustomSearch(string customKey, System.Action<Hashtable, string> callback)
        {
            if (!SocialGamificationManager.isInitialized)
            {
                throw ExceptionSocialGamificationNotInitialized;
            }

            Dictionary<string, string> form = new Dictionary<string, string>();

            string urlString = "api/players/" + id + "/custom";
            string requestType = "PUT";

            form.Add("verb", customKey);

            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl(urlString), form, (string text, string error) =>
            {
                Hashtable customResult = null;
                if (string.IsNullOrEmpty(error))
                {
                    Hashtable result = text.hashtableFromJson();
                    if (result.Keys.Count > 0 && result.ContainsKey("id") && result["id"] != null)
                    {
                        customResult = result;
                    }
                    else
                    {
                        error = "Data not found";
                    }
                }
                if (callback != null)
                    callback(customResult, error);
            }, requestType);
        }

        /// <summary>
        /// Delete this instance from the server.
        /// </summary>
        /// <param name="callback">Callback.</param>
        public virtual void Delete(System.Action<bool, string> callback)
		{
			Delete(userName, password, callback);
		}

		/// <summary>
		/// Delete a user from the server.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="callback">Callback.</param>
		public static void Delete(string username, string password, System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "delete");
			form.Add("Username", username);
			form.Add("Password", password);
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (!success && result.ContainsKey("message"))
							error = result["message"].ToString();
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Load of the current user from server.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void Load(System.Action<bool> callback)
		{
			Load(new string[] { _id }, (User[] users) =>
			{
				if (callback != null)
					callback(users.Length > 0);
			}, this);
		}

		/// <summary>
		/// Loads a user by Id.
		/// </summary>
		/// <param name="userId">User Id.</param>
		/// <param name="callback">Callback.</param>
		public static void Load(string userId, System.Action<User> callback)
		{
			Load(new string[] { userId }, (User[] users) =>
			{
				if (callback != null)
					callback(users.Length > 0 ? users[0] : null);
			});
		}

		/// <summary>
		/// Loads a user by userName.
		/// </summary>
		/// <param name="userName">User Name.</param>
		/// <param name="callback">Callback.</param>
		public static void LoadFromUsername(string userName, System.Action<User> callback)
		{
			LoadFromUsernames(new string[] { userName }, (User[] users) =>
			{
				if (callback != null)
					callback(users.Length > 0 ? users[0] : null);
			});
		}

		/// <summary>
		/// Loads the users by Id.
		/// </summary>
		/// <param name="userIds">User Ids.</param>
		/// <param name="callback">Callback.</param>
		/// <param name="updateUser">If passed its data will be replaced with the server result.</param>
		public static void Load(string[] userIds, System.Action<User[]> callback, User updateUser = null)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			string[] strUserIds = new string[userIds.Length];
			for (int i = 0; i < userIds.Length; ++i)
			{
				strUserIds[i] = userIds[i].ToString();
			}
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "load");
			form.Add("Ids", string.Join(",", strUserIds));
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				List<User> profiles = new List<User>();
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						bool success = false;
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (success)
						{
							ArrayList profilesList = result["message"].ToString().arrayListFromJson();
							if (profilesList != null)
							{
								foreach (Hashtable profileData in profilesList)
								{
									User user = new User(profileData);
									if (updateUser != null)
										updateUser.FromHashtable(profileData);
									profiles.Add(user);
								}
							}
						}
						else if (result.ContainsKey("message") && result["message"] != null)
						{
							error = result["message"].ToString();
						}
					}
				}

				if (callback != null)
					callback(profiles.ToArray());
			});
		}

		/// <summary>
		/// Loads the users by userName.
		/// </summary>
		/// <param name="userNames">User Names.</param>
		/// <param name="callback">Callback.</param>
		public static void LoadFromUsernames(string[] userNames, System.Action<User[]> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "load");
			form.Add("Usernames", string.Join(",", userNames));
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				List<User> profiles = new List<User>();
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						bool success = false;
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (success)
						{
							ArrayList profilesList = result["message"].ToString().arrayListFromJson();
							if (profilesList != null)
							{
								foreach (Hashtable profileData in profilesList)
								{
									User user = new User(profileData);
									profiles.Add(user);
								}
							}
						}
						else if (result.ContainsKey("message") && result["message"] != null)
						{
							error = result["message"].ToString();
						}
					}
				}

				if (callback != null)
					callback(profiles.ToArray());
			});
		}

		/// <summary>
		/// Loads the users by searching for the specified parameters.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="email">Email.</param>
		/// <param name="customData">Custom data.</param>
		/// <param name="pageNumber">Page number.</param>
		/// <param name="limit">Limit.</param>
		/// <param name="callback">Callback.</param>
		public static void Load<T>(string username, string email, SearchCustomData[] customData, bool isOnline, int pageNumber, int limit, System.Action<T[], int, int> callback) where T : User, new()
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;

			// Build the JSON string for CustomData filtering
			string searchCustomData = "";
			if (customData != null)
			{
				ArrayList list = new ArrayList();
				foreach (SearchCustomData data in customData)
				{
					if (string.IsNullOrEmpty(data.key))
						continue;
					Hashtable search = new Hashtable();
					search.Add("key", data.key);
					search.Add("value", data.value);
					switch (data.op)
					{
						case eSearchOperator.Equals:
							search.Add("operator", "=");
							break;

						case eSearchOperator.Disequals:
							search.Add("operator", "!");
							break;

						case eSearchOperator.Like:
							search.Add("operator", "%");
							break;

						case eSearchOperator.Greater:
							search.Add("operator", ">");
							break;

						case eSearchOperator.GreaterOrEquals:
							search.Add("operator", ">=");
							break;

						case eSearchOperator.Lower:
							search.Add("operator", "<");
							break;

						case eSearchOperator.LowerOrEquals:
							search.Add("operator", "<=");
							break;
					}
					list.Add(search);
				}
				if (list.Count > 0)
					searchCustomData = list.toJson();
			}

			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "search");
			form.Add("Limit", limit.ToString());
			form.Add("Page", pageNumber.ToString());

			if (isOnline)
				form.Add("Online", "1");

			if (!string.IsNullOrEmpty(username))
				form.Add("Username", username);

			if (!string.IsNullOrEmpty(email))
				form.Add("Email", email);

			if (!string.IsNullOrEmpty(searchCustomData))
				form.Add("CustomData", searchCustomData);

			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				List<T> profiles = new List<T>();
				int count = 0, pagesCount = 0;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null && result.ContainsKey("total"))
					{
						count = int.Parse(result["total"].ToString());
						pagesCount = int.Parse(result["pages"].ToString());
						ArrayList profilesList = (ArrayList)result["results"];
						if (profilesList != null)
						{
							foreach (Hashtable profileData in profilesList)
							{
								T user = new T();
								user.FromHashtable(profileData);
								profiles.Add(user);
							}
						}
					}
				}

				if (callback != null)
					callback(profiles.ToArray(), count, pagesCount);
			});
		}

		/// <summary>
		/// Loads a specified count of random users.
		/// </summary>
		/// <param name="customData">Custom data.</param>
		/// <param name="count">Count.</param>
		/// <param name="callback">Callback.</param>
		public static void Random<T>(SearchCustomData[] customData, int count, System.Action<T[]> callback) where T : User, new()
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;

			// Build the JSON string for CustomData filtering
			string searchCustomData = "";
			if (customData != null)
			{
				ArrayList list = new ArrayList();
				foreach (SearchCustomData data in customData)
				{
					if (string.IsNullOrEmpty(data.key))
						continue;
					Hashtable search = new Hashtable();
					search.Add("key", data.key);
					search.Add("value", data.value);
					switch (data.op)
					{
						case eSearchOperator.Equals:
							search.Add("operator", "=");
							break;

						case eSearchOperator.Disequals:
							search.Add("operator", "!");
							break;

						case eSearchOperator.Like:
							search.Add("operator", "%");
							break;

						case eSearchOperator.Greater:
							search.Add("operator", ">");
							break;

						case eSearchOperator.GreaterOrEquals:
							search.Add("operator", ">=");
							break;

						case eSearchOperator.Lower:
							search.Add("operator", "<");
							break;

						case eSearchOperator.LowerOrEquals:
							search.Add("operator", "<=");
							break;
					}
					list.Add(search);
				}
				if (list.Count > 0)
					searchCustomData = list.toJson();
			}

			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "random");
			if (!string.IsNullOrEmpty(searchCustomData))
				form.Add("CustomData", searchCustomData);
			form.Add("Count", count.ToString());

			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				List<T> profiles = new List<T>();
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null && result.ContainsKey("total"))
					{
						count = int.Parse(result["total"].ToString());
						ArrayList profilesList = (ArrayList)result["results"];
						if (profilesList != null)
						{
							foreach (Hashtable profileData in profilesList)
							{
								T user = new T();
								user.FromHashtable(profileData);
								profiles.Add(user);
							}
						}
					}
				}

				if (callback != null)
					callback(profiles.ToArray());
			});
		}

		/// <summary>
		/// Verify if it exists an account with the specified username and email.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="email">Email.</param>
		/// <param name="callback">Callback.</param>
		public static void Exists(string username, string email, System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "exists");
			if (!string.IsNullOrEmpty(username))
				form.Add("Username", username);
			if (!string.IsNullOrEmpty(email))
				form.Add("Email", email);
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (success)
						{
							success = bool.Parse(result["message"].ToString());
						}
						else if (result.ContainsKey("message") && result["message"] != null)
						{
							error = result["message"].ToString();
						}
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Resets the password of this user.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void ResetPassword(System.Action<bool, string> callback)
		{
			ResetPassword(_id, string.Empty, callback);
		}

		/// <summary>
		/// Resets the password of a user by Id Account.
		/// </summary>
		/// <param name="idUser">Identifier user.</param>
		/// <param name="callback">Callback.</param>
		public static void ResetPassword(string idUser, System.Action<bool, string> callback)
		{
			ResetPassword(idUser, string.Empty, callback);
		}

		/// <summary>
		/// Resets the password of a user by Username.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="callback">Callback.</param>
		public static void ResetPasswordFromUserName(string username, System.Action<bool, string> callback)
		{
			ResetPassword(string.Empty, username, callback);
		}

		/// <summary>
		/// Resets the password of a user.
		/// </summary>
		/// <param name="idUser">Identifier user.</param>
		/// <param name="username">Username.</param>
		/// <param name="callback">Callback.</param>
		private static void ResetPassword(string idUser, string username, System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "reset_pwd");
			if (!string.IsNullOrEmpty(idUser))
				form.Add("Id", idUser.ToString());
			else if (!string.IsNullOrEmpty(username))
				form.Add("Username", username);
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (!success && result.ContainsKey("message") && result["message"] != null)
						{
							error = result["message"].ToString();
						}
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Changes the password of this user.
		/// </summary>
		/// <param name="newPassword">New password.</param>
		/// <param name="callback">Callback.</param>
		public virtual void ChangePassword(string newPassword, System.Action<bool, string> callback)
		{
			ChangePassword(_id, string.Empty, string.Empty, newPassword, (bool success, string error) =>
			{
				if (success)
					FromJson(error);
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Changes the password of a user.
		/// </summary>
		/// <param name="idUser">Identifier user.</param>
		/// <param name="username">Username.</param>
		/// <param name="resetCode">Reset code.</param>
		/// <param name="newPassword">New password.</param>
		/// <param name="callback">Callback.</param>
		public static void ChangePassword(string idUser, string username, string resetCode, string newPassword, System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "change_pwd");
			form.Add("Password", newPassword);
			if (SocialGamificationManager.localUser.authenticated && SocialGamificationManager.localUser.id.Equals(idUser))
			{
				// We are changing the password of localUser, no extra config needed
			}
			else
			{
				if (!string.IsNullOrEmpty(idUser))
					form.Add("Id", idUser.ToString());
				else if (!string.IsNullOrEmpty(username))
					form.Add("Username", username);
				form.Add("Code", resetCode);
			}
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (result.ContainsKey("message") && result["message"] != null)
						{
							error = result["message"].ToString();
						}
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Authenticates the user from an external platform (like Facebook, Game Center, GooglePlay etc).
		/// Note you are the only responsible for the external authentication, SocialGamification only stores the info that you send.
		/// </summary>
		/// <param name="platformKey">Platform key.</param>
		/// <param name="platformId">Platform identifier.</param>
		/// <param name="callback">Callback.</param>
		public virtual void AuthenticatePlatform(string platformKey, string platformId, System.Action<bool, string> callback)
		{
			AuthenticatePlatform<User>(platformKey, platformId, callback);
		}

		public virtual void AuthenticatePlatform<T>(string platformKey, string platformId, System.Action<bool, string> callback) where T : User, new()
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionOnlyLocalUser;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "login_platform");
			form.Add("PlatformKey", platformKey);
			form.Add("PlatformId", platformId);
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (success && result.ContainsKey("message") && result["message"] != null)
						{
							_authenticated = true;
							FromJson(result["message"].ToString());
							T user = new T();
							user.FromJson(result["message"].ToString());
							user._authenticated = true;
							SocialGamificationManager.platform.SetLocalUser(user);
						}
						else if (!success && result.ContainsKey("message"))
						{
							error = result["message"].ToString();
						}
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Links the currently logged account to another: all the platforms Ids of the current user
		/// will be transferred to the new account and the current account will be deleted.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="callback">Callback.</param>
		public virtual void LinkAccount(string username, string password, System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			if (!_id.Equals(SocialGamificationManager.localUser.id))
				throw ExceptionOnlyLocalUser;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "link_account");
			form.Add("Username", username);
			form.Add("Password", password);
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (success && result.ContainsKey("message") && result["message"] != null)
						{
							_authenticated = true;
							FromJson(result["message"].ToString());
							SocialGamificationManager.platform.SetLocalUser(this);
						}
						else if (!success && result.ContainsKey("message"))
						{
							error = result["message"].ToString();
						}
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Links a new platform Id to the logged account.
		/// </summary>
		/// <param name="platformKey">Platform key.</param>
		/// <param name="platformId">Platform identifier.</param>
		/// <param name="callback">Callback.</param>
		public virtual void LinkPlatform(string platformKey, string platformId, System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;
			if (!_id.Equals(SocialGamificationManager.localUser.id))
				throw ExceptionOnlyLocalUser;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "link_platform");
			form.Add("PlatformKey", platformKey);
			form.Add("PlatformId", platformId);
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (success && result.ContainsKey("message") && result["message"] != null)
						{
							_authenticated = true;
							FromJson(result["message"].ToString());
							SocialGamificationManager.platform.SetLocalUser(this);
						}
						else if (!success && result.ContainsKey("message"))
						{
							error = result["message"].ToString();
						}
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Adds the contact.
		/// </summary>
		/// <param name="otherUsername">Other username.</param>
		/// <param name="contactType">Contact type.</param>
		/// <param name="callback">Callback.</param>
		public void AddContact(string otherUsername, eContactType contactType, System.Action<bool, string> callback)
		{
			AddContact(string.Empty, otherUsername, contactType, callback);
		}

		/// <summary>
		/// Adds the contact.
		/// </summary>
		/// <param name="otherUser">Other user.</param>
		/// <param name="contactType">Contact type.</param>
		/// <param name="callback">Callback.</param>
		public void AddContact(Profile otherUser, eContactType contactType, System.Action<bool, string> callback)
		{
			AddContact(otherUser == null ? string.Empty : otherUser.id, string.Empty, contactType, callback);
		}

		/// <summary>
		/// Adds the contact.
		/// </summary>
		/// <param name="idUser">Identifier user.</param>
		/// <param name="username">Username.</param>
		/// <param name="contactType">Contact type.</param>
		/// <param name="callback">Callback.</param>
		private void AddContact(string idUser, string username, eContactType contactType, System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;

			// Use only on localUser
			if (SocialGamificationManager.localUser == null || !SocialGamificationManager.localUser.id.Equals(id))
				throw ExceptionOnlyLocalUser;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "save");
			if (!string.IsNullOrEmpty(idUser))
			{
				form.Add("Id", id);
			}
			else if (!string.IsNullOrEmpty(username))
			{
				form.Add("Username", username);
			}
			else
			{
				if (callback != null)
					callback(false, "No user");
				return;
			}
			form.Add("State", ((int)contactType).ToString());
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("contacts.php"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (!success && result.ContainsKey("message") && result["message"] != null)
						{
							error = result["message"].ToString();
						}
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Removes the contact.
		/// </summary>
		/// <param name="otherUsername">Other username.</param>
		/// <param name="callback">Callback.</param>
		public void RemoveContact(string otherUsername, System.Action<bool, string> callback)
		{
			RemoveContact(string.Empty, otherUsername, callback);
		}

		/// <summary>
		/// Removes the contact.
		/// </summary>
		/// <param name="otherUser">Other user.</param>
		/// <param name="callback">Callback.</param>
		public void RemoveContact(Profile otherUser, System.Action<bool, string> callback)
		{
			RemoveContact(otherUser == null ? string.Empty : otherUser.id, string.Empty, callback);
		}

		/// <summary>
		/// Removes the contact.
		/// </summary>
		/// <param name="idUser">Identifier user.</param>
		/// <param name="otherUsername">Other username.</param>
		/// <param name="callback">Callback.</param>
		private void RemoveContact(string idUser, string otherUsername, System.Action<bool, string> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw ExceptionSocialGamificationNotInitialized;

			// Use only on localUser
			if (SocialGamificationManager.localUser == null || !SocialGamificationManager.localUser.id.Equals(id))
				throw ExceptionOnlyLocalUser;
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "delete");
			if (!string.IsNullOrEmpty(idUser))
			{
				form.Add("Id", id);
			}
			else if (!string.IsNullOrEmpty(otherUsername))
			{
				form.Add("Username", otherUsername);
			}
			else
			{
				if (callback != null)
					callback(false, "No user");
				return;
			}
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("contacts.php"), form, (string text, string error) =>
			{
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (!success && result.ContainsKey("message") && result["message"] != null)
						{
							error = result["message"].ToString();
						}
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}
	}
}

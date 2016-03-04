using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class Role
    {
        public string id = "";
        public string name = "";
        public string description = "";
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;

        private static bool loadingRole = false;

        public Role()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Role"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public Role(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Role"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public Role(Hashtable data)
        {
            FromHashtable(data);
        }

        /// <summary>
		/// Initialize the object from a JSON formatted string.
		/// </summary>
		/// <param name="jsonString">Json string.</param>
		public virtual void FromJson(string jsonString)
        {
            FromHashtable(jsonString.hashtableFromJson());
        }

        /// <summary>
		/// Initialize the object from a hashtable.
		/// </summary>
		/// <param name="hash">Hash.</param>
		public virtual void FromHashtable(Hashtable hash)
        {
            if (hash == null)
            {
                return;
            }
            if (hash.ContainsKey("id") && hash["id"] != null)
            {
                id = hash["id"].ToString();
            }
            if (hash.ContainsKey("name") && hash["name"] != null)
            {
                name = hash["name"].ToString();
            }
            if (hash.ContainsKey("description") && hash["description"] != null)
            {
                description = hash["description"].ToString();
            }
            if (hash.ContainsKey("createdDate") && hash["createdDate"] != null && !string.IsNullOrEmpty(hash["createdDate"].ToString()))
            {
                DateTime myDate;
                if (DateTime.TryParseExact(hash["createdDate"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out myDate))
                {
                    createdTime = myDate;
                }
            }

            if (hash.ContainsKey("updatedDate") && hash["updatedDate"] != null && !string.IsNullOrEmpty(hash["updatedDate"].ToString()))
            {
                DateTime myDate;
                if (DateTime.TryParseExact(hash["updatedDate"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out myDate))
                {
                    updatedTime = myDate;
                }
            }
        }

        public static void GetRole(string idRole, Action<Role> callback)
        {
            if (loadingRole)
            {
                callback(null);
                return;
            }
            else
            {
                loadingRole = true;
            }

            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/roles/" + idRole), null, (string text, string error) =>
            {
                loadingRole = false;
                Role role = null;
                Hashtable result = text.hashtableFromJson();

                if (result != null)
                {
                    if (result.ContainsKey("id"))
                    {
                        role = new Role(result);
                    }
                    else
                    {
                        error = "API Response doesn't contact ID";
                    }
                }
                if (callback != null)
                    callback(role);
            });
        }
    }
}
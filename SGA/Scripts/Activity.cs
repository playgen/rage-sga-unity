using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class Activity
    {
        public string id = "";
        public string name = "";
        public string description = "";
        public string image = "";
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;
        private List<AttributeType> _attributeTypes = new List<AttributeType>();
        public List<AttributeType> attributeTypes { get { return _attributeTypes; } }
        private List<Goal> _goals = new List<Goal>();
        public List<Goal> goals { get { return _goals; } }
        private List<Role> _roles = new List<Role>();
        public List<Role> roles { get { return _roles; } }

        private static bool loadingActivity = false;

        public Activity()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Activity"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public Activity(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Activity"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public Activity(Hashtable data)
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
            if (hash.ContainsKey("image") && hash["image"] != null)
            {
                image = hash["image"].ToString();
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

        public static void GetActivity(string idActivity, Action<Activity> callback)
        {
            if (loadingActivity)
            {
                callback(null);
                return;
            }
            else
            {
                loadingActivity = true;
            }

            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/activities/" + idActivity), null, (string text, string error) =>
            {
                loadingActivity = false;
                Activity activity = null;
                Hashtable result = text.hashtableFromJson();

                if (result != null)
                {
                    if (result.ContainsKey("id"))
                    {
                        activity = new Activity(result);
                    }
                    else
                    {
                        error = "API Response doesn't contact ID";
                    }
                }
                if (callback != null)
                    callback(activity);
            });
        }
    }
}
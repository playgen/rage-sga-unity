using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class GoalFeedback
    {
        public string id = "";
        public float threshold = 0;
        public string direction = "";
        public string message = "";
        public string target = "";
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;

        public GoalFeedback()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.GoalFeedback"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public GoalFeedback(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.GoalFeedback"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public GoalFeedback(Hashtable data)
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
            if (hash.ContainsKey("threshold") && hash["threshold"] != null)
            {
                float.TryParse(hash["threshold"].ToString(), out threshold);
            }
            if (hash.ContainsKey("direction") && hash["direction"] != null)
            {
                direction = hash["direction"].ToString();
            }
            if (hash.ContainsKey("message") && hash["message"] != null)
            {
                message = hash["message"].ToString();
            }
            if (hash.ContainsKey("target") && hash["target"] != null)
            {
                target = hash["target"].ToString();
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
    }
}
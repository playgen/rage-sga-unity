using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class ActionRelation
    {
        public string id = "";
        public string relationship = "";
        public Vector2 concernChange = Vector2.zero;
        public Vector2 rewardResourceChange = Vector2.zero;
        public string actionId = "";
        public Action action;
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;
        private List<Reward> _rewards = new List<Reward>();
        public List<Reward> rewards { get { return _rewards; } }

        public ActionRelation()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.ActionRelation"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public ActionRelation(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.ActionRelation"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public ActionRelation(Hashtable data)
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
            if (hash.ContainsKey("relationship") && hash["relationship"] != null)
            {
                relationship = hash["relationship"].ToString();
            }
            if (hash.ContainsKey("concernChange") && hash["concernChange"] != null)
            {
                concernChange = GetCoordinates((Hashtable)hash["concernChange"]);
            }
            if (hash.ContainsKey("rewardResourceChange") && hash["rewardResourceChange"] != null)
            {
                rewardResourceChange = GetCoordinates((Hashtable)hash["rewardResourceChange"]);
            }
            if (hash.ContainsKey("actionId") && hash["actionId"] != null)
            {
                actionId = hash["actionId"].ToString();
            }
            if (hash.ContainsKey("action") && hash["action"] != null)
            {
                action = new Action((Hashtable)hash["action"]);
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

        private Vector2 GetCoordinates(Hashtable hash)
        {
            Vector2 returnV2 = Vector2.zero;
            if (hash == null)
            {
                return returnV2;
            }
            if (hash.ContainsKey("x"))
            {
                float.TryParse(hash["x"].ToString(), out returnV2.x);
            }
            if (hash.ContainsKey("y"))
            {
                float.TryParse(hash["y"].ToString(), out returnV2.y);
            }
            return returnV2;
        }
    }
}
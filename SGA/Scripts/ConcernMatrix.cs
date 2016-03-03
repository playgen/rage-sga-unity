using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class ConcernMatrix
    {
        public string id = "";
        public Vector2 coordinates;
        public string category = "";
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;
        public ConcernMatrix()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.ConcernMatrix"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public ConcernMatrix(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.ConcernMatrix"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public ConcernMatrix(Hashtable data)
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
            if (hash.ContainsKey("category") && hash["category"] != null)
            {
                category = hash["category"].ToString();
            }
            if (hash.ContainsKey("coordinates") && hash["coordinates"] != null)
            {
                coordinates = GetCoordinates((Hashtable)hash["coordinates"]);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void CalculateCategory(Action<bool, string> callback)
        {

        }
    }
}
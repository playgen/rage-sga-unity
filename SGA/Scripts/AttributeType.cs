using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class AttributeType
    {
        public string id = "";
        public float defaultValue = 0;
        public string name = "";
        public string type = "";
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;

        public AttributeType()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.AttributeType"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public AttributeType(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.AttributeType"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public AttributeType(Hashtable data)
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
            if (hash.ContainsKey("defaultValue") && hash["defaultValue"] != null)
            {
                float.TryParse(hash["defaultValue"].ToString(), out defaultValue);
            }
            if (hash.ContainsKey("name") && hash["name"] != null)
            {
                name = hash["name"].ToString();
            }
            if (hash.ContainsKey("type") && hash["type"] != null)
            {
                type = hash["type"].ToString();
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

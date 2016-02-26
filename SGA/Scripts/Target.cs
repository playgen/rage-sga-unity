using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class Target
    {
        public string id = "";
        public float value = 0;
        public string operation = "";
        public string status = "";
        public string attributeTypeId = "";
        public AttributeType attributeType;
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;

        public Target()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Target"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public Target(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Target"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public Target(Hashtable data)
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
            if (hash.ContainsKey("id"))
            {
                id = hash["id"].ToString();
            }
            if (hash.ContainsKey("value"))
            {
                float.TryParse(hash["value"].ToString(), out value);
            }
            if (hash.ContainsKey("operation"))
            {
                operation = hash["operation"].ToString();
            }
            if (hash.ContainsKey("status"))
            {
                status = hash["status"].ToString();
            }
            if (hash.ContainsKey("attributeTypeId") && hash["attributeTypeId"] != null)
            {
                attributeTypeId = hash["attributeTypeId"].ToString();
            }
            if (hash.ContainsKey("attributeType") && hash["attributeType"] != null)
            {
                attributeType = new AttributeType(hash["attributeType"].ToString());
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

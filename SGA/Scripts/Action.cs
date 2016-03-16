using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class Action
    {
        public string id = "";
        public string verb = "";
        public string idActivity = "";
        public Activity activity;
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;
        private List<ActionRelation> _actionRelations = new List<ActionRelation>();
        public List<ActionRelation> actionRelations { get { return _actionRelations; } }

        public Action()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Action"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public Action(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Action"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public Action(Hashtable data)
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

            if (hash.ContainsKey("verb") && hash["verb"] != null)
            {
                verb = hash["verb"].ToString();
            }

            if (hash.ContainsKey("activityId") && hash["activityId"] != null)
            {
                idActivity = hash["activityId"].ToString();
            }

            if (hash.ContainsKey("activity") && hash["activity"] != null)
            {
                activity = new Activity((Hashtable)hash["activity"]);
            }

            if (hash.ContainsKey("createdDate") && hash["createdDate"] != null && !string.IsNullOrEmpty(hash["createdDate"].ToString()))
            {
                DateTime myDate;
                if (DateTime.TryParseExact(hash["createdDate"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out myDate)) {
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

        /// <summary>
        /// Pushes a new Action
        /// </summary>
        /// <param name="callback">Callback.</param>
        public static Reward Push(string newVerb, Action<Reward> callback)
        {
            Reward reward = null;
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add("Verb", newVerb);
            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/actions"), form, (string text, string error) =>
            {

                if (string.IsNullOrEmpty(error))
                {
                    Hashtable result = text.hashtableFromJson();

                    if (result != null)
                    {
                        if (result.ContainsKey("id"))
                        {
                            reward = new Reward(result);
                        }
                        else
                        {
                            error = "API Response doesn't contact ID";
                        }
                    }
                }

                if (callback != null)
                {
                    callback(reward);
                }
            });
            return reward;
        }

        /// <summary>
        /// Registers an Action
        /// </summary>
        /// <param name="callback">Callback.</param>
        private void RegisterAction(Action<bool, string> callback)
        {
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add("id", id.ToString());
            ArrayList listRelations = new ArrayList();
            foreach (ActionRelation relation in _actionRelations)
            {
                if (string.IsNullOrEmpty(relation.id))
                    listRelations.Add(relation.id);
            }
            form.Add("ActionRelations", listRelations.toJson());

            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/actions/" + id), form, (string text, string error) =>
            {
                bool success = false;
                if (string.IsNullOrEmpty(error))
                {
                    Hashtable result = text.hashtableFromJson();
                    if (result != null)
                    {
                        if (result.ContainsKey("id") && result["id"] != null)
                        {
                            success = true;
                        }
                        else {
                            error = result["message"].ToString();
                        }
                    }
                }
                if (callback != null)
                    callback(success, error);
            }, "PUT");
        }
    }
}
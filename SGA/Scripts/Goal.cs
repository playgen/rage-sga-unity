using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class Goal
    {
        public string id = "";
        private List<Reward> _rewards = new List<Reward>();
        public List<Reward> rewards { get { return _rewards; } }
        private List<Target> _targets = new List<Target>();
        public List<Target> targets { get { return _targets; } }
        public string concernId;
        public ConcernMatrix concern;
        public string rewardResourceId;
        public RewardResourceMatrix rewardResource;
        private List<Activity> _activities = new List<Activity>();
        public List<Activity> activities { get { return _activities; } }
        private List<Action> _actions = new List<Action>();
        public List<Action> actions { get { return _actions; } }
        public string description = "";
        private List<Role> _roles = new List<Role>();
        public List<Role> roles { get { return _roles; } }
        public string feedbackId;
        public GoalFeedback feedback;
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;

        public Goal()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Goal"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public Goal(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Goal"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public Goal(Hashtable data)
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

            if (hash.ContainsKey("description") && hash["description"] != null)
            {
                description = hash["description"].ToString();
            }

            if (hash.ContainsKey("concernId") && hash["concernId"] != null)
            {
                concernId = hash["concernId"].ToString();
            }

            if (hash.ContainsKey("concern") && hash["concern"] != null)
            {
                concern = new ConcernMatrix(hash["concern"].ToString());
            }

            if (hash.ContainsKey("rewardResourceId") && hash["rewardResourceId"] != null)
            {
                rewardResourceId = hash["rewardResourceId"].ToString();
            }

            if (hash.ContainsKey("rewardResource") && hash["rewardResource"] != null)
            {
                rewardResource = new RewardResourceMatrix(hash["rewardResource"].ToString());
            }

           if (hash.ContainsKey("feedbackId") && hash["feedbackId"] != null)
           {
               feedbackId = hash["feedbackId"].ToString();
           }

           if (hash.ContainsKey("feedback") && hash["feedback"] != null)
           {
               feedback = new GoalFeedback(hash["feedback"].ToString());
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


        public static bool CheckTargetCompletion(List<Target> tars, Action<bool> callback)
        {
            foreach (Target t in tars)
            {
                if (t.status == "Completed")
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback">Callback.</param>
        private void RecommendGoal(Action<Goal> callback)
        {
           
        }
    }
}
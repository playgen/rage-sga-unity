using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
    [Serializable]
    public class ActorGoal
    {
        public string id = "";
        public string actorId;
        public Profile actor;
        public string goalId;
        public Goal goal;
        public string status = "";
        public string concernId;
        public ConcernMatrix concern;
        public string rewardResourceId;
        public RewardResourceMatrix rewardResource;
        public string activityId;
        public Activity activity;
        public string roleId;
        public Role role;
        public DateTime? updatedTime = null;
        public DateTime? createdTime = null;

        private static bool creatingGoal = false;
        private static bool loadingGoal = false;

        public ActorGoal()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Goal"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
        public ActorGoal(string jsonString)
        {
            FromJson(jsonString);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Goal"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
        public ActorGoal(Hashtable data)
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

            if (hash.ContainsKey("actorId") && hash["actorId"] != null)
            {
                actorId = hash["actorId"].ToString();
            }

            if (hash.ContainsKey("actor") && hash["actor"] != null)
            {
                actor = new Profile((Hashtable)hash["actor"]);
            }

            if (hash.ContainsKey("goalId") && hash["goalId"] != null)
            {
                goalId = hash["goalId"].ToString();
            }

            if (hash.ContainsKey("goal") && hash["goal"] != null)
            {
                goal = new Goal((Hashtable)hash["goal"]);
            }

            if (hash.ContainsKey("status") && hash["status"] != null)
            {
                status = hash["status"].ToString();
            }

            if (hash.ContainsKey("concernId") && hash["concernId"] != null)
            {
                concernId = hash["concernId"].ToString();
            }

            if (hash.ContainsKey("concern") && hash["concern"] != null)
            {
                concern = new ConcernMatrix((Hashtable)hash["concern"]);
            }

            if (hash.ContainsKey("rewardResourceId") && hash["rewardResourceId"] != null)
            {
                rewardResourceId = hash["rewardResourceId"].ToString();
            }

            if (hash.ContainsKey("rewardResource") && hash["rewardResource"] != null)
            {
                rewardResource = new RewardResourceMatrix((Hashtable)hash["rewardResource"]);
            }

            if (hash.ContainsKey("activityId") && hash["activityId"] != null)
            {
                activityId = hash["activityId"].ToString();
            }

            if (hash.ContainsKey("activity") && hash["activity"] != null)
            {
                activity = new Activity((Hashtable)hash["activity"]);
            }

            if (hash.ContainsKey("roleId") && hash["roleId"] != null)
            {
                roleId = hash["roleId"].ToString();
            }

            if (hash.ContainsKey("role") && hash["role"] != null)
            {
                role = new Role((Hashtable)hash["role"]);
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

        /// <summary>
		/// Get the goal that matches the id
		/// </summary>
        /// <param name="idGoal">The ID of the goal</param>
		/// <param name="callback">Callback.</param>
        public static void GetActorGoal(string idGoal, Action<ActorGoal> callback)
        {
            if (loadingGoal)
            {
                callback(null);
                return;
            }
            else
            {
                loadingGoal = true;
            }

            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/goals/" + idGoal + "/actor"), null, (string text, string error) =>
            {
                loadingGoal = false;
                ActorGoal goal = null;
                Hashtable result = text.hashtableFromJson();

                if (result != null)
                {
                    if (result.ContainsKey("id"))
                    {
                        goal = new ActorGoal(result);
                    }
                    else
                    {
                        error = "API Response doesn't contact ID";
                    }
                }
                else
                {

                }
                if (callback != null)
                    callback(goal);
            });
        }

        /// <summary>
		/// Create a new actorgoal
		/// </summary>
		/// <param name="callback">Callback.</param>
        public static void CreateActorGoal(string goalId, string concernId, string rewardResourceId, string activityId, string roleId, Action<ActorGoal> callback)
        {
            if (creatingGoal)
            {
                callback(null);
                return;
            }
            else
            {
                creatingGoal = true;
            }
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add("ActorId", SocialGamificationManager.localUser.id);
            form.Add("GoalId", goalId);
            form.Add("Status", "0");
            form.Add("ConcernOutcomeId", concernId);
            form.Add("RewardResourceOutcomeId", rewardResourceId);
            form.Add("ActivityId", activityId);
            form.Add("RoleId", roleId);
            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/goals/actors"), form, (string text, string error) =>
            {
                creatingGoal = false;
                ActorGoal goal = null;

                if (string.IsNullOrEmpty(error))
                {
                    Hashtable result = text.hashtableFromJson();

                    if (result != null)
                    {
                        if (result.ContainsKey("id"))
                        {
                            goal = new ActorGoal(result);
                        }
                    }
                }
                else
                {
                    Debug.Log(error);
                }

                if (callback != null)
                {
                    callback(goal);
                }
            });
        }
    }
}
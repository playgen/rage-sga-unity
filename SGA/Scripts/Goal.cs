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

        private static bool creatingGoal = false;
        private static bool loadingGoal = false;

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

           if (hash.ContainsKey("feedbackId") && hash["feedbackId"] != null)
           {
               feedbackId = hash["feedbackId"].ToString();
           }

           if (hash.ContainsKey("feedback") && hash["feedback"] != null)
           {
               feedback = new GoalFeedback((Hashtable)hash["feedback"]);
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

            if (hash.ContainsKey("rewards") && hash["rewards"] != null && hash["rewards"] is ArrayList)
            {
                _rewards.Clear();
                ArrayList listRewards = (ArrayList)hash["rewards"];
                if (listRewards != null)
                {
                    foreach (Hashtable dataReward in listRewards)
                    {
                        _rewards.Add(new Reward(dataReward));
                    }
                }
            }

            if (hash.ContainsKey("targets") && hash["targets"] != null && hash["targets"] is ArrayList)
            {
                _targets.Clear();
                ArrayList listTargets = (ArrayList)hash["targets"];
                if (listTargets != null)
                {
                    foreach (Hashtable dataTarget in listTargets)
                    {
                        _targets.Add(new Target(dataTarget));
                    }
                }
            }

            if (hash.ContainsKey("activities") && hash["activities"] != null && hash["activities"] is ArrayList)
            {
                _activities.Clear();
                ArrayList listActivities = (ArrayList)hash["activities"];
                if (listActivities != null)
                {
                    foreach (Hashtable dataActivity in listActivities)
                    {
                        _activities.Add(new Activity(dataActivity));
                    }
                }
            }

            if (hash.ContainsKey("actions") && hash["actions"] != null && hash["actions"] is ArrayList)
            {
                _actions.Clear();
                ArrayList listActions = (ArrayList)hash["actions"];
                if (listActions != null)
                {
                    foreach (Hashtable dataAction in listActions)
                    {
                        _actions.Add(new Action(dataAction));
                    }
                }
            }

            if (hash.ContainsKey("roles") && hash["roles"] != null && hash["roles"] is ArrayList)
            {
                _roles.Clear();
                ArrayList listRoles = (ArrayList)hash["roles"];
                if (listRoles != null)
                {
                    foreach (Hashtable dataRole in listRoles)
                    {
                        _roles.Add(new Role(dataRole));
                    }
                }
            }
        }

        /// <summary>
		/// Check if all targets for this goal are set as complete
		/// </summary>
		/// <param name="callback">Callback.</param>
        public bool CheckTargetCompletion(Action<bool> callback)
        {
            foreach (Target t in targets)
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
		/// Create a new goal
		/// </summary>
		/// <param name="callback">Callback.</param>
        public static void CreateGoal(string description, string concernId, string rewardResourceId, string feedbackId, Action<Goal> callback)
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
            form.Add("Description", description);
            form.Add("ConcernId", concernId);
            form.Add("RewardResourceId", rewardResourceId);
            form.Add("FeedbackId", feedbackId);
            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/goals"), form, (string text, string error) =>
            {
                creatingGoal = false;
                Goal goal = null;

                if (string.IsNullOrEmpty(error))
                {
                    Hashtable result = text.hashtableFromJson();

                    if (result != null)
                    {
                        if (result.ContainsKey("id"))
                        {
                            goal = new Goal(result);
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

        /// <summary>
		/// Get all goals for this activity
		/// </summary>
		/// <param name="callback">Callback.</param>
        public static void GetActivityGoals(string id, Action<List<Goal>> callback)
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

            Dictionary<string, string> form = new Dictionary<string, string>();
            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/goals/" + id + "/activity"), form, (string text, string error) =>
            {
                loadingGoal = false;
                List<Goal> listGoals = new List<Goal>();
                ArrayList list = text.arrayListFromJson();
                if (list != null)
                {
                    foreach (Hashtable data in list)
                    {
                        Goal goal = new Goal(data);

                        // Add to the list
                        listGoals.Add(goal);
                    }
                }
                if (callback != null)
                    callback(listGoals);
            });
        }

        /// <summary>
		/// Get the goal that matches the id
		/// </summary>
        /// <param name="idGoal">The ID of the goal</param>
		/// <param name="callback">Callback.</param>
        public static void GetGoal(string idGoal, Action<Goal> callback)
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

            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/goals/" + idGoal + "/detailed"), null, (string text, string error) =>
            {
                loadingGoal = false;
                Goal goal = null;
                Hashtable result = text.hashtableFromJson();

                if (result != null)
                {
                    if (result.ContainsKey("id"))
                    {
                        goal = new Goal(result);
                    }
                    else
                    {
                        error = "API Response doesn't contact ID";
                    }
                }
                if (callback != null)
                    callback(goal);
            });
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
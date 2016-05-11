using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
	[Serializable]
	public class MatchRound
	{
		public string id = "";
		public string idMatchActor = "";
		public float score = 0;
		public DateTime? dateScore = null;
		public int roundNumber = 0;

		public bool hasScore { get { return (dateScore.HasValue && dateScore != null); } }

		public MatchRound()
		{
		}

		public MatchRound(string jsonString)
		{
			FromJson(jsonString);
		}

		public MatchRound(Hashtable data)
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
				return;
			if (hash.ContainsKey("matchActorId") && hash["matchActorId"] != null)
			{
				idMatchActor = hash["matchActorId"].ToString();
			}
			if (hash.ContainsKey("score") && hash["score"] != null)
			{
				float.TryParse(hash["score"].ToString(), out score);
			}
			if (hash.ContainsKey("roundNumber") && hash["roundNumber"] != null)
			{
				int.TryParse(hash["roundNumber"].ToString(), out roundNumber);
			}
			if (hash.ContainsKey("dateScore") && hash["dateScore"] != null && !string.IsNullOrEmpty(hash["dateScore"].ToString()))
			{
				DateTime myDate;
				if (DateTime.TryParseExact(hash["dateScore"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out myDate))
					dateScore = myDate;
			}
		}
	}
}

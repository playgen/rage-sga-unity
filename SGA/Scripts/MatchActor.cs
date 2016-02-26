using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SocialGamification
{
	[Serializable]
	public class MatchActor
	{
		public string id = "";
		public string idMatch = "";
		public string idAccount = "";
		public Hashtable customData = new Hashtable();
		public float score = 0;
		public DateTime? dateScore = null;
		public Profile user = null;

		private List<MatchRound> _rounds = new List<MatchRound>();

		public List<MatchRound> rounds { get { return _rounds; } }

		public MatchActor()
		{
		}

		public MatchActor(string jsonString)
		{
			FromJson(jsonString);
		}

		public MatchActor(Hashtable data)
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

			if (hash.ContainsKey("matchId") && hash["matchId"] != null)
			{
				idMatch = hash["matchId"].ToString();
			}

			if (hash.ContainsKey("accountId") && hash["accountId"] != null)
			{
				idAccount = hash["accountId"].ToString();
			}

			if (hash.ContainsKey("customData") && hash["customData"] != null && hash["customData"] is Hashtable)
			{
				customData = (Hashtable)hash["customData"];
			}

			if (hash.ContainsKey("actor") && hash["actor"] != null)
			{
				user = new Profile((Hashtable)hash["actor"]);
			}

			if (hash.ContainsKey("rounds") && hash["rounds"] != null)
			{
				_rounds.Clear();
				ArrayList listRounds = (ArrayList)hash["rounds"];
				foreach (Hashtable data in listRounds)
				{
					_rounds.Add(new MatchRound(data));
				}
			}
		}
	}
}

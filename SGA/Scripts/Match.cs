using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace SocialGamification
{
	[Serializable]
	public class Match
	{
		[Serializable]
		public class MatchRoundData
		{
			public List<MatchActor> users = new List<MatchActor>();
			public List<MatchRound> scores = new List<MatchRound>();
		}

		public string id = "";
		public string idTournament = "";
		public string title = "";
		public int roundsCount = 1;
		public int currentRound = 0;
		public DateTime dateCreation = DateTime.Now;
		public DateTime? dateExpire = null;
		public Hashtable customData = new Hashtable();

		private List<string> _deletedUsers = new List<string>();
		private List<MatchActor> _users = new List<MatchActor>();

		public List<MatchActor> users { get { return _users; } }

		private List<MatchRoundData> _rounds = new List<MatchRoundData>();

		public List<MatchRoundData> rounds { get { return _rounds; } }

		private bool _finished = false;

		public bool finished { get { return _finished; } }

		private bool _quickMatch = false;

		public bool searchingQuickMatch { get { return _quickMatch; } }

        private bool matchDuplicate = false;
        private bool matchFromHashtable = false;
        private bool matchGetCurrentRound = false;
        private bool matchEnd = false;
        private bool matchScore = false;
        private bool matchGetScore = false;
        private bool matchSave = false;
        private static bool matchDelete = false;
        private static bool matchQuickMatch = false;
        private static bool matchLoadTournament = false;
        private static bool matchLoadMatch = false;

		public Match()
		{
		}

        public Match(Match clone)
        {
            this.id = clone.id;
            this.idTournament = clone.idTournament;
            this.title = clone.title;
            this.roundsCount = clone.roundsCount;
            this.currentRound = clone.currentRound;
            this.dateCreation = clone.dateCreation;
            this.dateExpire = clone.dateExpire;
            this._deletedUsers = new List<string>(clone._deletedUsers);
            this._users = new List<MatchActor>(clone._users);
            this._rounds = new List<MatchRoundData>(clone._rounds);
            this._finished = clone._finished;
            this._quickMatch = clone._quickMatch;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Match"/> class.
		/// </summary>
		/// <param name="jsonString">JSON string to initialize the instance.</param>
		public Match(string jsonString)
		{
			FromJson(jsonString);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SocialGamification.Match"/> class.
		/// </summary>
		/// <param name="data">Data to initialize the instance.</param>
		public Match(Hashtable data)
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
        /// Makes a copy of the current match
        /// </summary>
        public void Duplicate(Action<Match, bool, string> callback)
        {
            if (matchDuplicate)
            {
                callback(null,false, "already duplicating");
                return;
            }
            else
            {
                matchDuplicate = true;
            }
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add("actors", _users.Select(user => user.idAccount).ToList().toArrayString());

            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches/actors"), form, (string text, string error) =>
            {
                matchDuplicate = false;
                Hashtable hash = text.hashtableFromJson();
                if (hash != null)
                {
                    Match match = new Match(hash);
                    callback(match, true, error);
                }
                else
                {
                    callback(null, false, error);
                }

            });
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

            if (matchFromHashtable)
            {
                Debug.Log("Already calling function FromHashTable");
                return;
            }
            else
            {
                matchFromHashtable = true;
            }

            if (hash.ContainsKey("id") && hash["id"] != null)
			{
				id = hash["id"].ToString();
			}

			if (hash.ContainsKey("tournamentId") && hash["tournamentId"] != null)
			{
				idTournament = hash["tournamentId"].ToString();
			}

			if (hash.ContainsKey("title") && hash["title"] != null)
			{
				title = hash["title"].ToString();
			}

			if (hash.ContainsKey("totalRounds") && hash["totalRounds"] != null)
			{
				int.TryParse(hash["totalRounds"].ToString(), out roundsCount);
			}

			if (hash.ContainsKey("dateCreation") && hash["dateCreation"] != null && !string.IsNullOrEmpty(hash["dateCreation"].ToString()))
			{
				DateTime.TryParseExact(hash["dateCreation"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateCreation);
			}

			if (hash.ContainsKey("dateExpire") && hash["dateExpire"] != null && !string.IsNullOrEmpty(hash["dateExpire"].ToString()))
			{
				DateTime myDate;
				if (DateTime.TryParseExact(hash["dateExpire"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out myDate))
				{
					dateExpire = myDate;
				}
			}

			if (hash.ContainsKey("isFinished") && hash["isFinished"] != null)
			{
				_finished = (bool)hash["isFinished"];
			}

			if (hash.ContainsKey("customData") && hash["customData"] != null && hash["customData"] is Hashtable)
			{
				customData = (Hashtable)hash["customData"];
			}

            if (hash.ContainsKey("actors") && hash["actors"] != null && hash["actors"] is ArrayList)
            {
                _users.Clear();
                ArrayList listActors = (ArrayList)hash["actors"];
                if (listActors != null)
                {
                    foreach (Hashtable dataActor in listActors)
                    {
                        _users.Add(new MatchActor(dataActor));
                    }
                }
            }

            if (hash.ContainsKey("rounds") && hash["rounds"] != null && hash["rounds"] is ArrayList)
            {
                _rounds.Clear();
                ArrayList listRounds = (ArrayList)hash["rounds"];
                if (listRounds != null)
                {
                    List<MatchRound> _matchRounds = new List<MatchRound>();
                    foreach (Hashtable dataRound in listRounds)
                    {
                        _matchRounds.Add(new MatchRound(dataRound));
                    }
                    MatchActor matchActor = _users.Find(u => u.idAccount.Equals(SocialGamificationManager.localUser.id));
                    foreach (MatchRound mr in _matchRounds)
                    {
                        if (mr.idMatchActor == matchActor.id)
                        {
                            currentRound = mr.roundNumber;
                        }
                    }
                    MatchRoundData mrd = new MatchRoundData();
                    mrd.users = _users;
                    mrd.scores = _matchRounds;
                    _rounds.Add(mrd);
                }
            }

            

            // Get list of Actors for this match
            /*SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches/" + id + "/actors"), null, (string text, string error) =>
            {
                matchFromHashtable = false;
                if (string.IsNullOrEmpty(error))
                {
                    _users.Clear();
                    _deletedUsers.Clear();

                    JSONObject j = new JSONObject(text);
                    foreach (JSONObject matchActor in j.list)
                    {
                        MatchActor actor = new MatchActor();
                        actor.id = matchActor["id"].str;
                        actor.idAccount = matchActor["actorId"].str;
                        actor.idMatch = matchActor["matchId"].str;

                        _users.Add(actor);
                    }

                    //FillRounds();
                    GetCurrentRound();
                }
                Debug.Log(_users.Count);
            });*/
        }

        /// <summary>
        /// Fills the rounds from Match Accounts.
        /// </summary>
        private void FillRounds()
		{
			_rounds.Clear();
			for (int i = 0; i < roundsCount; ++i)
			{
				_rounds.Add(new MatchRoundData());
			}

			// Fill the rounds for each user
			foreach (MatchActor user in _users)
			{
				for (int i = 0; i < roundsCount; ++i)
				{
					_rounds[i].users.Add(user);

					MatchRound round;
					if (i < user.rounds.Count)
					{
						round = user.rounds[i];
					}
					else
					{
						round = new MatchRound();
						round.idMatchActor = user.id;
					}
					_rounds[i].scores.Add(round);
				}
			}
		}

		/// <summary>
		/// Get the current Round for this actor. Goes through all the actors and rounds. Set's the current round where the dateScore is null or latest round if all scores were set.
		/// </summary>
		private void GetCurrentRound()
		{
            if (matchGetCurrentRound)
            {
                Debug.Log("Already retriving current round");
                return;
            }
            else
            {
                matchGetCurrentRound = true;
            }
			string myId = SocialGamificationManager.localUser.id;

			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches/" + id + "/rounds"), null, (string text, string error) =>
			{
                matchGetCurrentRound = false;
				//				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					ArrayList list = text.arrayListFromJson();
					if (list != null)
					{
						foreach (Hashtable hash in list)
						{
							if (hash.ContainsKey("roundNumber") && hash["roundNumber"] != null)
							{
								int.TryParse(hash["roundNumber"].ToString(), out currentRound);
								if (hash.ContainsKey("actors") && hash["actors"] != null)
								{
									foreach (Hashtable actorHash in (ArrayList)hash["actors"])
									{
										if (actorHash.ContainsKey("actorId") && actorHash["actorId"] != null && actorHash["actorId"].ToString() == myId
											&& actorHash.ContainsKey("dateScore") && actorHash["dateScore"] == null)
										{
											return;
										}
									}
								}
							}
						}
					}
				}
			});
		}

		/// <summary>
		/// Adds the user to this match.
		/// </summary>
		/// <param name="user">User.</param>
		public virtual void AddUser(Profile user)
		{
			MatchActor matchActor = new MatchActor();
			matchActor.idMatch = id;
			matchActor.user = user;
			matchActor.idAccount = matchActor.user.id;
			_users.Add(matchActor);
		}

		/// <summary>
		/// Removes the user.
		/// </summary>
		/// <param name="user">User.</param>
		public virtual void RemoveUser(Profile user)
		{
			RemoveUserId(user.id);
		}

		/// <summary>
		/// Removes the user.
		/// </summary>
		/// <param name="idUser">Identifier user.</param>
		public virtual void RemoveUserId(string idUser)
		{
			RemoveUser(idUser, string.Empty);
		}

		/// <summary>
		/// Removes the user.
		/// </summary>
		/// <param name="username">Username.</param>
		public virtual void RemoveUserName(string username)
		{
			RemoveUser(string.Empty, username);
		}

		/// <summary>
		/// Removes the user.
		/// </summary>
		/// <param name="idUser">Identifier user.</param>
		/// <param name="username">Username.</param>
		protected virtual void RemoveUser(string idUser, string username)
		{
			if (string.IsNullOrEmpty(idUser) && string.IsNullOrEmpty(username))
				return;
			int i;
			if (!string.IsNullOrEmpty(idUser))
				i = _users.FindIndex(u => u.idAccount.Equals(idUser));
			else
				i = _users.FindIndex(u => u.user != null && u.user.userName.Equals(username));
			if (i != -1)
			{
				if (!string.IsNullOrEmpty(_users[i].id))
					_deletedUsers.Add(_users[i].id);
				_users.RemoveAt(i);
			}
		}

		/// <summary>
		/// Finish the current Match when a player quits
		/// </summary>
		public void Quit()
		{
            if (matchEnd)
            {
                Debug.Log("Already Ending Match");
                return;
            }
            else
            {
                matchEnd = true;
            }
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("Id", id);
			form.Add("TournamentId", idTournament);
			form.Add("IsFinished", "true");
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches/" + id), form, (string text, string error) =>
			{
                matchEnd = false;
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null && result.ContainsKey("id"))
					{
						success = true;
					}
				}
				_finished = success;
			}, "PUT");
		}

        /// <summary>
		/// End the current Match
		/// </summary>
		public void End()
        {
            if (matchEnd)
            {
                Debug.Log("Already Ending Match");
                return;
            }
            else
            {
                matchEnd = true;
            }
            Dictionary<string, string> form = new Dictionary<string, string>();
            form.Add("Id", id);
            form.Add("TournamentId", idTournament);
            form.Add("IsFinished", "true");
            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches/" + id), form, (string text, string error) =>
            {
                matchEnd = false;
                bool success = false;
                if (string.IsNullOrEmpty(error))
                {
                    Hashtable result = text.hashtableFromJson();
                    if (result != null && result.ContainsKey("id"))
                    {
                        success = true;
                    }
                }
                _finished = success;
            }, "PUT");
        }

        /// <summary>
        /// Send the specified score.
        /// </summary>
        /// <param name="score">Score.</param>
        /// <param name="callback">Callback.</param>
        public void Score(float score, Action<bool, string> callback)
		{
            if (matchScore)
            {
                if (callback != null)
                {
                    callback(false, "Already updating score");
                }
                return;
            }
            else
            {
                matchScore = true;
            }

			if (_finished)
			{
                if (callback != null)
                {
                    callback(false, "This Match is already finished");
                }
				return;
			}

			if (_users.Count < 1)
			{
                if (callback != null)
                {
                    callback(false, "No Users found for this Match");
                }
                return;
			}

			string myId = SocialGamificationManager.localUser.id;
			MatchActor matchActor = _users.Find(u => u.idAccount.Equals(myId));

			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("ActorId", matchActor.idAccount);
			form.Add("Score", score.ToString());
			form.Add("RoundNumber", currentRound.ToString());
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches/" + id + "/rounds"), form, (string text, string error) =>
			  {
                  matchScore = false;
				  //Debug.Log("Score Text: " + text);
				  bool success = false;
				  if (string.IsNullOrEmpty(error))
				  {
					  Hashtable result = text.hashtableFromJson();
					  if (result != null)
					  {
						  if (result.ContainsKey("actorId") && result["actorId"] != null)
						  {
							  success = true;

							  //						  	matchActor.FromJson(result["message"].ToString());
							  //						  	FillRounds();
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

		/// <summary>
		/// Gets the score.
		/// </summary>
		/// <param name="score">Score.</param>
		/// <param name="callback">Callback.</param>
		public void GetScore(Action<bool, float, float, string> callback)
		{
            if (matchGetScore)
            {
                callback(false, 0, 0, "Already retrieving score");
            }
            else
            {
                matchGetScore = true;
            }

			if (_users.Count < 1)
			{
				callback(false, 0, 0, "No Users found for this Match");
			}

			string myId = SocialGamificationManager.localUser.id;

			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches/" + id + "/rounds"), null, (string text, string error) =>
			{
                matchGetScore = false;
				bool success = false;
				float opponentScore = 0;
				float myScore = 0;
				if (string.IsNullOrEmpty(error))
				{
					ArrayList list = text.arrayListFromJson();
					if (list != null)
					{
						foreach (Hashtable hash in list)
						{
							if (hash.ContainsKey("roundNumber") && hash["roundNumber"] != null)
							{
								int.TryParse(hash["roundNumber"].ToString(), out currentRound);
								if (hash.ContainsKey("actors") && hash["actors"] != null)
								{
									Debug.Log("HashContains actors");
									foreach (Hashtable actorHash in (ArrayList)hash["actors"])
									{
										Debug.Log("foreach actor: " + actorHash);
										if (actorHash.ContainsKey("actorId") && actorHash["actorId"] != null
											&& actorHash.ContainsKey("dateScore") && actorHash["dateScore"] != null)
										{
											Debug.Log(actorHash["actorId"]);
											if (actorHash["actorId"].ToString() == myId)
											{
												Debug.Log("MyScore : " + actorHash["score"]);
												float.TryParse(actorHash["score"].ToString(), out myScore);
											}
											else {
												Debug.Log("OpponentScore : " + actorHash["score"]);
												float.TryParse(actorHash["score"].ToString(), out opponentScore);
												success = true;
											}
										}
									}
								}
							}
						}
					}
				}
				if (callback != null)
					callback(success, opponentScore, myScore, error);
			});
		}

		/// <summary>
		/// Save the this instance in the server.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void Save(Action<bool, string> callback)
		{
            if (matchSave)
            {
                if (callback != null)
                {
                    callback(false, "Save function already being called");
                }
                return;
            }
            else
            {
                matchSave = true;
            }

			if (_users.Count < 2)
			{
                if (callback != null)
                {
                    callback(false, "A match requires at least 2 users");
                }
				return;
			}

			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "match_save");
			form.Add("Id", id.ToString());
			form.Add("IdTournament", idTournament.ToString());
			form.Add("Title", title);
			form.Add("Rounds", roundsCount.ToString());
			if (dateExpire.HasValue && dateExpire != null)
			{
				form.Add("DateExpire", ((System.DateTime)dateExpire).ToString("yyyy-MM-dd HH:mm:ss"));
			}
			form.Add("CustomData", customData.toJson());

			// Add new users to the request
			ArrayList listUsers = new ArrayList();
			foreach (MatchActor user in _users)
			{
				if (string.IsNullOrEmpty(user.id))
					listUsers.Add(user.idAccount);
			}
			form.Add("Users", listUsers.toJson());

			// Add the deleted users to the request
			listUsers.Clear();
			foreach (string userId in _deletedUsers)
			{
				listUsers.Add(userId);
			}
			form.Add("DeleteUsers", listUsers.toJson());

			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("match.php"), form, (string text, string error) =>
			{
                matchSave = false;
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						bool.TryParse(result["success"].ToString(), out success);
						if (success)
							FromJson(result["message"].ToString());
						else
							error = result["message"].ToString();
					}
				}

				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Delete this instance from the database.
		/// </summary>
		public virtual void Delete(Action<bool, string> callback)
		{
			Delete(id, callback);
		}

		/// <summary>
		/// Delete the specified Match.
		/// </summary>
		/// <param name="idMatch">Identifier match.</param>
		/// <param name="callback">Callback.</param>
		public static void Delete(string idMatch, Action<bool, string> callback)
		{
            if (matchDelete)
            {
                if (callback != null)
                {
                    callback(false, "Delete already being called");
                    return;
                }
            }
            else
            {
                matchDelete = true;
            }

			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "match_delete");
			form.Add("Id", idMatch.ToString());
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("match.php"), form, (string text, string error) =>
			{
                matchDelete = false;
				bool success = false;
				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();
					if (result != null)
					{
						if (result.ContainsKey("success"))
							bool.TryParse(result["success"].ToString(), out success);
						if (!success && result.ContainsKey("message"))
							error = result["message"].ToString();
					}
				}
				if (callback != null)
					callback(success, error);
			});
		}

		/// <summary>
		/// Creates a quick match.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public static void QuickMatch(bool friendsOnly, SearchCustomData[] customData, int rounds, Action<Match> callback)
		{
            if (matchQuickMatch)
            {
                //Debug.Log("QuickMatch function already being called");
                callback(null);
                return;
            }
            else
            {
                matchQuickMatch = true;
            }

			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("Friends", friendsOnly ? "1" : "0");

			// Build the JSON string for CustomData filtering
			string searchCustomData = "";
			if (customData != null)
			{
				ArrayList list = new ArrayList();
				foreach (SearchCustomData data in customData)
				{
					if (string.IsNullOrEmpty(data.key))
					{
						continue;
					}

					Hashtable search = new Hashtable();
					search.Add("key", data.key);
					search.Add("value", data.value);
					switch (data.op)
					{
						case eSearchOperator.Equals:
							search.Add("operator", "=");
							break;

						case eSearchOperator.Disequals:
							search.Add("operator", "!");
							break;

						case eSearchOperator.Like:
							search.Add("operator", "%");
							break;

						case eSearchOperator.Greater:
							search.Add("operator", ">");
							break;

						case eSearchOperator.GreaterOrEquals:
							search.Add("operator", ">=");
							break;

						case eSearchOperator.Lower:
							search.Add("operator", "<");
							break;

						case eSearchOperator.LowerOrEquals:
							search.Add("operator", "<=");
							break;
					}

					list.Add(search);
				}

				if (list.Count > 0)
				{
					searchCustomData = list.toJson();
				}
			}

			form.Add("CustomData", searchCustomData);

			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches"), form, (string text, string error) =>
			{
                matchQuickMatch = false;
				Match match = null;

				if (string.IsNullOrEmpty(error))
				{
					Hashtable result = text.hashtableFromJson();

					if (result != null)
					{
						if (result.ContainsKey("id"))
						{
							match = new Match(result);
						}
						else
						{
							error = "API Response doesn't contact ID";
						}
					}
				}

				if (callback != null)
				{
					callback(match);
				}
			});
		}

        /// <summary>
		/// Load the list of ongoing Matches by specified filters.
		/// </summary>
		/// <param name="idTournament">Identifier tournament.</param>
		/// <param name="activeOnly">If set to <c>true</c> then displays active matches only, else archived matches.</param>
		/// <param name="title">Title.</param>
		/// <param name="callback">Callback.</param>
		public static void LoadOngoing(string idTournament, bool activeOnly, string title, Action<Match[]> callback)
        {
            if (matchLoadTournament)
            {

                callback(null);
                return;
            }
            else
            {
                matchLoadTournament = true;
            }

            Dictionary<string, string> form = new Dictionary<string, string>();

            //form.Add("action", "match_list");
            //if (!activeOnly)
            //	form.Add("Active", "0");
            if (idTournament != "0")
                form.Add("IdTournament", idTournament);
            if (!string.IsNullOrEmpty(title))
                form.Add("Title", title);
            SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches/ongoing"), form, (string text, string error) =>
            {
                matchLoadTournament = false;
                List<Match> listMatches = new List<Match>();
                ArrayList list = text.arrayListFromJson();
                if (list != null)
                {
                    foreach (Hashtable data in list)
                    {
                        // Create a new object from the result
                        Match match = new Match(data);

                        // Add to the list
                        listMatches.Add(match);
                    }
                }
                if (callback != null)
                    callback(listMatches.ToArray());
            });
        }

        /// <summary>
        /// Load the list of Matchs by specified filters.
        /// </summary>
        /// <param name="idTournament">Identifier tournament.</param>
        /// <param name="activeOnly">If set to <c>true</c> then displays active matches only, else archived matches.</param>
        /// <param name="title">Title.</param>
        /// <param name="callback">Callback.</param>
        public static void Load(string idTournament, bool activeOnly, string title, Action<Match[]> callback)
		{
            if (matchLoadTournament)
            {
               
                callback(null);
                return;
            }
            else
            {
                matchLoadTournament = true;
            }

			Dictionary<string, string> form = new Dictionary<string, string>();

			//form.Add("action", "match_list");
			//if (!activeOnly)
			//	form.Add("Active", "0");
			if (idTournament != "0")
				form.Add("IdTournament", idTournament);
			if (!string.IsNullOrEmpty(title))
				form.Add("Title", title);
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches"), form, (string text, string error) =>
			{
                matchLoadTournament = false;
				List<Match> listMatches = new List<Match>();
				ArrayList list = text.arrayListFromJson();
				if (list != null)
				{
					foreach (Hashtable data in list)
					{
						// Create a new object from the result
						Match match = new Match(data);

						// Add to the list
						listMatches.Add(match);
					}
				}
				if (callback != null)
					callback(listMatches.ToArray());
			});
		}

        /// <summary>
        /// Load the specified Match. Or any created match with myself if ID == 0
        /// </summary>
        /// <param name="idMatch">Identifier match.</param>
        /// <param name="callback">Callback.</param>
        public static void Load(string idMatch, Action<Match> callback)
		{
            if (matchLoadMatch)
            {
                //Debug.Log("ALready Loading match");
                callback(null);
                return;
            }
            else
            {
                matchLoadMatch = true;
            }

			Dictionary<string, string> form = new Dictionary<string, string>();
			if (idMatch != "0")
				form.Add("Id", idMatch);
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/matches"), form, (string text, string error) =>
            {
                matchLoadMatch = false;
                Match match = null;
				Hashtable result = text.hashtableFromJson();

				if (result != null)
				{
					if (result.ContainsKey("id"))
					{
						match = new Match(result);
					}
					else
					{
						error = "API Response doesn't contact ID";
					}
				}
				if (callback != null)
					callback(match);
			});
		}
	}
}

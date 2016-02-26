using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace SocialGamification
{
	/// <summary>
	/// SocialGamification Platform implementation of Unity built-in Social interfaces (ISocialPlatform).
	/// </summary>
	public class SocialGamificationPlatform : ISocialPlatform
	{
		private User _localUser = new User();

		public ILocalUser localUser { get { return _localUser; } }

		#region ISocialPlatform implementation

		/// <summary>
		/// Authenticates the user.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="callback">Callback.</param>
		public virtual void Authenticate(ILocalUser user, System.Action<bool> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw new System.Exception("SocialGamification Manager not initialized");
			user.Authenticate((bool success) =>
			{
				if (success)
					_localUser = (User)user;
				if (callback != null)
					callback(success);
			});
		}

		/// <summary>
		/// Authenticates the user with specified username and password.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="callback">Callback.</param>
		public virtual void Authenticate(string username, string password, System.Action<bool, string> callback)
		{
			Authenticate<User>(username, password, callback);
		}

		/// <summary>
		/// Authenticates the user with specified username and password using the specified profile class.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="T">Type of the returned profiles.</typeparam>
		public virtual void Authenticate<T>(string username, string password, System.Action<bool, string> callback) where T : User, new()
		{
			if (!SocialGamificationManager.isInitialized)
				throw new System.Exception("SocialGamification Manager not initialized");
			_localUser = new T();
			_localUser.userName = username;
			_localUser.Authenticate(password, callback);
		}

		/// <summary>
		/// Loads the users by Id.
		/// </summary>
		/// <param name="userIDs">User I ds.</param>
		/// <param name="callback">Callback.</param>
		public virtual void LoadUsers(string[] userIDs, System.Action<IUserProfile[]> callback)
		{
			User.Load(userIDs, (User[] profiles) =>
			{
				if (callback != null)
					callback((IUserProfile[])profiles);
			});
		}

		/// <summary>
		/// Reports the progress of an Achievement expressed as percentage. The progress will be multiplied by 100.0 and finally rounded to int.
		/// </summary>
		/// <param name="achievementID">Achievement identifier.</param>
		/// <param name="progress">Progress.</param>
		/// <param name="callback">Callback.</param>
		public virtual void ReportProgress(string achievementId, double progress, System.Action<bool> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Reports the progress of an Achievement.
		/// </summary>
		/// <param name="achievementId">Achievement identifier.</param>
		/// <param name="progress">Progress.</param>
		/// <param name="callback">Callback.</param>
		public virtual void ReportProgress(string achievementId, int progress, System.Action<bool> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Loads the achievement descriptions.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void LoadAchievementDescriptions(System.Action<IAchievementDescription[]> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Loads the achievements.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void LoadAchievements(System.Action<IAchievement[]> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Loads the achievements.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual void LoadAchievements<T>(System.Action<T[]> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates the achievement.
		/// </summary>
		/// <returns>The achievement.</returns>
		public virtual IAchievement CreateAchievement()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Reports the score of a Leaderboard.
		/// </summary>
		/// <param name="score">Score.</param>
		/// <param name="board">Board.</param>
		/// <param name="callback">Callback.</param>
		public virtual void ReportScore(long score, string board, System.Action<bool> callback)
		{
			ReportScore(score.ToString(), board, string.Empty, callback);
		}

		/// <summary>
		/// Reports the score of a Leaderboard.
		/// </summary>
		/// <param name="score">Score.</param>
		/// <param name="board">Board.</param>
		/// <param name="callback">Callback.</param>
		public virtual void ReportScore(string score, string board, System.Action<bool> callback)
		{
			ReportScore(score, board, string.Empty, callback);
		}

		/// <summary>
		/// Reports the score of a Leaderboard.
		/// </summary>
		/// <param name="score">Score.</param>
		/// <param name="board">Board.</param>
		/// <param name="username">Username.</param>
		/// <param name="callback">Callback.</param>
		public virtual void ReportScore(string score, string board, string username, System.Action<bool> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Loads the scores of a Leaderboard.
		/// </summary>
		/// <param name="leaderboardID">Leaderboard I.</param>
		/// <param name="callback">Callback.</param>
		public virtual void LoadScores(string leaderboardID, System.Action<IScore[]> callback)
		{
			LoadScores(leaderboardID, 1, 10, callback);
		}

		/// <summary>
		/// Loads the scores of a Leaderboard.
		/// </summary>
		/// <param name="leaderboardID">Leaderboard I.</param>
		/// <param name="page">Page.</param>
		/// <param name="countPerPage">Count per page.</param>
		/// <param name="callback">Callback.</param>
		public virtual void LoadScores(string leaderboardID, int page, int countPerPage, System.Action<IScore[]> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates the leaderboard.
		/// </summary>
		/// <returns>The leaderboard.</returns>
		public virtual ILeaderboard CreateLeaderboard()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Shows the achievements UI. Requires achievementUIObject and eventually achievementUIFunction set in order to work.
		/// </summary>
		public virtual void ShowAchievementsUI()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Shows the leaderboard UI. Requires leaderboardUIObject and eventually leaderboardUIFunction set in order to work.
		/// </summary>
		public virtual void ShowLeaderboardUI()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Loads the friends of localUser.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="callback">Callback.</param>
		public virtual void LoadFriends(ILocalUser user, System.Action<bool> callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw new System.Exception("SocialGamification Manager not initialized");
			user.LoadFriends(callback);
		}

		/// <summary>
		/// Loads the scores of a Leaderboard.
		/// </summary>
		/// <param name="board">Board.</param>
		/// <param name="callback">Callback.</param>
		public virtual void LoadScores(ILeaderboard board, System.Action<bool> callback)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the loading state of a Leaderboard.
		/// </summary>
		/// <returns><c>true</c>, if loading was gotten, <c>false</c> otherwise.</returns>
		/// <param name="board">Board.</param>
		public virtual bool GetLoading(ILeaderboard board)
		{
			throw new NotImplementedException();
		}

		#endregion ISocialPlatform implementation

		/// <summary>
		/// Sets the local user. For internal use only (e.g. User.Authenticate), it's not recommended to call this method directly.
		/// </summary>
		/// <param name="user">User.</param>
		public virtual void SetLocalUser(User user)
		{
			_localUser = user;
		}

		/// <summary>
		/// Logout localUser.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public virtual void Logout(System.Action callback)
		{
			if (!SocialGamificationManager.isInitialized)
				throw new System.Exception("SocialGamification Manager not initialized");
			_localUser = new User();
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("action", "logout");
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("users.php"), form, (string text, string error) =>
			{
				if (callback != null)
					callback();
			});
		}

		/// <summary>
		/// Loads the scores of a Leaderboard by user.
		/// </summary>
		/// <param name="leaderboardId">Leaderboard identifier.</param>
		/// <param name="user">User.</param>
		/// <param name="interval">Interval.</param>
		/// <param name="callback">Callback.</param>
		//public virtual void LoadScoresByUser(string leaderboardId, User user, eLeaderboardInterval interval, int limit, System.Action<Score, int, string> callback)
		//{
		//}
	}
}

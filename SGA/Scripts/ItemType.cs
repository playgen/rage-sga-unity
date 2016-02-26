using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace SocialGamification
{
	[System.Serializable]
	public class ItemType {
		public string id = "";
		public string name = "";
		public string image = "";
		public DateTime? updateDate = null;
		public DateTime? createDate = null;
		
		public ItemType()
		{
		}
		
		public ItemType(string jsonString)
		{
			FromJson(jsonString);
		}
		
		public ItemType(Hashtable data)
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
			
			if (hash.ContainsKey("name") && hash["name"] != null)
			{
				name = hash["name"].ToString();
			}
			
			if (hash.ContainsKey("image") && hash["image"] != null)
			{
				image = hash["image"].ToString();
			}
			
			if (hash.ContainsKey ("updatedDate") && hash ["updatedDate"] != null) {
				DateTime myDate;
				if (DateTime.TryParseExact (hash ["updatedDate"].ToString (), "yyyy-MM-ddTHH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out myDate)) {
					updateDate = myDate;
				}else{
					Debug.Log("Cannot Parse UpdatedDate");
				}
			}
			
			if (hash.ContainsKey ("createdDate") && hash ["createdDate"] != null) {
				DateTime myDate;
				if (DateTime.TryParseExact (hash ["createdDate"].ToString (), "yyyy-MM-ddTHH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out myDate)) {
					createDate = myDate;
				}else{
					Debug.Log("Cannot Parse CreatedDate");
				}
			}
		}
		
		/// <summary>
		/// Save the specified callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void Save(Action<bool, string> callback)
		{
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("Name", name);
			form.Add("Image", image);
			
			// If ItemType does not exist, add a new record to the database. Else update the ItemType values.
			if (createDate == null) {
				SocialGamificationManager.instance.CallWebservice (SocialGamificationManager.instance.GetUrl ("/api/items/type"), form, (string text, string error) =>
				                                                   {
					Debug.Log ("Save New ItemType : " + text);
					bool success = false;
					if (string.IsNullOrEmpty (error)) {
						success = true;
					}
					
					if (callback != null)
						callback (success, error);
				}, "POST");
			} else {
				SocialGamificationManager.instance.CallWebservice (SocialGamificationManager.instance.GetUrl ("/api/items/type"), form, (string text, string error) =>
				                                                   {
					Debug.Log ("Save Existing ItemType : " + text);
					bool success = false;
					if (string.IsNullOrEmpty (error)) {
						success = true;
					}
					
					if (callback != null)
						callback (success, error);
				}, "PUT");
			}
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace SocialGamification
{
	[System.Serializable]
	public class Item {
		
		public string id = "";
		public string actorId = "";
		public string itemTypeId = "";
		public string itemTypeName = "";
		public int quantity;
        public ItemType itemType = null;
		public DateTime? updateDate = null;
		public DateTime? createDate = null;
		
		public Item()
		{
		}
		
		public Item(string jsonString)
		{
			FromJson(jsonString);
		}
		
		public Item(Hashtable data)
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
			
			if (hash.ContainsKey("itemTypeId") && hash["itemTypeId"] != null)
			{
				itemTypeId = hash["itemTypeId"].ToString();
			}
			
			if (hash.ContainsKey("quantity") && hash["quantity"] != null)
			{
				int.TryParse(hash["quantity"].ToString(),out quantity);
			}
			
			if (hash.ContainsKey ("updatedDate") && hash ["updatedDate"] != null) {
				DateTime myDate;
				if (DateTime.TryParseExact (hash ["updatedDate"].ToString (), "yyyy-MM-ddTHH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out myDate)) {
					updateDate = myDate;
				}else{
					Debug.Log("Cannot parse updatedDate");
				}
			}
			
			if (hash.ContainsKey ("createdDate") && hash ["createdDate"] != null) {
				DateTime myDate;
				if (DateTime.TryParseExact (hash ["createdDate"].ToString (), "yyyy-MM-ddTHH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out myDate)) {
					createDate = myDate;
				}else{
					Debug.Log("Cannot parse createdDate");
				}
			}
			
			Debug.Log ("Id: " + id);
			Debug.Log ("ActorId: "+actorId);
			Debug.Log ("ItemTypeId: "+itemTypeId);
			Debug.Log ("ItemTypeName: "+itemTypeName);
			Debug.Log ("Quantity: "+quantity);
			Debug.Log ("UpdateDate: "+updateDate);
			Debug.Log ("CreatedDate: "+createDate);

		}

		/// <summary>
		/// Save the specified callback.
		/// </summary>
		/// <param name="callback">Callback.</param>
		public void Save(Action<bool, string> callback)
		{
			Dictionary<string, string> form = new Dictionary<string, string>();
			form.Add("ActorId", actorId);
			form.Add("ItemTypeId", itemTypeId);
			form.Add ("ItemTypeName", itemTypeName);
			form.Add("Quantity", quantity.ToString());

			// If Item does not exist, add a new record to the database. Else update the Item values.
			if (createDate == null) {
				SocialGamificationManager.instance.CallWebservice (SocialGamificationManager.instance.GetUrl ("/api/items"), form, (string text, string error) =>
				{
					Debug.Log ("Save New Item : " + text);
					bool success = false;
					if (string.IsNullOrEmpty (error)) {
						success = true;
					}
				
					if (callback != null)
						callback (success, error);
				}, "POST");
			} else {
				SocialGamificationManager.instance.CallWebservice (SocialGamificationManager.instance.GetUrl ("/api/items"), form, (string text, string error) =>
				                                                   {
					Debug.Log ("Save Existing Item : " + text);
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

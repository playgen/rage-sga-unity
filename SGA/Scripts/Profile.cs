using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace SocialGamification
{
	[System.Serializable]
	public class Profile : IUserProfile
	{
		protected string _id = "";
		protected string _userName = "";

		[System.NonSerialized]
		protected Texture2D _image;

		protected string _sessionToken = "";
		protected System.DateTime? _lastSeen;

		private List<ProfilePlatform> _platforms = new List<ProfilePlatform>();

		public List<ProfilePlatform> platforms { get { return _platforms; } }

        private List<Item> _inventory;

        public List<Item> inventory { get { return _inventory; } }

		/// <summary>
		/// Gets the identifier value as string.
		/// </summary>
		/// <value>The identifier.</value>
		public string id { get { return _id.ToString(); } }

		/// <summary>
		/// Gets or sets the name of the user.
		/// </summary>
		/// <value>The name of the user.</value>
		public string userName
		{
			get { return _userName; }
			set { _userName = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="SocialGamification.Profile"/> is a friend of the local user.
		/// </summary>
		/// <value><c>true</c> if is friend; otherwise, <c>false</c>.</value>
		public bool isFriend
		{
			get
			{
				if (SocialGamificationManager.isInitialized && SocialGamificationManager.instance.isAuthenticated)
				{
					foreach (var friend in SocialGamificationManager.localUser.friends)
					{
						if (friend.id.Equals(id))
							return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Gets the online state.
		/// </summary>
		/// <value>The state.</value>
		public virtual UserState state
		{
			get
			{
				if (_lastSeen.HasValue && _lastSeen != null)
				{
					int seconds = (int)(System.DateTime.Now - _lastSeen.Value).TotalSeconds;
					if (seconds <= SocialGamificationManager.instance.onlineSeconds)
					{
						if (SocialGamificationManager.instance.playingSeconds > 0 && seconds <= SocialGamificationManager.instance.playingSeconds)
							return UserState.Playing;
						return UserState.Online;
					}
				}
				return UserState.Offline;
			}
		}


        public void Init()
        {
            _inventory = new List<Item>();
            GetInventory();
        }

        public virtual void GetInventory()
        {
            if (_inventory != null)
            {


                SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/items"), null, (string text, string error) =>
                {
                    Debug.Log("Get Items: " + text);
                    if (string.IsNullOrEmpty(error))
                    {
                        ArrayList list = text.arrayListFromJson();
                        if (list != null)
                        {
                            foreach (Hashtable itemhash in list)
                            {
                                if (itemhash.ContainsKey("id") && itemhash["id"] != null)
                                {
                                    Item item = new Item(itemhash);
                                    _inventory.Add(item);
                                }
                            }
                        }
                    }

                    Debug.Log(_inventory);
                });
            }
        }

        //private List<Item> GetInventory()
        //{
        //    if (_inventory != null)
        //    {
        //        return _inventory;
        //    }

        //}

		/// <summary>
		/// Gets or sets the image.
		/// </summary>
		/// <value>The image.</value>
		public Texture2D image
		{
			get { return _image; }
			set { _image = value; }
		}

		/// <summary>
		/// Gets the session token.
		/// </summary>
		/// <value>The session token.</value>
		public string sessionToken { get { return _sessionToken; } }

		/// <summary>
		/// Gets the last seen date/time.
		/// </summary>
		/// <value>The last seen.</value>
		public System.DateTime? lastSeen { get { return _lastSeen; } }

		/// <summary>
		/// The email address.
		/// </summary>
		public string email;

		/// <summary>
		/// The custom data.
		/// </summary>
		public Hashtable customData = new Hashtable();

		/// <summary>
		/// The item types.
		/// </summary>
		public List<ItemType> itemTypes = new List<ItemType> ();


		public Profile()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CBUser"/> class from a JSON formatted string.
		/// </summary>
		/// <param name='jsonString'>
		/// JSON formatted string.
		/// </param>
		public Profile(string jsonString)
		{
			FromJson(jsonString);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CBUser"/> class from a Hashtable.
		/// </summary>
		/// <param name='hash'>
		/// Hash.
		/// </param>
		public Profile(Hashtable hash)
		{
			FromHashtable(hash);
		}

		/// <summary>
		/// Initialize the object from a JSON formatted string.
		/// </summary>
		/// <param name='jsonString'>
		/// Json string.
		/// </param>
		public virtual void FromJson(string jsonString)
		{
			FromHashtable(jsonString.hashtableFromJson());
		}

		/// <summary>
		/// Initialize the object from a hashtable.
		/// </summary>
		/// <param name='hash'>
		/// Hash.
		/// </param>
		public virtual void FromHashtable(Hashtable hash)
		{
			if (hash != null)
			{
				if (hash.ContainsKey("id") && hash["id"] != null)
				{
					_id = hash["id"].ToString();
				}

				//if (hash.ContainsKey("LastSeen") && hash["LastSeen"] != null)
				//{
				//	_lastSeen = null;
				//	if (!string.IsNullOrEmpty(hash["LastSeen"].ToString()))
				//	{
				//		System.DateTime dateTime;
				//		if (System.DateTime.TryParseExact(hash["LastSeen"].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
				//			_lastSeen = dateTime;
				//	}
				//}

				if (hash.ContainsKey("username") && hash["username"] != null)
				{
					_userName = hash["username"].ToString();
				}

				if (hash.ContainsKey("email") && hash["email"] != null)
				{
					email = hash["email"].ToString();
				}

				if (hash.ContainsKey("customData") && hash["customData"] != null && hash["customData"] is Hashtable)
				{
					customData = (Hashtable)hash["customData"];
				}

				if (hash.ContainsKey("platforms") && hash["platforms"] != null && hash["platforms"] is ArrayList)
				{
					_platforms.Clear();
					ArrayList listPlatforms = (ArrayList)hash["platforms"];
					if (listPlatforms != null)
					{
						foreach (Hashtable dataPlatform in listPlatforms)
						{
							_platforms.Add(new ProfilePlatform(dataPlatform));
						}
					}
				}

                //GetItemTypes();

			}
		}

		/// <summary>
		/// Gets the item types.
		/// </summary>
        //public virtual void GetItemTypes(){
        //    SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/items/types"), null, (string text, string error) =>
        //                                                      {
        //        Debug.Log("Get ItemTypes: "+text);
        //        if (string.IsNullOrEmpty(error))
        //        {
        //            items.Clear();
					
        //            ArrayList list = text.arrayListFromJson();
        //            if (list != null)
        //            {
        //                foreach (Hashtable itemhash in list)
        //                {
        //                    if (itemhash.ContainsKey("name") && itemhash["name"] != null)
        //                    {
        //                        ItemType itemType = new ItemType(itemhash);
        //                        itemTypes.Add(itemType);
        //                    }
        //                }
        //            }
        //        }

        //        AddItemType();

        //    });
        //}

		/// <summary>
		/// Adds the type of the item.
		/// </summary>
        //public virtual void AddItemType(){
        //    // Test - Create New Item
        //    ItemType itemType = new ItemType();
        //    itemType.name = "ItemX";
        //    itemType.image = "ItemXImage";
        //    itemType.Save((bool success, string error)=> {
        //        if(success){
        //            Debug.Log("ItemType Saved Successfully");
        //        }else{
        //            Debug.Log("ItemType was not saved." + error);
        //        }
        //        itemTypes.Add(itemType);
        //        GetItems();
        //    });
        //}

		/// <summary>
		/// Gets the items.
		/// </summary>
		public virtual void GetItems(){
			SocialGamificationManager.instance.CallWebservice(SocialGamificationManager.instance.GetUrl("api/items/"), null, (string text, string error) =>
			                                                  {
				Debug.Log("Get Item: "+text);
				if (string.IsNullOrEmpty(error))
				{
                    //items.Clear();
					
					ArrayList list = text.arrayListFromJson();
					if (list != null)
					{
						foreach (Hashtable itemhash in list)
						{
							if (itemhash.ContainsKey("itemTypeId") && itemhash["itemTypeId"] != null)
							{
								Item matchItem = new Item(itemhash);
                                //items.Add(matchItem);
							}
						}
					}
				}
				AddItem();
			});

		}

		/// <summary>
		/// Adds the item.
		/// </summary>
		public virtual void AddItem(){
			// Test - Create New Item
			Item item = new Item();
			item.quantity = 1;
			item.itemTypeId = itemTypes [0].id;
			item.itemTypeName = itemTypes[0].name;
			item.Save((bool success, string error)=> {
				if(success){
					Debug.Log("Item Saved Successfully");
				}else{
					Debug.Log("Item was not saved." + error);
				}
			});
            //items.Add(item);
		}
	}
}

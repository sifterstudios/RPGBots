using System;
using System.Collections.Generic;
using GYAInternal.Json;
using GYAInternal.Json.Converters;

namespace XeirGYA
{
    public class GYAData
    {
        public string version { get; set; }
        public List<Asset> Assets { get; set; }
        public List<Group> Groups { get; set; }

        public GYAData()
        {
            version = String.Empty;
            Assets = new List<Asset>();
            Groups = new List<Group>();
        }

        public class Asset
        {
            public Link link { get; set; }
            public string unity_version { get; set; }
            public string pubdate { get; set; }
            public string version { get; set; }
            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int upload_id { get; set; }
            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int version_id { get; set; }
            public string description { get; set; }
            public string publishnotes { get; set; }
            public Category category { get; set; }
            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int id { get; set; }
            public string title { get; set; }
            public Publisher publisher { get; set; }

            // Not included within the unityPackages
            public string filePath { get; set; }
            public double fileSize { get; set; }
            public DateTimeOffset fileDataCreated { get; set; }
            public DateTimeOffset fileDateCreated { get; set; }
            public bool isExported { get; set; } // True = User Created Pkg, False = Asset Store Pkg
            public bool isDamaged { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public GYA.svCollection collection { get; set; } // Use Store, Standard, Old, User when creating json
            [JsonIgnore]
            public bool isOldToMove { get; set; } // Is old file that can be moved/consolidated
            [JsonIgnore]
            public bool isMarked { get; set; } // Is asset marked for import or multiple
            [JsonIgnore]
            public bool isInAGroup { get; set; } // Is asset in a group
            [JsonIgnore]
            public bool isInAGroupLockedVersion { get; set; } // Is asset in a group and version locked
            [JsonIgnore]
            public bool isDeprecated { get; set; } // Is asset deprecated from ASPurchased
            [JsonIgnore]
            public string icon { get; set; } // Icon link from ASPurchased
            [JsonIgnore]
            public bool isFileMissing { get; set; } // Is file missing
            [JsonIgnore]
            public bool isUpdateAvail { get; set; } // Is update avail
            [JsonIgnore]
            public bool isLatestVersion { get; set; } // Is latest version of asset
            [JsonIgnore]
            public bool notDownloaded { get; set; } // Is not downloaded
            [JsonIgnore]
            public bool isInPurchasedList { get; set; }
            [JsonIgnore]
            public DateTimeOffset datePurchased { get; set; }
            [JsonIgnore]
            public DateTimeOffset dateCreated { get; set; }
            [JsonIgnore]
            public DateTimeOffset dateUpdated { get; set; }
            [JsonIgnore]
            public ASPurchased.Result Purchased { get; set; }
			[JsonIgnore]
			public GYAAssetInfo.AssetInfo AssetInfo { get; set; }
            
			// Pre-calc'd fields
            [JsonIgnore]
            public bool isSameVersionAsUnity { get; set; } // Is version already part of the asset filename
            [JsonIgnore]
            public bool isFavorite { get; set; } // Is asset in favorites group
            [JsonIgnore]
            public bool isVersionAppended { get; set; } // Is version already part of the asset filename
            [JsonIgnore]
			public string titleWithVersion { get; set; } // Title with version appended
            [JsonIgnore]
            public DateTime pubDateToDateTime { get; set; }

            public Asset()
            {
                unity_version = String.Empty;
                pubdate = String.Empty;
                version = String.Empty;
                upload_id = 0;
                version_id = 0;
                description = String.Empty;
                publishnotes = String.Empty;
                id = 0;
                title = String.Empty;
                filePath = String.Empty;
                fileSize = 0;
                isExported = false;
                isDamaged = false;
                collection = GYA.svCollection.Store;
                isOldToMove = false;
                isMarked = false;
                isInAGroup = false;
                isInAGroupLockedVersion = false;
                isDeprecated = false;
                icon = String.Empty;
                isFileMissing = false;
                isUpdateAvail = false;
                isLatestVersion = true;
	            notDownloaded = false;
	            // Does it exists in the ASPurchased list, remains false if ASPurchased not downloaded
                isInPurchasedList = false;

                link = new Link();
                category = new Category();
                publisher = new Publisher();

				Purchased = new ASPurchased.Result();
				AssetInfo = new GYAAssetInfo.AssetInfo();

                isSameVersionAsUnity = false;
                isFavorite = false;
                isVersionAppended = false;
				titleWithVersion = String.Empty;
            }

            public class Link
            {
                public string type { get; set; }

                [JsonConverter(typeof(GYA.StringToIntConverter))]
                public int id { get; set; }

                public Link()
                {
                    type = String.Empty;
                    id = 0;
                }
            }

            public class Category
            {
                public string label { get; set; }

                [JsonConverter(typeof(GYA.StringToIntConverter))]
                public int id { get; set; }

                public Category()
                {
                    label = String.Empty;
                    id = 0;
                }
            }

            public class Publisher
            {
                public string label { get; set; }

                [JsonConverter(typeof(GYA.StringToIntConverter))]
                public int id { get; set; }

                public Publisher()
                {
                    label = String.Empty;
                    id = 0;
                }
            }
        }

        public class GroupAsset
        {
            // Asset Store Packages
            public string title { get; set; } // Package title
            public bool isExported { get; set; } // Is asset store package?
            public bool useLatestVersion { get; set; } // If isassetstorepkg, use latest version of asset?
            public int id { get; set; } // If isassetstorepkg, assets id
            public int version_id { get; set; } // If useLatestVersion, use this version
            // Used for exported (Non-Asset Store) packages, Get the title as the filename in the path
            public string filePath { get; set; } // If !isassetstorepkg, use filepath

            // Generated later
	        [JsonIgnore]
	        public bool isFileMissing { get; set; } // Is file missing
	        [JsonIgnore]
	        public int group_ID { get; set; } // Assists multi asset (re)move
	        
            public GroupAsset()
            {
                title = String.Empty;
                isExported = false;
                useLatestVersion = true;
                id = 0;
                version_id = 0;
                filePath = String.Empty;
	            isFileMissing = false;
	            group_ID = -1; // -1 = unassigned
            }
        }

        public class Group
        {
            public string name { get; set; }
            public string notes { get; set; }
            public List<GroupAsset> Assets { get; set; }

            public Group()
            {
                name = String.Empty;
                notes = String.Empty;
                Assets = new List<GroupAsset>();
            }

            public Group(string pName, string pNotes, List<GroupAsset> pAssets)
            {
                name = pName;
                notes = pNotes;
                Assets = new List<GroupAsset>(pAssets);
            }
        }
    }
}
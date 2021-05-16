using System;
using System.Collections.Generic;
using GYAInternal.Json;

// GYA Asset Info - User maintained asset info for each package

namespace XeirGYA
{
    public class GYAAssetInfo
    {
		public List<AssetInfo> Data { get; set; }

        public GYAAssetInfo()
        {
			Data = new List<AssetInfo>();
        }

        public class AssetInfo
        {
            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int id { get; set; }
            public string filePath { get; set; }

			public List<string> Links { get; set; }
            public string Notes { get; set; }

			public AssetInfo()
            {
                id = 0;
                filePath = String.Empty;

				Links = new List<string>();
                Notes = String.Empty;
            }
        }
    }
}
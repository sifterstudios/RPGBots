using System.Collections.Generic;
using GYAInternal.Json;

namespace XeirGYA
{
    public class ASPurchased
    {
        public List<Result> results { get; set; }

        public ASPurchased()
        {
        }

        public class Result
        {
            public object local_version_name { get; set; }
            public Category category { get; set; }

            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int id { get; set; }

            public object published_at { get; set; }

            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int can_comment { get; set; }

            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int can_download { get; set; }

            public object last_downloaded_at { get; set; }
            public object local_path { get; set; }

            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int is_complete_project { get; set; }

            public List<string> tags { get; set; }
            public Publisher publisher { get; set; }
            public object purchased_at { get; set; }
            public string icon { get; set; }

            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int can_update { get; set; }

            public string type { get; set; }

            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int user_rating { get; set; }

            public string status { get; set; }
            public string name { get; set; }

            public object created_at { get; set; }
            public object updated_at { get; set; }

            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int in_users_downloads { get; set; }

            public Result()
            {
            }
        }

        public class Category
        {
            public string name { get; set; }

            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int id { get; set; }

            public Category()
            {
            }
        }

        public class Publisher
        {
            public string name { get; set; }

            [JsonConverter(typeof(GYA.StringToIntConverter))]
            public int id { get; set; }

            public Publisher()
            {
            }
        }
    }
}
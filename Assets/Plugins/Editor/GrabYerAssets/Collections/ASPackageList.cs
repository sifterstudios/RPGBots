using System;
using System.Collections.Generic;

// AS Internal Unity Package Scan, AS Folder ONLY

namespace XeirGYA
{
    public class ASPackageList
    {
        public List<Result> results { get; set; }

        public class Publisher
        {
            public string label { get; set; }
            public string id { get; set; }

            public Publisher()
            {
                label = String.Empty;
                id = String.Empty;
            }
        }

        public class Category
        {
            public string label { get; set; }
            public string id { get; set; }

            public Category()
            {
                label = String.Empty;
                //id = 0;
                id = String.Empty;
            }
        }

        public class Link
        {
            public string type { get; set; }
            public string id { get; set; }

            public Link()
            {
                type = String.Empty;
                id = String.Empty;
            }
        }

        public class Result
        {
            public string title { get; set; }
            public string id { get; set; } // # = AS asset, path = standard asset
            public string version { get; set; }
            public string version_id { get; set; }
            public string pubdate { get; set; }
            public object description { get; set; }
            public Publisher publisher { get; set; }
            public Category category { get; set; }
            public Link link { get; set; }
            public string local_path { get; set; }
            public string local_icon { get; set; }

            public Result()
            {
                publisher = new Publisher();
                category = new Category();
                link = new Link();
                title = String.Empty;
                id = String.Empty;
                version = String.Empty;
                version_id = String.Empty;
                local_icon = String.Empty;
                local_path = String.Empty;
                pubdate = String.Empty;
            }
        }
    }
}
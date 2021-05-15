using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Entities
{
    public class Movie
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string MetaTitle { get; set; }
        public string title { get; set; }
        public string tagline { get; set; }
        public string released { get; set; }
        public string Image { get; set; }
    }
}

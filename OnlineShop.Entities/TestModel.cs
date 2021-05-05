using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OnlineShop.Entities
{
    public class TestModel
    {
        [BsonId]
        public ObjectId ID { set; get; }
        [BsonElement("MaLop")]
        public string MaLop { set; get; }
        [BsonElement("TenLop")]
        public string TenLop { set; get; }
    }
}

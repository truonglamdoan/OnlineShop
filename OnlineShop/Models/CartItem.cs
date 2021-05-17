using Model.EF;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineShop.Models
{
    [Serializable]
    public class CartItem
    {
        [BsonId]
        public ObjectId ID { set; get; }
        public Product Product { set; get; }
        public int Quantity { set; get; }
    }
}
using Model.Dao;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Model.EF;
using Common;
using System.Configuration;
using System.IO;
using OnlineShop.Core;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OnlineShop.Common;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        private MongoDbContext mongoDbContext;
        private IMongoCollection<OnlineShop.Entities.TestModel> testModel;
        private const string CartSession = "CartSession";
        // GET: Cart
        public ActionResult Index()
        {
            mongoDbContext = new MongoDbContext();
            IMongoCollection<Product> dataItem = mongoDbContext.database.GetCollection<Product>("Test1");
            var user = (UserLogin)Session[Common.CommonConstants.USER_SESSION];
            var cart = Session[CartSession];
            var list = new List<CartItem>();

            // Get data từ MongoDb

            //// Find item
            //var dataItemEdit = dataItem.AsQueryable<Product>().Where(x => x.UserID == user.UserID.ToString());
            var dataItemEdit = dataItem.AsQueryable<Product>().ToList();

            foreach(var item in dataItemEdit)
            {
                if(item.UserID == user.UserID.ToString())
                {
                    CartItem itemCart = new CartItem()
                    {
                        Product = item,
                        Quantity = item.Quantity
                    };
                    list.Add(itemCart);
                }
                
            }
            if(list.Count <= 0)
            {
                if (cart != null)
                {
                    list = (List<CartItem>)cart;
                }
            }

            return View(list);
        }

        public JsonResult DeleteAll()
        {
            Session[CartSession] = null;
            return Json(new
            {
                status = true
            });
        }

        public JsonResult Delete(long id)
        {
            var sessionCart = (List<CartItem>)Session[CartSession];
            sessionCart.RemoveAll(x => x.Product.ID == id);
            Session[CartSession] = sessionCart;
            return Json(new
            {
                status = true
            });
        }
        public JsonResult Update(string cartModel)
        {
            mongoDbContext = new MongoDbContext();
            IMongoCollection<Product> dataItem = mongoDbContext.database.GetCollection<Product>("Test1");
            var jsonCart = new JavaScriptSerializer().Deserialize<List<CartItem>>(cartModel);
            var sessionCart = (List<CartItem>)Session[CartSession];

            foreach(var item in jsonCart)
            {
                var filter = Builders<Product>.Filter.Eq("ID", item.Product.ID);
                var update = Builders<Product>.Update
                           .Set("Quantity", item.Quantity);
                var resultData = dataItem.UpdateOne(filter, update);
            }

            foreach (var item in sessionCart)
            {
                var jsonItem = jsonCart.SingleOrDefault(x => x.Product.ID == item.Product.ID);
                if (jsonItem != null)
                {
                    item.Quantity = jsonItem.Quantity;
                }
            }
            Session[CartSession] = sessionCart;
            return Json(new
            {
                status = true
            });
        }
        public ActionResult AddItem(long productId, int quantity)
        {
            var user = Session[Common.CommonConstants.USER_SESSION];
            mongoDbContext = new MongoDbContext();
            IMongoCollection<Product> dataItem = mongoDbContext.database.GetCollection<Product>("Test1");

            // Find item
            Product dataItemEdit = dataItem.AsQueryable<Product>().SingleOrDefault(x => x.ID == productId);

            var product = new ProductDao().ViewDetail(productId);

            if(dataItemEdit != null)
            {
                dataItemEdit.UserID = (user == null? "" : ((UserLogin)user).UserID.ToString());
                var filter = Builders<Product>.Filter.Eq("_id", ObjectId.Parse(dataItemEdit._id.ToString()));
                var update = Builders<Product>.Update
                           .Set("Quantity", dataItemEdit.Quantity + 1);
                var resultData = dataItem.UpdateOne(filter, update);
            }
            else
            {
                product.UserID = (user == null ? "" : ((UserLogin)user).UserID.ToString());
                product.Quantity = product.Quantity + 1;
                // Insert vào mongo
                dataItem.InsertOne(product);
            }

            var cart = Session[CartSession];
            if (cart != null)
            {
                var list = (List<CartItem>)cart;
                if (list.Exists(x => x.Product.ID == productId))
                {

                    foreach (var item in list)
                    {
                        if (item.Product.ID == productId)
                        {
                            item.Quantity += quantity;
                        }
                    }
                }
                else
                {
                    //tạo mới đối tượng cart item
                    var item = new CartItem();
                    item.Product = product;
                    item.Quantity = quantity;
                    list.Add(item);
                }
                //Gán vào session
                Session[CartSession] = list;
                // Insert vào mongo
                //dataItem.InsertOne(list);
            }
            else
            {
                //tạo mới đối tượng cart item
                var item = new CartItem();
                item.Product = product;
                item.Quantity = quantity;
                var list = new List<CartItem>();
                list.Add(item);
                //Gán vào session
                Session[CartSession] = list;
                //// Insert vào mongo
                //dataItem.InsertOne(product);
            }
            

            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Payment()
        {
            var cart = Session[CartSession];
            var list = new List<CartItem>();
            if (cart != null)
            {
                list = (List<CartItem>)cart;
            }
            return View(list);
        }

        [HttpPost]
        public ActionResult Payment(string shipName,string mobile,string address,string email)
        {
            var order = new Order();
            order.CreatedDate = DateTime.Now;
            order.ShipAddress = address;
            order.ShipMobile = mobile;
            order.ShipName = shipName;
            order.ShipEmail = email;

            try
            {
                var id = new OrderDao().Insert(order);
                var cart = (List<CartItem>)Session[CartSession];
                var detailDao = new Model.Dao.OrderDetailDao();
                decimal total = 0;
                foreach (var item in cart)
                {
                    var orderDetail = new OrderDetail();
                    orderDetail.ProductID = item.Product.ID;
                    orderDetail.OrderID = id;
                    orderDetail.Price = item.Product.Price;
                    orderDetail.Quantity = item.Quantity;
                    detailDao.Insert(orderDetail);

                    total += (item.Product.Price.GetValueOrDefault(0) * item.Quantity);
                }
                string content = System.IO.File.ReadAllText(Server.MapPath("~/assets/client/template/neworder.html"));

                content = content.Replace("{{CustomerName}}", shipName);
                content = content.Replace("{{Phone}}", mobile);
                content = content.Replace("{{Email}}", email);
                content = content.Replace("{{Address}}", address);
                content = content.Replace("{{Total}}", total.ToString("N0"));
                var toEmail = ConfigurationManager.AppSettings["ToEmailAddress"].ToString();

                new MailHelper().SendMail(email, "Đơn hàng mới từ OnlineShop", content);
                new MailHelper().SendMail(toEmail, "Đơn hàng mới từ OnlineShop", content);
            }
            catch (Exception ex)
            {
                //ghi log
                return Redirect("/loi-thanh-toan");
            }
            return Redirect("/hoan-thanh");
        }

        public ActionResult Success()
        {
            return View();
        }
    }
}
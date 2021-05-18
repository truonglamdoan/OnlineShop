using Model.Dao;
using Model.EF;
using MongoDB.Driver;
using OnlineShop.Common;
using OnlineShop.Core;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace OnlineShop.Controllers
{
    public class HomeController : Controller
    {

        private MongoDbContext mongoDbContext;
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.Slides = new SlideDao().ListAll();
            var productDao = new ProductDao();
            //ViewBag.NewProducts = productDao.ListNewProduct(4);
            ViewBag.NewProducts = productDao.ListProducts();
            ViewBag.ListFeatureProducts = productDao.ListFeatureProduct(4);
            return View();
        }

        [ChildActionOnly]
        [OutputCache(Duration = 3600 * 24)]
        public ActionResult MainMenu()
        {
            var model = new MenuDao().ListByGroupId(1);
            return PartialView(model);
        }
        [ChildActionOnly]
        //[OutputCache(Duration = 3600 * 24)]
        public ActionResult TopMenu()
        {
            var model = new MenuDao().ListByGroupId(2);
            return PartialView(model);
        }
        [ChildActionOnly]
        public PartialViewResult HeaderCart()
        {
            mongoDbContext = new MongoDbContext();
            IMongoCollection<Product> dataItem = mongoDbContext.database.GetCollection<Product>("Test1");
            var user = (UserLogin)Session[Common.CommonConstants.USER_SESSION];

            var cart = Session[CommonConstants.CartSession];
            var list = new List<CartItem>();

            var dataItemEdit = dataItem.AsQueryable<Product>().ToList();
            if (user == null)
            {
                if (cart != null)
                {
                    list = (List<CartItem>)cart;
                }
            }
            else
            {
                foreach (var item in dataItemEdit)
                {
                    if (item.UserID == user.UserID.ToString())
                    {
                        CartItem itemCart = new CartItem()
                        {
                            Product = item,
                            Quantity = item.Quantity
                        };
                        list.Add(itemCart);
                    }

                }
            }

            return PartialView(list);
        }
        [ChildActionOnly]
        [OutputCache(Duration = 3600 * 24)]
        public ActionResult Footer()
        {
            var model = new FooterDao().GetFooter();
            return PartialView(model);
        }
    }
}
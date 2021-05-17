﻿using BotDetect.Web.UI.Mvc;
using Facebook;
using Model.Dao;
using Model.EF;
using OnlineShop.Common;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
//using OnlineShop.Entities;
using OnlineShop.Bussiness;
using MongoDB.Driver;
using OnlineShop.Core;
using MongoDB.Bson;
using Neo4jClient.Cypher;
using Neo4jClient;
using Neo4j.Driver.V1;
using Newtonsoft.Json;

namespace OnlineShop.Controllers
{
    
    public class UserController : Controller
    {
        private MongoDbContext mongoDbContext;
        private GraphDbContext graphDbContext;
        private IMongoCollection<OnlineShop.Entities.TestModel> testModel;

        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }

        // GET: User
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        public ActionResult Login()
        {

            return View();
        }
        public ActionResult LoginFacebook()
        {
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = ConfigurationManager.AppSettings["FbAppId"],
                client_secret = ConfigurationManager.AppSettings["FbAppSecret"],
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                scope = "email",
            });

            return Redirect(loginUrl.AbsoluteUri);
        }

        public ActionResult FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = ConfigurationManager.AppSettings["FbAppId"],
                client_secret = ConfigurationManager.AppSettings["FbAppSecret"],
                redirect_uri = RedirectUri.AbsoluteUri,
                code = code
            });


            var accessToken = result.access_token;
            if (!string.IsNullOrEmpty(accessToken))
            {
                fb.AccessToken = accessToken;
                // Get the user's information, like email, first name, middle name etc
                dynamic me = fb.Get("me?fields=first_name,middle_name,last_name,id,email");
                string email = me.email;
                string userName = me.email;
                string firstname = me.first_name;
                string middlename = me.middle_name;
                string lastname = me.last_name;

                var user = new User();
                user.Email = email;
                user.UserName = email;
                user.Status = true;
                user.Name = firstname + " " + middlename + " " + lastname;
                user.CreatedDate = DateTime.Now;
                var resultInsert = new UserDao().InsertForFacebook(user);
                if (resultInsert > 0)
                {
                    var userSession = new UserLogin();
                    userSession.UserName = user.UserName;
                    userSession.UserID = user.ID;
                    Session.Add(CommonConstants.USER_SESSION, userSession);
                }
            }
            return Redirect("/");
        }
        public ActionResult Logout()
        {
            Session[CommonConstants.USER_SESSION] = null;
            return Redirect("/");
        }
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var dao = new UserDao();
                var result = dao.Login(model.UserName, Encryptor.MD5Hash(model.Password));
                if (result == 1)
                {
                    var user = dao.GetById(model.UserName);
                    var userSession = new UserLogin();
                    userSession.UserName = user.UserName;
                    userSession.UserID = user.ID;
                    Session.Add(CommonConstants.USER_SESSION, userSession);


                    List<OnlineShop.Entities.User> data = new UserBL().LoadDataMaster();

                    // MongoDb
                    try
                    {

                        //mongoDbContext = new MongoDbContext();
                        //// select
                        //testModel = mongoDbContext.database.GetCollection<OnlineShop.Entities.TestModel>("Test1");
                        //// select by Key
                        ////var id = new ObjectId(string.Empty);
                        ////var dataItem = testModel.AsQueryable<OnlineShop.Entities.TestModel>().FirstOrDefault(x => x.ID == id);
                        //var dataItem = testModel.AsQueryable<OnlineShop.Entities.TestModel>().ToList();

                        //// Insert
                        //Entities.TestModel item = new Entities.TestModel();
                        //testModel.InsertOne(item);
                        //// Edit
                        //// find ==> update
                        //Entities.TestModel dataItemEdit = testModel.AsQueryable<OnlineShop.Entities.TestModel>().SingleOrDefault(x => x.ID == id);
                        //var filter = Builders<Entities.TestModel>.Filter.Eq("_id", ObjectId.Parse(string.Empty));
                        //var update = Builders<Entities.TestModel>.Update
                        //    .Set("MaLop", dataItemEdit.MaLop)
                        //    .Set("TenLop", dataItemEdit.TenLop);
                        //var resultData = testModel.UpdateOne(filter, update);
                        //// Delete
                        //testModel.DeleteOne(filter);

                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }

                    // GraphNeo4J
                    try
                    {
                        graphDbContext = new GraphDbContext();

                        var query = graphDbContext.GraphClient.Cypher
                                   .Match("(m:Movie)")
                                   .Return(m => m.As<Entities.Movie>())
                                   .Limit(100);

                        ////You can see the cypher query here when debugging
                        var dataList = query.Results.ToList();

                        var query1 = graphDbContext.GraphClient.Cypher
                                   .Match("p=()-[r:ACTED_IN]->()")
                                   .Return(p => p.As<Entities.Movie>())
                                   .Limit(100);

                        ////You can see the cypher query here when debugging
                        var dataList1 = query1.Results.ToList();

                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }

                    List<OnlineShop.Entities.TestModel> listTest = testModel.AsQueryable<OnlineShop.Entities.TestModel>().ToList();

                    // Get data test mongo
                    return Redirect("/");
                }
                else if (result == 0)
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại.");
                }
                else if (result == -1)
                {
                    ModelState.AddModelError("", "Tài khoản đang bị khoá.");
                }
                else if (result == -2)
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                }
                else
                {
                    ModelState.AddModelError("", "đăng nhập không đúng.");
                }

            }
            return View(model);
        }
        [HttpPost]
        [CaptchaValidation("CaptchaCode", "registerCapcha", "Mã xác nhận không đúng!")]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var dao = new UserDao();
                if (dao.CheckUserName(model.UserName))
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                }
                else if (dao.CheckEmail(model.Email))                {
                    ModelState.AddModelError("", "Email đã tồn tại");
                }
                else
                {
                    var user = new User();
                    user.Name = model.Name;
                    user.Password = Encryptor.MD5Hash(model.Password);
                    user.Phone = model.Phone;
                    user.Email = model.Email;
                    user.Address = model.Address;
                    user.CreatedDate = DateTime.Now;
                    user.Status = true;
                    if (!string.IsNullOrEmpty(model.ProvinceID))
                    {
                        user.ProvinceID = int.Parse(model.ProvinceID);
                    }
                    if (!string.IsNullOrEmpty(model.DistrictID))
                    {
                        user.DistrictID = int.Parse(model.DistrictID);
                    }
                   
                    var result = dao.Insert(user);
                    if (result > 0)
                    {
                        ViewBag.Success = "Đăng ký thành công";
                        model = new RegisterModel();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Đăng ký không thành công.");
                    }
                }
            }
            return View(model);
        }

        public JsonResult LoadProvince()
        {
            var xmlDoc = XDocument.Load(Server.MapPath(@"~/assets/client/data/Provinces_Data.xml"));

            var xElements = xmlDoc.Element("Root").Elements("Item").Where(x => x.Attribute("type").Value == "province");
            var list = new List<ProvinceModel>();
            ProvinceModel province = null;
            foreach (var item in xElements)
            {
                province = new ProvinceModel();
                province.ID = int.Parse(item.Attribute("id").Value);
                province.Name = item.Attribute("value").Value;
                list.Add(province);

            }
            return Json(new
            {
                data = list,
                status = true
            });
        }
        public JsonResult LoadDistrict(int provinceID)
        {
            var xmlDoc = XDocument.Load(Server.MapPath(@"~/assets/client/data/Provinces_Data.xml"));

            var xElement = xmlDoc.Element("Root").Elements("Item")
                .Single(x => x.Attribute("type").Value == "province" && int.Parse(x.Attribute("id").Value) == provinceID);

            var list = new List<DistrictModel>();
            DistrictModel district = null;
            foreach (var item in xElement.Elements("Item").Where(x => x.Attribute("type").Value == "district"))
            {
                district = new DistrictModel();
                district.ID = int.Parse(item.Attribute("id").Value);
                district.Name = item.Attribute("value").Value;
                district.ProvinceID = int.Parse(xElement.Attribute("id").Value);
                list.Add(district);

            }
            return Json(new
            {
                data = list,
                status = true
            });
        }

    }
}
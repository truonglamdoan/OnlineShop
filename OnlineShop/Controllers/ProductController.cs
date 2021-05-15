using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.Dao;
using OnlineShop.Core;

namespace OnlineShop.Controllers
{
    public class ProductController : Controller
    {
        private GraphDbContext graphDbContext;
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public PartialViewResult ProductCategory()
        {
            var model = new ProductCategoryDao().ListAll();
            return PartialView(model);
        }
        public JsonResult ListName(string q)
        {
            var data = new ProductDao().ListName(q);
            return Json(new
            {
                data = data,
                status = true
            },JsonRequestBehavior.AllowGet);
        }
        public ActionResult Category(long cateId, int page = 1, int pageSize = 1)
        {
            var category = new CategoryDao().ViewDetail(cateId);
            ViewBag.Category = category;
            int totalRecord = 0;
            var model = new ProductDao().ListByCategoryId(cateId, ref totalRecord, page, pageSize);

            ViewBag.Total = totalRecord;
            ViewBag.Page = page;

            int maxPage = 5;
            int totalPage = 0;

            totalPage = (int)Math.Ceiling((double)(totalRecord / pageSize));
            ViewBag.TotalPage = totalPage;
            ViewBag.MaxPage = maxPage;
            ViewBag.First = 1;
            ViewBag.Last = totalPage;
            ViewBag.Next = page + 1;
            ViewBag.Prev = page - 1;

            return View(model);
        }
        public ActionResult Search(string keyword, int page = 1, int pageSize = 1)
        {
            int totalRecord = 0;
            var model = new ProductDao().Search(keyword, ref totalRecord, page, pageSize);

            ViewBag.Total = totalRecord;
            ViewBag.Page = page;
            ViewBag.Keyword = keyword;
            int maxPage = 5;
            int totalPage = 0;

            totalPage = (int)Math.Ceiling((double)(totalRecord / pageSize));
            ViewBag.TotalPage = totalPage;
            ViewBag.MaxPage = maxPage;
            ViewBag.First = 1;
            ViewBag.Last = totalPage;
            ViewBag.Next = page + 1;
            ViewBag.Prev = page - 1;

            return View(model);
        }

        [OutputCache(CacheProfile = "Cache1DayForProduct")]
        public ActionResult Detail(long id)
        {
            var product = new ProductDao().ViewDetail(id);
            ViewBag.Category = new ProductCategoryDao().ViewDetail(product.CategoryID.Value);

            List<Model.EF.Product> productList = new ProductDao().ListProducts();


            ViewBag.RelatedProducts = new ProductDao().ListRelatedProducts(id);

            graphDbContext = new GraphDbContext();

            var query = graphDbContext.GraphClient.Cypher
                       .Match("(m:Movie)")
                       .Return(m => m.As<Entities.Movie>())
                       .Limit(100);

            ////You can see the cypher query here when debugging
            var dataList = query.Results.ToList();
            var result = graphDbContext.GraphClient.Cypher
                                       .Match(@"(person: Person)-[:ACTED_IN]->(movie: Movie)")
                                       .Where((Entities.Movie movie) => movie.title == product.Name)
                                       //.WithParam("nameParam", "Cuba Gooding Jr.")
                                       .Return((person, movie) => new
                                       {
                                           Person = person.As<Entities.Person>(),
                                           Movie = movie.As<Entities.Movie>()
                                       }).ResultsAsync;


            var listAll = result.Result.ToList();
            List<Entities.Movie> listMovie = new List<Entities.Movie>();
            List<Entities.Person> listPerson = new List<Entities.Person>();
            foreach(var item in listAll)
            {
                if(item.Movie != null)
                {
                    string tempId = productList.Find(x => x.Name == item.Movie.title).ID.ToString();
                    string temp = item.Movie.title.Replace(' ', '_');
                    item.Movie.Image = "/assets/client/images/Movie/" + temp + ".jpg";
                    item.Movie.MetaTitle = temp;
                    item.Movie.Id = tempId == "" ? "1":temp;
                    listMovie.Add(item.Movie);
                }

                if (item.Person != null)
                {
                    //string tempId = productList.Find(x => x.Name == item.Person.name).ID.ToString();
                    string temp = item.Person.name.Replace(' ', '_');
                    item.Person.Image = "/assets/client/images/Person/" + temp + ".jpg";
                    item.Person.MetaTitle = temp;
                    item.Person.Id = "1";
                    listPerson.Add(item.Person);
                }
            }

            ViewBag.listMovie = listMovie;
            ViewBag.listPerson = listPerson;
            return View(product);
        }
    }
}
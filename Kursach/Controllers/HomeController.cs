using Kursach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace Kursach.Controllers
{
    public class HomeController : Controller
    {
        CommonContext db = new CommonContext();

        public ActionResult Index(int? page)
        {
            var movies = db.Movies;
            if (page == null)
            {
                return View(movies.Take(20).ToList());
            }
            else
            {
                return View(movies.Skip(0 + 20 * (int)page).Take(20).ToList());
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Index(InnerUser data)
        {
            //string userID = User.Identity.GetUserId();
            var movies = db.Movies.Where(b => b.MovieID == data.MovieID).FirstOrDefault();
            data.Movies = movies;
            db.InnerUsers.Add(data);
            db.SaveChanges();
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult Import()
        {
            
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase upload)
        {
            string fileName = null;
            if (upload != null)
            {
                // получаем имя файла
                fileName = System.IO.Path.GetFileName(upload.FileName);
                // сохраняем файл в папку Files в проекте
                upload.SaveAs(Server.MapPath("~/App_Data/Upload/" + fileName));
            }
            

            DataTable dt = new DataTable();

            /* moviesmodel
            dt.Columns.AddRange(new DataColumn[4] {new DataColumn("ID", typeof(int)),
            new DataColumn("MovieID", typeof(int)),
            new DataColumn("Title", typeof(string)),
            new DataColumn("Genres",typeof(string))});
            */

            /* linksmodels
            dt.Columns.AddRange(new DataColumn[4] {new DataColumn("LinksModelsID", typeof(int)),
            new DataColumn("MovieID", typeof(int)),
            new DataColumn("ImdbID", typeof(string)),
            new DataColumn("TmdbID",typeof(string))});
            */

            
            dt.Columns.AddRange(new DataColumn[5] {new DataColumn("RatingsModelsID", typeof(int)),
            new DataColumn("UserID", typeof(int)),
            new DataColumn("MovieID", typeof(int)),
            new DataColumn("Rating",typeof(float)),
            new DataColumn("Timestamp",typeof(long))});
            


            string csvData = Server.MapPath("~/App_Data/Upload/" + fileName);

            // You can also read from a file
            Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(csvData);

            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters(",");

            int it = 1;
            string[] fields;
            bool colNames = true;
            while (!parser.EndOfData)
            {
                fields = parser.ReadFields();
                if (!colNames)
                {
                    dt.Rows.Add();
                    int i = 0;
                    dt.Rows[dt.Rows.Count - 1][i] = it;
                    i++;
                    foreach (string field in fields)
                    {
                        dt.Rows[dt.Rows.Count - 1][i] = field.Replace('.', ',');
                        i++;
                    }
                    it++;
                }
                else colNames = false;
            }

            parser.Close();

            string consString = ConfigurationManager.ConnectionStrings["CommonContext"].ConnectionString;
            using (SqlConnection con = new SqlConnection(consString))
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                {
                    //Set the database table name
                    sqlBulkCopy.DestinationTableName = "dbo.RatingsModels";
                    con.Open();
                    sqlBulkCopy.WriteToServer(dt);
                    con.Close();
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Kino()
        {
            ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = userManager.FindByName(User.Identity.Name);
            return View();
        }
    }
}
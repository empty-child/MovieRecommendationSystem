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
        CommonModel db = new CommonModel();

        public ActionResult Index()
        {
            var rating = db.Ratings.Include(p => p.Movies);
            return View(rating.ToList());
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
            dt.Columns.AddRange(new DataColumn[4] { new DataColumn("UserID", typeof(int)),
            new DataColumn("MovieID", typeof(int)),
            new DataColumn("Rating",typeof(float)),
            new DataColumn("Timestamp",typeof(long))});


            string csvData = Server.MapPath("~/App_Data/Upload/" + fileName);

            // You can also read from a file
            Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(csvData);

            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters(",");

            string[] fields;
            bool colNames = true;
            while (!parser.EndOfData)
            {
                fields = parser.ReadFields();
                if (!colNames)
                {
                    dt.Rows.Add();
                    int i = 0;
                    foreach (string field in fields)
                    {
                        dt.Rows[dt.Rows.Count - 1][i] = field.Replace('.', ',');
                        i++;
                    }
                }
                else colNames = false;
            }

            parser.Close();

            string consString = ConfigurationManager.ConnectionStrings["CommonModel"].ConnectionString;
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
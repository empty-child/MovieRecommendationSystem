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
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Kursach.Controllers
{
    public class HomeController : Controller
    {
        CommonContext db = new CommonContext();

        public ActionResult Index(int? id)
        {
            var movies = db.Movies;
            string userID = User.Identity.GetUserId();
            List<MoviesModel> model;
            bool[] isViewed;
            if (id == null || id == 0)
            {
                ViewBag.previousPageStat = false;
                model = movies.Take(20).ToList();
                isViewed = new bool[model.Count()];
                int i = 0;
                foreach (var movie in model)
                {
                    if (movie.InnerUsers.Where(r => r.LocalID == userID).Count() == 0)
                    {
                        isViewed[i] = false;
                    }
                    else isViewed[i] = true;
                    i++;
                }
                if (model.Count() < 20)
                {
                    ViewBag.nextPageStat = false;
                }
                else
                {
                    ViewBag.nextPageStat = true;
                    ViewBag.nextPage = 1;
                }
            }
            else
            {
                ViewBag.previousPageStat = true;
                ViewBag.previousPage = id - 1;
                
                model = movies.OrderBy(p => p.ID).Skip(0 + 20 * (int)id).Take(20).ToList();
                isViewed = new bool[model.Count()];
                int i = 0;
                foreach(var movie in model)
                {
                    if (movie.InnerUsers.Where(r => r.LocalID == userID).Count() == 0)
                    {
                        isViewed[i] = false;
                    }
                    else isViewed[i] = true;
                    i++;
                }
                if (model.Count() < 20)
                {
                    ViewBag.nextPageStat = false;
                }
                else
                {
                    ViewBag.nextPageStat = true;
                    ViewBag.nextPage = id + 1;
                }
            }
            ViewBag.model = model;
            ViewBag.isViewed = isViewed;
            ViewBag.count = model.Count();
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Index(InnerUser data)
        {
            if (data.Rating > 5.0 || data.Rating <= 0) return new HttpStatusCodeResult(400);
            string userID = User.Identity.GetUserId();
            var movies = db.Movies.Where(b => b.MovieID == data.MovieID).FirstOrDefault();
            data.Movies = movies;
            data.LocalID = userID;
            db.InnerUsers.Add(data);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Search(string movie)
        {
            if (movie == null) return RedirectToAction("Index");
            ViewBag.isFound = true;
            var result = db.Movies.Where(x=>x.Title.Contains(movie));
            ViewBag.count = result.Count();
            if (result.Count() == 0)
            {
                ViewBag.isFound = false;
                return View();
            }
            return View(result);
        }

        public ActionResult Movie(int id)
        {
            var movies = db.Movies.Where(r => r.MovieID == id).First();
            string userID = User.Identity.GetUserId();
            bool isViewed;
            ViewBag.movieID = movies.MovieID;
            if (movies.InnerUsers.Where(r => r.LocalID == userID).Count() == 0)
            {
                isViewed = false;
            }
            else isViewed = true;
            ViewBag.isViewed = isViewed;

            var movie = db.Links.Where(b => b.MovieID == id).First();
            string response = GetReq(@"https://api.themoviedb.org/3/movie/"+movie.TmdbID+"?api_key=###&language=ru-RU");
            if (response == null) return new HttpStatusCodeResult(502);
            MovieRecord record = JsonConvert.DeserializeObject<MovieRecord>(response);
            return View(record);
        }

        [Authorize]
        public ActionResult Recomendation()
        {
            var relatedCoefficient = findRelatedCoefficient();
            var recommendation = getRecommendations(relatedCoefficient);
            var movies = db.Movies;
            int i = 0;
            string[] movieTitle = new string[recommendation.Count()];
            int[] movieID = new int[recommendation.Count()];
            string[] progressBarType = new string[recommendation.Count()];
            string[] recommendationCoefficient = new string[recommendation.Count()];

            foreach (var dict in recommendation)
            {
                var recommendedMovie = movies.Where(p => p.MovieID == dict.Key).First();
                movieTitle[i] = recommendedMovie.Title;
                movieID[i] = recommendedMovie.MovieID;
                recommendationCoefficient[i] = Math.Round(dict.Value * 100, 0).ToString() + "%";
                if(Math.Round(dict.Value * 100, 0) <= 15)
                {
                    progressBarType[i] = "progress-bar-danger";
                }
                else if(Math.Round(dict.Value * 100, 0) <= 50)
                {
                    progressBarType[i] = "progress-bar-warning";
                }
                else
                {
                    progressBarType[i] = "progress-bar-success";
                }
                i++;
            }
            ViewBag.movieTitle = movieTitle;
            ViewBag.recommendationCoefficient = recommendationCoefficient;
            ViewBag.movieID = movieID;
            ViewBag.progressBarType = progressBarType;
            return View();
        }

        private Dictionary<int,float> findRelatedCoefficient()
        {
            string userID = User.Identity.GetUserId();

            Dictionary<int, float> relatedRating = new Dictionary<int, float>();
            Dictionary<int, int> inclusion = new Dictionary<int, int>();

            var innerRatings = db.InnerUsers.Where(n => n.LocalID == userID);
            foreach(var score in innerRatings.Include(p => p.Movies.Ratings))
            {
                foreach(var userRating in score.Movies.Ratings)
                {
                    if (relatedRating.ContainsKey(userRating.UserID))
                    {
                        var sum = relatedRating[userRating.UserID];
                        var incl = inclusion[userRating.UserID];
                        relatedRating.Remove(userRating.UserID);
                        inclusion.Remove(userRating.UserID);
                        relatedRating.Add(userRating.UserID, (float)(Math.Pow((score.Rating - userRating.Rating), 2)) + sum);
                        inclusion.Add(userRating.UserID, incl + 1);
                    }
                    else
                    {
                        inclusion.Add(userRating.UserID, 1);
                        relatedRating.Add(userRating.UserID, (float)(Math.Pow((score.Rating - userRating.Rating), 2)));
                    }
                }
            }

            var topUser = inclusion.OrderByDescending(pair => pair.Value).First();
            Dictionary<int, float> relatedCoefficient = new Dictionary<int, float>();
            var keys = new List<int>(relatedRating.Keys);
            foreach (int key in keys)
            {
                if (inclusion[key] == topUser.Value)
                {
                    float value = relatedRating[key];
                    relatedRating.Remove(key);
                    relatedCoefficient.Add(key, (1 / (1 + value)));
                }
            }
            return relatedCoefficient;
        }

        private IEnumerable<KeyValuePair<int,float>> getRecommendations(Dictionary<int, float> relatedCoefficient)
        {
            string localID = User.Identity.GetUserId();
            Dictionary<int, string> viewedMovies = new Dictionary<int, string>();
            var innerRatings = db.InnerUsers.Where(p => p.LocalID == localID);
            foreach (var record in innerRatings)
            {
                viewedMovies.Add(record.MovieID, record.LocalID);
            }

            var ratingBase = db.Ratings;
            var keys = new List<int>(relatedCoefficient.Keys);
            Dictionary<int, float> recommendationBase = new Dictionary<int, float>();
            Dictionary<int, int> recommendationInclusion = new Dictionary<int, int>();
            
            foreach (int key in keys)
            {
                foreach(var score in ratingBase.Where(p => p.UserID == key))
                {
                    if (recommendationBase.ContainsKey(score.MovieID))
                    {
                        if (!viewedMovies.ContainsKey(score.MovieID))
                        {
                            var baseValue = recommendationBase[score.MovieID];
                            var inclusionValue = recommendationInclusion[score.MovieID];
                            recommendationBase.Remove(score.MovieID);
                            recommendationInclusion.Remove(score.MovieID);
                            recommendationBase.Add(score.MovieID, (score.Rating * relatedCoefficient[key]) + baseValue);
                            recommendationInclusion.Add(score.MovieID, inclusionValue + 1);
                        }
                    }
                    else
                    {
                        if (!viewedMovies.ContainsKey(score.MovieID))
                        {
                            recommendationBase.Add(score.MovieID, score.Rating * relatedCoefficient[key]);
                            recommendationInclusion.Add(score.MovieID, 1);
                        }
                    }
                }
            }

            var topInclusion = recommendationInclusion.OrderByDescending(pair => pair.Value).First().Value;

            var baseKeys = new List<int>(recommendationInclusion.Keys);
            foreach(int key in baseKeys)
            {
                if (recommendationInclusion[key] > topInclusion / 3)
                {
                    float baseValue = recommendationBase[key];
                    recommendationBase.Remove(key);
                    recommendationBase.Add(key, baseValue / recommendationInclusion[key]);
                }
                else
                {
                    recommendationBase.Remove(key);
                }
            }

            var topMovie = recommendationBase.OrderByDescending(pair => pair.Value).Take(10); //.ToDictionary(pair => pair.Key, pair => pair.Value)
            return topMovie;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult Import()
        {
            return View();
        }

        //[Authorize(Roles = "admin")]
        //[HttpPost]
        //public ActionResult Import(IEnumerable<HttpPostedFileBase> uploads)
        //{
        //    List<string> fileNames = new List<string>();
        //    foreach (var file in uploads)
        //    {
        //        if (file != null)
        //        {
        //            // получаем имя файла
        //            fileNames.Add(Path.GetFileName(file.FileName));
        //            // сохраняем файл в папку Files в проекте
        //            file.SaveAs(Server.MapPath("~/App_Data/Upload/" + Path.GetFileName(file.FileName)));
        //        }
        //    }

        //    //string fileName = null;
        //    //if (upload != null)
        //    //{
        //    //    // получаем имя файла
        //    //    fileName = Path.GetFileName(upload.FileName);
        //    //    // сохраняем файл в папку Files в проекте
        //    //    upload.SaveAs(Server.MapPath("~/App_Data/Upload/" + fileName));
        //    //}


        //    DataTable dt = new DataTable();

        //    /* moviesmodel
        //    dt.Columns.AddRange(new DataColumn[4] {new DataColumn("ID", typeof(int)),
        //    new DataColumn("MovieID", typeof(int)),
        //    new DataColumn("Title", typeof(string)),
        //    new DataColumn("Genres",typeof(string))});
        //    */

        //    /* linksmodels
        //    dt.Columns.AddRange(new DataColumn[4] {new DataColumn("LinksModelsID", typeof(int)),
        //    new DataColumn("MovieID", typeof(int)),
        //    new DataColumn("ImdbID", typeof(string)),
        //    new DataColumn("TmdbID",typeof(string))});
        //    */


        //    dt.Columns.AddRange(new DataColumn[5] {new DataColumn("RatingsModelsID", typeof(int)),
        //    new DataColumn("UserID", typeof(int)),
        //    new DataColumn("MovieID", typeof(int)),
        //    new DataColumn("Rating",typeof(float)),
        //    new DataColumn("Timestamp",typeof(long))});



        //    string csvData = Server.MapPath("~/App_Data/Upload/" + fileName);

        //    // You can also read from a file
        //    Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(csvData);

        //    parser.HasFieldsEnclosedInQuotes = true;
        //    parser.SetDelimiters(",");

        //    int it = 1;
        //    string[] fields;
        //    bool colNames = true;
        //    while (!parser.EndOfData)
        //    {
        //        fields = parser.ReadFields();
        //        if (!colNames)
        //        {
        //            dt.Rows.Add();
        //            int i = 0;
        //            dt.Rows[dt.Rows.Count - 1][i] = it;
        //            i++;
        //            foreach (string field in fields)
        //            {
        //                dt.Rows[dt.Rows.Count - 1][i] = field.Replace('.', ',');
        //                i++;
        //            }
        //            it++;
        //        }
        //        else colNames = false;
        //    }

        //    parser.Close();

        //    string consString = ConfigurationManager.ConnectionStrings["CommonContext"].ConnectionString;
        //    using (SqlConnection con = new SqlConnection(consString))
        //    {
        //        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
        //        {
        //            //Set the database table name
        //            sqlBulkCopy.DestinationTableName = "dbo.RatingsModels";
        //            con.Open();
        //            sqlBulkCopy.WriteToServer(dt);
        //            con.Close();
        //        }
        //    }

        //    return RedirectToAction("Index");
        //}

        private static string GetReq(string site, bool addHeader = false, string headerData = "", string requestMethod = "GET")
        {
            string req = site;
            WebRequest request = WebRequest.Create(req);
            if (addHeader)
            {
                request.Headers.Add(headerData);
            }
            request.Method = requestMethod;
            try
            {
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                return responseFromServer;
            }
            catch (WebException e)
            {
                return null;
            }
        }
    }
}
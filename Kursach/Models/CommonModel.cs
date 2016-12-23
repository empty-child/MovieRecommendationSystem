using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kursach.Models
{
    public class MoviesModel
    {
        [Key]
        public int MovieID { get; set; }
        public string Title { get; set; }
        public string Genres { get; set; }

        public virtual ICollection<LinksModel> Links { get; set; }
        public virtual ICollection<RatingsModel> Ratings { get; set; }
        public virtual ICollection<InnerUser> InnerUsers { get; set; }

        public MoviesModel()
        {
            Links = new List<LinksModel>();
            Ratings = new List<RatingsModel>();
            InnerUsers = new List<InnerUser>();
        }
    }

    public class LinksModel
    {
        [Key]
        public int MovieID { get; set; }
        public string ImdbID { get; set; }
        public string TmdbID { get; set; }

        public virtual MoviesModel Movies { get; set; }
    }

    public class RatingsModel
    {
        [Key]
        public int UserID { get; set; }
        public int MovieID { get; set; }
        public float Rating { get; set; }
        public long Timestamp { get; set; }

        public virtual MoviesModel Movies { get; set; }
    }

    public class InnerUser
    {
        [Key]
        public string InnerUserID { get; set; }
        public int MovieID { get; set; }
        public float Rating { get; set; }

        public virtual MoviesModel Movies { get; set; }
    }
}
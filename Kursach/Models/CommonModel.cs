using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Kursach.Models
{
    public class MoviesModel
    {
        [Key]
        public int ID { get; set; }
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
        public int LinksModelID { get; set; }

        [ForeignKey("Movies")]
        public int MovieID { get; set; }

        public string ImdbID { get; set; }
        public string TmdbID { get; set; }

        public virtual MoviesModel Movies { get; set; }
    }

    public class RatingsModel
    {
        public int RatingsModelID { get; set; }
        public int UserID { get; set; }

        [ForeignKey("Movies")]
        public int MovieID { get; set; }

        public float Rating { get; set; }
        public long Timestamp { get; set; }

        public virtual MoviesModel Movies { get; set; }
    }

    public class InnerUser
    {
        public int InnerUserID { get; set; }
        public string LocalID { get; set; }
        [Range(0.0, 5.0, ErrorMessage = "Enter a value between 1 and 1000")]
        public float Rating { get; set; }

        [ForeignKey("Movies")]
        public int MovieID { get; set; }

        public virtual MoviesModel Movies { get; set; }
    }
}
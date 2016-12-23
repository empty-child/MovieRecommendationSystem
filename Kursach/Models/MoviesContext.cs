using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Kursach.Models
{
    public class CommonContext : DbContext
    {
        public CommonContext() : base("CommonContext") {}

        public DbSet<MoviesModel> Movies { get; set; }
        public DbSet<LinksModel> Links { get; set; }
        public DbSet<RatingsModel> Ratings { get; set; }
        public DbSet<InnerUser> InnerUsers { get; set; }
    }
}
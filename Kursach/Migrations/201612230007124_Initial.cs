namespace Kursach.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InnerUsers",
                c => new
                    {
                        InnerUserID = c.Int(nullable: false, identity: true),
                        MovieID = c.Int(nullable: false),
                        Rating = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.InnerUserID);
            
            CreateTable(
                "dbo.MoviesModels",
                c => new
                    {
                        MovieID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Genres = c.String(),
                    })
                .PrimaryKey(t => t.MovieID);
            
            CreateTable(
                "dbo.LinksModels",
                c => new
                    {
                        MovieID = c.Int(nullable: false, identity: true),
                        ImdbID = c.String(),
                        TmdbID = c.String(),
                    })
                .PrimaryKey(t => t.MovieID);
            
            CreateTable(
                "dbo.RatingsModels",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        MovieID = c.Int(nullable: false),
                        Rating = c.Single(nullable: false),
                        Timestamp = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.UserID);
            
            CreateTable(
                "dbo.MoviesModelInnerUsers",
                c => new
                    {
                        MoviesModel_MovieID = c.Int(nullable: false),
                        InnerUser_InnerUserID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.MoviesModel_MovieID, t.InnerUser_InnerUserID })
                .ForeignKey("dbo.MoviesModels", t => t.MoviesModel_MovieID, cascadeDelete: true)
                .ForeignKey("dbo.InnerUsers", t => t.InnerUser_InnerUserID, cascadeDelete: true)
                .Index(t => t.MoviesModel_MovieID)
                .Index(t => t.InnerUser_InnerUserID);
            
            CreateTable(
                "dbo.LinksModelMoviesModels",
                c => new
                    {
                        LinksModel_MovieID = c.Int(nullable: false),
                        MoviesModel_MovieID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LinksModel_MovieID, t.MoviesModel_MovieID })
                .ForeignKey("dbo.LinksModels", t => t.LinksModel_MovieID, cascadeDelete: true)
                .ForeignKey("dbo.MoviesModels", t => t.MoviesModel_MovieID, cascadeDelete: true)
                .Index(t => t.LinksModel_MovieID)
                .Index(t => t.MoviesModel_MovieID);
            
            CreateTable(
                "dbo.RatingsModelMoviesModels",
                c => new
                    {
                        RatingsModel_UserID = c.Int(nullable: false),
                        MoviesModel_MovieID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.RatingsModel_UserID, t.MoviesModel_MovieID })
                .ForeignKey("dbo.RatingsModels", t => t.RatingsModel_UserID, cascadeDelete: true)
                .ForeignKey("dbo.MoviesModels", t => t.MoviesModel_MovieID, cascadeDelete: true)
                .Index(t => t.RatingsModel_UserID)
                .Index(t => t.MoviesModel_MovieID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RatingsModelMoviesModels", "MoviesModel_MovieID", "dbo.MoviesModels");
            DropForeignKey("dbo.RatingsModelMoviesModels", "RatingsModel_UserID", "dbo.RatingsModels");
            DropForeignKey("dbo.LinksModelMoviesModels", "MoviesModel_MovieID", "dbo.MoviesModels");
            DropForeignKey("dbo.LinksModelMoviesModels", "LinksModel_MovieID", "dbo.LinksModels");
            DropForeignKey("dbo.MoviesModelInnerUsers", "InnerUser_InnerUserID", "dbo.InnerUsers");
            DropForeignKey("dbo.MoviesModelInnerUsers", "MoviesModel_MovieID", "dbo.MoviesModels");
            DropIndex("dbo.RatingsModelMoviesModels", new[] { "MoviesModel_MovieID" });
            DropIndex("dbo.RatingsModelMoviesModels", new[] { "RatingsModel_UserID" });
            DropIndex("dbo.LinksModelMoviesModels", new[] { "MoviesModel_MovieID" });
            DropIndex("dbo.LinksModelMoviesModels", new[] { "LinksModel_MovieID" });
            DropIndex("dbo.MoviesModelInnerUsers", new[] { "InnerUser_InnerUserID" });
            DropIndex("dbo.MoviesModelInnerUsers", new[] { "MoviesModel_MovieID" });
            DropTable("dbo.RatingsModelMoviesModels");
            DropTable("dbo.LinksModelMoviesModels");
            DropTable("dbo.MoviesModelInnerUsers");
            DropTable("dbo.RatingsModels");
            DropTable("dbo.LinksModels");
            DropTable("dbo.MoviesModels");
            DropTable("dbo.InnerUsers");
        }
    }
}

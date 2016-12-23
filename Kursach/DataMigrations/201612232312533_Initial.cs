namespace Kursach.DataMigrations
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
                        LocalID = c.String(),
                        Rating = c.Single(nullable: false),
                        MovieID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.InnerUserID)
                .ForeignKey("dbo.MoviesModels", t => t.MovieID, cascadeDelete: true)
                .Index(t => t.MovieID);
            
            CreateTable(
                "dbo.MoviesModels",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MovieID = c.Int(nullable: false),
                        Title = c.String(),
                        Genres = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.LinksModels",
                c => new
                    {
                        LinksModelID = c.Int(nullable: false, identity: true),
                        MovieID = c.Int(nullable: false),
                        ImdbID = c.String(),
                        TmdbID = c.String(),
                    })
                .PrimaryKey(t => t.LinksModelID)
                .ForeignKey("dbo.MoviesModels", t => t.MovieID, cascadeDelete: true)
                .Index(t => t.MovieID);
            
            CreateTable(
                "dbo.RatingsModels",
                c => new
                    {
                        RatingsModelID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        MovieID = c.Int(nullable: false),
                        Rating = c.Single(nullable: false),
                        Timestamp = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.RatingsModelID)
                .ForeignKey("dbo.MoviesModels", t => t.MovieID, cascadeDelete: true)
                .Index(t => t.MovieID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InnerUsers", "MovieID", "dbo.MoviesModels");
            DropForeignKey("dbo.RatingsModels", "MovieID", "dbo.MoviesModels");
            DropForeignKey("dbo.LinksModels", "MovieID", "dbo.MoviesModels");
            DropIndex("dbo.RatingsModels", new[] { "MovieID" });
            DropIndex("dbo.LinksModels", new[] { "MovieID" });
            DropIndex("dbo.InnerUsers", new[] { "MovieID" });
            DropTable("dbo.RatingsModels");
            DropTable("dbo.LinksModels");
            DropTable("dbo.MoviesModels");
            DropTable("dbo.InnerUsers");
        }
    }
}

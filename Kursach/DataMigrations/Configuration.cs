namespace Kursach.DataMigrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class DataConfig : DbMigrationsConfiguration<Kursach.Models.CommonContext>
    {
        public DataConfig()
        {
            AutomaticMigrationsEnabled = true;
            MigrationsDirectory = @"DataMigrations";
        }

        protected override void Seed(Kursach.Models.CommonContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}

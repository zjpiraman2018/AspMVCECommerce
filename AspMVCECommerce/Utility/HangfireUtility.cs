using AspMVCECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspMVCECommerce.Utility
{
    public  static class HangfireUtility
    {
        public static void ClearDatabase(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlCommand("TRUNCATE TABLE[HangFire].[AggregatedCounter]");
            context.Database.ExecuteSqlCommand("TRUNCATE TABLE[HangFire].[Counter]");
            context.Database.ExecuteSqlCommand("TRUNCATE TABLE[HangFire].[JobParameter]");
            context.Database.ExecuteSqlCommand("TRUNCATE TABLE[HangFire].[JobQueue]");
            context.Database.ExecuteSqlCommand("TRUNCATE TABLE[HangFire].[List]");
            context.Database.ExecuteSqlCommand("TRUNCATE TABLE[HangFire].[State]");
            context.Database.ExecuteSqlCommand("DELETE FROM[HangFire].[Job]");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT('[HangFire].[Job]', reseed, 0)");
            context.Database.ExecuteSqlCommand("UPDATE[HangFire].[Hash] SET Value = 1 WHERE Field = 'LastJobId'");
        }
    }
}
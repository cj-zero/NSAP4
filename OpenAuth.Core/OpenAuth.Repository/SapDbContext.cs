using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    [ConnectionString("SapDbContext", DbType = "SqlServer")]
    public class SapDbContext : DbContext
    {
        public SapDbContext(DbContextOptions<SapDbContext> options) : base(options)
        {
        }

    }
}

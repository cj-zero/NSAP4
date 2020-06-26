using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain.Serve;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    [ConnectionString("Nsap4ServeDbContext")]
    public class Nsap4ServeDbContext : DbContext
    {
        public Nsap4ServeDbContext(DbContextOptions<Nsap4ServeDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Solution> Solutions { get; set; }
    }
}

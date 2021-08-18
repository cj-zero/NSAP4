using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{

	[ConnectionString("NsapOaDbContext", DbType = "MySql")]
	public  class NsapOaDbContext : DbContext {
		public NsapOaDbContext(DbContextOptions<NsapOaDbContext> options) : base(options) {
		}
		//public virtual DbSet<file_main> file_main { get; set; }
	}
}

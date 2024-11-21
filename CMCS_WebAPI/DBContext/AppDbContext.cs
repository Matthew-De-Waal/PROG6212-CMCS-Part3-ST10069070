using Microsoft.EntityFrameworkCore;
using CMCS_WebAPI.Models;

namespace CMCS_WebAPI.DBContext
{
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Master constructor
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// AccountRecovery table
        /// </summary>
        public DbSet<AccountRecovery> AccountRecovery { get; set; }

        /// <summary>
        /// Document table
        /// </summary>
        public DbSet<Document> Document { get; set; }

        /// <summary>
        /// Lecturer table
        /// </summary>
        public DbSet<Lecturer> Lecturer { get; set; }

        /// <summary>
        /// Manager table
        /// </summary>
        public DbSet<Manager> Manager { get; set; }

        /// <summary>
        /// Request table
        /// </summary>
        public DbSet<Request> Request { get; set; }

        /// <summary>
        /// RequestDocument table
        /// </summary>
        public DbSet<RequestDocument> RequestDocument { get; set; }

        /// <summary>
        /// RequestProcess table
        /// </summary>
        public DbSet<RequestProcess> RequestProcess { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;

namespace SampleRabbitmq_RPC.Repository.Model
{
	public class AcademyDbContext : DbContext
	{
		public DbSet<Category> Categories { get; set; }
		public DbSet<Course> Courses { get; set; }

		public AcademyDbContext(DbContextOptions<AcademyDbContext> options) : base(options)
		{

		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseInMemoryDatabase("sample_rabbitmq_rpc");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Category>().HasData(
				new Category { Id = 1, Title = "برنامه نویسی دات نت" },
				new Category { Id = 2, Title = "Asp.Net Core" }
			);

			modelBuilder.Entity<Course>()
				.HasOne(c => c.Category)
				.WithMany(g => g.Courses)
				.OnDelete(DeleteBehavior.Cascade);

		}
	}
}

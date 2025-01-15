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

			modelBuilder.Entity<Course>().HasData(
				new Course
				{
					Id = 1,
					Title = "آموزش برنامه نویسی سی شارپ مقدمانی",
					Price = 1_400_000,
					CategoryId = 1
				},
				new Course
				{
					Id = 2,
					Title = "آموزش جامع الگوهای طراحی در سی شارپ",
					Price = 1_900_000,
					CategoryId = 1
				},
				new Course
				{
					Id = 3,
					Title = "دوره آموزشی Asp.Net Core 8",
					Price = 5_400_000,
					CategoryId = 2
				},
				new Course
				{
					Id = 4,
					Title = "آموزش برنامه نویسی سی شارپ پیشرفته",
					Price = 2_400_000,
					CategoryId = 1
				});

			modelBuilder.Entity<Course>()
				.HasOne(c => c.Category)
				.WithMany(g => g.Courses)
				.OnDelete(DeleteBehavior.Cascade);

		}
	}
}

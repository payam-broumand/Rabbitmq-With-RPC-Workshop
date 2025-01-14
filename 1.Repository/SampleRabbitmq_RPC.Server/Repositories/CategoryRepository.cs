using SampleRabbitmq_RPC.Repository.Contracts;
using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.Repository.Repositories
{
	public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
	{
		public CategoryRepository(AcademyDbContext dbContext) : base(dbContext)
		{
		}
	}
}

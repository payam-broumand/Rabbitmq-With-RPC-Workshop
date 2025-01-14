using SampleRabbitmq_RPC.Repository.Contracts;
using SampleRabbitmq_RPC.Repository.Repositories;

namespace SampleRabbitmq_RPC.Repository.Model
{
	public class CategoryRepository :
		BaseRepository<Category>,
		ICategoryRepository
	{  
		public CategoryRepository(AcademyDbContext dbContext) : base(dbContext)
		{
		}
	}
}

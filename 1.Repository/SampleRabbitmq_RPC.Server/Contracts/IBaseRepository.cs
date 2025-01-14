using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.Repository.Contracts
{
	public interface IBaseRepository<TEntity> where TEntity : BaseEntity
	{
		TEntity? GetById(int id);

		IReadOnlyList<TEntity> GetAll();

		TEntity? Edit(int id, TEntity model);

		void Delete(int id);
	}
}

using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.Common.BaseContract
{
	/// <summary>
	/// this interface define methods which is specfied for clients
	/// and clients with these methods can perform crud operations
	/// </summary>
	/// <typeparam name="TEntity">refers to that entity we want to person CRUD operation on it</typeparam>
	public interface IRabbitmqClientCommand<TEntity> : IRabbitmqCommand<TEntity>
		where TEntity : BaseEntity
	{
		Task<IReadOnlyList<TEntity>?> GetAllEntitesAsync();

		Task<TEntity?> GetEntityByIdAsync();

		Task<TEntity?> CreateEntityAsync(TEntity entity);

		Task<TEntity?> UpdateEntityAsync();
		
		Task<TEntity?> DeleteEntityAsync();
	}
}

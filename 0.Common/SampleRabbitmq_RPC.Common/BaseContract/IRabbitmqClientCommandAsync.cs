using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.Common.BaseContract
{
	public interface IRabbitmqClientCommandAsync<TEntity> : IRabbitmqCommand<TEntity>
		where TEntity : BaseEntity
	{
		Task GetAllEntnitesAsync(Action<IReadOnlyList<TEntity>> actionEntityList);

		Task GetEntityByIdAsync(Action<TEntity> actionEntity);
		
		Task CreateEntityAsync(Action<TEntity> actionEntity);

		Task UpdateEntityAsync(Action<TEntity> actionUpdatedEntity);

		Task DeleteEntityAsync(Action<TEntity?> actionDeletedEntity);

		CommandConfig CommandConfig { get; } 
	}
}

using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.Common.BaseContract
{
	/// <summary>
	/// this interface generate methods which is specfied for server
	/// and server with these methods can perform any more required operations
	/// </summary>
	/// <typeparam name="TEntity">refers to that entity we want to person CRUD operation on it</typeparam>
	public interface IRabbitmqSenderCommand<TEntity> : IRabbitmqCommand<TEntity> where TEntity : BaseEntity
	{

	}
}

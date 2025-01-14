using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Model; 

namespace SampleRabbitmq_RPC.Common.BaseContract
{
	/// <summary>
	/// this interface produce commonly methods for comsume and publish methods
	/// </summary>
	/// <typeparam name="TEntity">refers to that entity we want to person CRUD operation on it</typeparam>
	public interface IRabbitmqCommand<TEntity> where TEntity : BaseEntity
	{
		Task InitializeConfig(string routingkey, string replyTo, RabbitCommandType commandType);
		Task ConsumerCommandAsync();
		Task PublishCommandAsync();
		Dictionary<string, string> Command { get; set; }
	}
}

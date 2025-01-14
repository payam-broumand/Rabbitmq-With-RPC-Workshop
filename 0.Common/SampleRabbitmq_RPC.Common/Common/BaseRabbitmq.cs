using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.RabbitCommnads;
using SampleRabbitmq_RPC.Repository.Contracts;
using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.Common.Common
{
	public abstract class BaseRabbitmq<TEntity> where TEntity : BaseEntity
	{ 
		public abstract IRabbitmqCommand<TEntity> SetRabbitmqCommand(string command);
	} 

	public class RabbitmqFactory<TEntity> : BaseRabbitmq<TEntity> where TEntity : BaseEntity
	{
		// default constructor for clients
		public RabbitmqFactory() { }

		// second constructor signature to assigning repostory for sender
		public RabbitmqFactory(IBaseRepository<TEntity> repository)
		{
			RabbitmqCommandServer<TEntity>.Repository = repository;
		}

		// using factory method to create appropriate rabbitmq client/sender command class 
		public override IRabbitmqCommand<TEntity> SetRabbitmqCommand(string command)
		{
			IRabbitmqCommand<TEntity> rabbitmqCommand =
				command switch
				{
					"sender" => RabbitmqCommandServer<TEntity>.Server,
					"client" => RabbitmqCommandClient<TEntity>.Client,
					_ => RabbitmqCommandClient<TEntity>.Client
				};

			return rabbitmqCommand;
		}
	}
}

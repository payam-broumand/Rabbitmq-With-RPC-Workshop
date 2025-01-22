using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.RabbitCommnads;
using SampleRabbitmq_RPC.Repository.Contracts;
using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.Common.Common
{
	public abstract class BaseRabbitmq<TEntity> where TEntity : BaseEntity
	{
		public abstract IRabbitmqCommand<TEntity> SetRabbitmqCommand(string command);

		public abstract Task<IRabbitmqCommand<TEntity>?> InitializeRabbitConfigurationAsync(
			string routingKey,
			string replyTo,
			RabbitCommandType commandType);
	}

	public class RabbitmqFactory<TEntity> : BaseRabbitmq<TEntity> where TEntity : BaseEntity
	{
		private static RabbitmqFactory<TEntity> _rabbitmqFactory;

		public static IBaseRepository<TEntity> _repository;

		private RabbitmqFactory() { } 

		private RabbitmqFactory(IBaseRepository<TEntity> repository)
		{
			RabbitmqCommandServer<TEntity>.CommandServerRepository = repository;
		}

		// create parameterless instance for clients
		public static RabbitmqFactory<TEntity> DefaultRabbitmqFactory
			=> _rabbitmqFactory ??= new RabbitmqFactory<TEntity>();

		// create constructor with repository for server
		public static RabbitmqFactory<TEntity> RabbitmqFactoryWithRepository
			=> _rabbitmqFactory ??= new RabbitmqFactory<TEntity>(_repository);

		/// <summary>
		/// initialize base rabbitmq configuration
		/// </summary>
		/// <param name="routingKey">the queue routing key which bind that to the exchange</param>
		/// <param name="replyTo">identify reply-to for receiving response from </param>
		/// <param name="commandType"></param>
		/// <returns></returns>
		public override async Task<IRabbitmqCommand<TEntity>?> InitializeRabbitConfigurationAsync(
			string routingKey,
			string replyTo,
			RabbitCommandType commandType)
		{
			// create new client instance for sending request to server with rabbit  
			IRabbitmqCommand<TEntity> command = SetRabbitmqCommand(commandType.ToString());

			if (command is null)
			{
				Console.WriteLine("RabbitMq clien/sernder not working please try later ...");
				return null;
			}

			// initialize base rabbitmq configuration like connection and channel
			await command.InitializeConfig(routingKey, replyTo, commandType);

			// invoke client/server consumer 
			await command.ConsumerCommandAsync();

			return command;
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

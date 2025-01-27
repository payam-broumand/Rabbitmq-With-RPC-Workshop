using Newtonsoft.Json;
using RabbitMQ.Client;
using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.Common.RabbitCommnads
{
	/// <summary>
	/// Create Rabbiemq client class in Asyncronously state
	/// In this class we using Action delegate methods to display 
	/// processed data after after the required time has passed
	/// to preparing output data
	/// </summary>
	/// <typeparam name="TEntity">Referes to the entity data model</typeparam>
	public class RabbitmqCommandClientAsync<TEntity> :
		RabbitmqConfiguration,
		IRabbitmqClientCommandAsync<TEntity>
		where TEntity : BaseEntity
	{
		/*
			this members using for async calls
			we using Action method for async calls
		*/
		private Action<TEntity?> _actionEntity;
		private Action<IReadOnlyList<TEntity>?> _actionEntityList;

		private CrudCommand _crudCommand;

		// using static members to create new singleton method instance
		private static RabbitmqCommandClientAsync<TEntity> _commandClient;

		public static RabbitmqCommandClientAsync<TEntity> CommandClient
			=> _commandClient ??= new RabbitmqCommandClientAsync<TEntity>();

		private RabbitmqCommandClientAsync() { }

		// getting singleton command config instance
		public CommandConfig CommandConfig => CommandConfig.GetCommandConfig;

		/*
			Consume clinet requests by sended command type in async mode
			in this method we using the Action methods to consume responses
		*/
		public async Task ConsumerCommandAsync()
		{
			_consumer.ReceivedAsync += (sender, e) =>
			{
				byte[]? responseBytes = null;
				responseBytes = e.Body.ToArray();

				// access correlation for defining whose command response must be consuming
				var basicProps = e.BasicProperties;
				var correlationId = basicProps.CorrelationId;

				Guid commandGuid = Guid.Parse(correlationId);
				CrudCommand crudCommand = CommandConfig[commandGuid];

				switch (crudCommand)
				{
					case CrudCommand.getall:
						IReadOnlyList<TEntity>? entites = new List<TEntity>();

						if (responseBytes is not null)
						{
							string allEntitesResponseString = System.Text.Encoding.UTF8.GetString(responseBytes);
							entites = JsonConvert.DeserializeObject<IReadOnlyList<TEntity>>(allEntitesResponseString);
						}

						if (entites is not null)
						{
							_actionEntityList(entites);
						}

						break;

					case CrudCommand.getbyid: 
					case CrudCommand.create:
					case CrudCommand.update:
					case CrudCommand.delete:
						ConsumeResponse(responseBytes);
						break; 

					default:
						break;
				}

				CommandConfig.Remove(commandGuid);
				return Task.CompletedTask;
			};

			await _channel.BasicConsumeAsync(_queueName, true, _consumer);
		}

		/// <summary>
		/// publish client commands to the server
		/// we serialize the client command that is the Dictionary and
		/// publish the command as byte array to the sever like the synv method
		/// 
		/// for complete description refer to the sync version method
		/// </summary>
		/// <returns></returns>
		public async Task PublishCommandAsync()
		{
			// identify properties (replyto / correlation_id)
			var props = new BasicProperties();
			props.ReplyTo = _replyTo;


			/*
				- getting command type from client request command
				- convert client command to crud command enum
				- set client command to commands list
				- return added client command index and send it with request
					as correlation id to the server
			 */
			string commandType = !string.IsNullOrWhiteSpace(_command["command"])
				? _command["command"]
				: CrudCommand.getall.ToString();
			_crudCommand = (CrudCommand)Enum.Parse(typeof(CrudCommand), commandType);

			/*
				adding new client command and return command key for
				using as correlation id, beacause when we using async method
				the every task in tasks list take diffenret time to complete 
				and theredore one task maybe will be completed erliear than previous tasks
				
				and on the other hand when we use asynchronous method we can send
				multiple request commands from client to the sever and as a result
				this causes while the previous task is processing to cpmlete
				new command sending from client and change the previous command 
				when the prior jos is complete, in the client side consumer method 
				comes with different command name and cause throw an exception or 
				return anormal result to the client

				therefore in this situation we need to set and maintain command type as key
				to every command and send it to the server as correlation id while result published
				to the client the Client can specify which one of the commands was sended
				to sever and also specify command type by receiving command key as correlation id
			 */
			Guid commandKey = CommandConfig.Add(_crudCommand);
			props.CorrelationId = commandKey.ToString();

			switch (_crudCommand)
			{
				case CrudCommand.getall:
					Console.WriteLine("Wait for receiving data from server ...");
					break;

				case CrudCommand.getbyid:
					Console.WriteLine("Wait for receiving entity by id ...");
					break;

				case CrudCommand.create:
					Console.WriteLine("Waiting for create new netity response ...");
					break;

				case CrudCommand.update:
					Console.WriteLine("Wating for updating entity response ...");
					break;

				case CrudCommand.delete:
					Console.WriteLine("Wating for deleting entity response ...");
					break;

				default:
					break;
			}

			string commandsList = JsonConvert.SerializeObject(CommandConfig.CommandsList);
			if (_command.ContainsKey("command_list"))
				_command["command_list"] = commandsList;
			else
			{
				_command.Add("command_list", commandsList);
			}

			// serialize and mke byte array from client commnd and send it to server
			byte[] commandBytes = System.Text.Encoding.UTF8.GetBytes(
				JsonConvert.SerializeObject(_command));

			/*
				when publish a message in client side we must set routing key correctly
				the routing key refers to the whose server that prepare the client request response
				and server with that's routing key binded to the exchange
			 */
			await _channel.BasicPublishAsync(exchangeName, _routingKey, mandatory: true, basicProperties: props, commandBytes);
		}

		private void ConsumeResponse(byte[]? responseBytes)
		{
			TEntity? entity = null;
			
			if (responseBytes is null || responseBytes.Length == 0)
			{
				_actionEntity(entity);
				return;
			}

			string entityString = System.Text.Encoding.UTF8.GetString(responseBytes);
			if (!string.IsNullOrEmpty(entityString))
			{
				entity = JsonConvert.DeserializeObject<TEntity>(entityString);
			}

			_actionEntity(entity);
		}

		/// <summary>
		/// Get all entites by Action delegate method argument
		/// in this method we assigning Action method with argument
		/// when the results is ready the Action method invoke
		/// </summary>
		/// <param name="actionEntityList"></param>
		/// <returns></returns>
		public async Task GetAllEntnitesAsync(Action<IReadOnlyList<TEntity>?> actionEntityList)
		{
			_actionEntityList = actionEntityList;

			await PublishCommandAsync();
		}

		public async Task GetEntityByIdAsync(Action<TEntity?> actionEntity)
		{
			_actionEntity = actionEntity;

			await PublishCommandAsync();
		}

		public async Task CreateEntityAsync(Action<TEntity?> actionCreatedEntity)
		{
			_actionEntity = actionCreatedEntity;

			await PublishCommandAsync();
		}

		public async Task UpdateEntityAsync(Action<TEntity?> actionUpdatedEntity)
		{
			_actionEntity = actionUpdatedEntity;

			await PublishCommandAsync();
		}

		public async Task DeleteEntityAsync(Action<TEntity?> actionDeletedEntity)
		{
			_actionEntity = actionDeletedEntity;

			await PublishCommandAsync();
		}
	}
}

using RabbitMQ.Client;
using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Model;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace SampleRabbitmq_RPC.Common.RabbitCommnads
{
	/// <summary>
	/// rabbitmq client that send(publish) clients requests
	/// process that commands by server and receive(consume) 
	/// response thst sended by server
	/// 
	/// this class implement base interface IRabbitCommand that produce
	/// consume and publish methods
	/// 
	/// inherit from IRabbitmqClientCommand to implement own methods in client side
	/// </summary>
	/// <typeparam name="TEntity">refers to whose entnity we apply crud operation on it</typeparam>
	public class RabbitmqCommandClient<TEntity> :
		RabbitmqConfiguration,
		IRabbitmqClientCommand<TEntity>
		where TEntity : BaseEntity
	{
		private BlockingCollection<TEntity> _entites = [];

		private TEntity? _entity { get; set; }

		private List<TEntity> _entityList = new List<TEntity>();

		private RabbitmqCommandClient()
		{

		}

		private static RabbitmqCommandClient<TEntity> _client;

		public static RabbitmqCommandClient<TEntity> Client
			=> _client ??= new RabbitmqCommandClient<TEntity>();

		public async Task<IReadOnlyList<TEntity>?> GetAllEntitesAsync()
		{
			await PublishCommandAsync();

			return _entityList;
		}

		public async Task<TEntity?> GetEntityByIdAsync()
		{
			await PublishCommandAsync();

			return _entity;
		}

		public async Task<TEntity?> UpdateEntityAsync()
		{
			await PublishCommandAsync();

			return _entity;
		}

		public async Task<TEntity?> DeleteEntityAsync()
		{
			await PublishCommandAsync();

			return _entity;
		}

		public async Task<TEntity?> CreateEntityAsync(TEntity entity)
		{
			await PublishCommandAsync();

			return _entity;
		}

		/// <summary>
		/// consume (receive) the responses has been sent from server
		/// we are using switch-case to specify which of the CURD comments is running
		/// and finaly we consume thw response
		/// </summary>
		/// <returns></returns>
		public async Task ConsumerCommandAsync()
		{
			_consumer.ReceivedAsync += (sender, e) =>
			{
				byte[]? responseBytes = null;
				responseBytes = e.Body.ToArray();

				string commandType = !string.IsNullOrWhiteSpace(_command["command"])
					? _command["command"]
					: string.Empty;

				switch (commandType)
				{
					case "getall":
						IReadOnlyList<TEntity>? entites = new List<TEntity>();

						if (responseBytes is not null)
						{
							string responseString = System.Text.Encoding.UTF8.GetString(responseBytes);
							entites = JsonConvert.DeserializeObject<IReadOnlyList<TEntity>>(responseString);
						}

						if (entites is not null)
						{
							foreach (var item in entites)
							{
								_entites.TryAdd(item);
							}
							_entites.CompleteAdding();
						}
						break;

					case "getbyid":
					case "create":
					case "update":
					case "delete":
						ConsumeEntityTypeResponse(responseBytes);
						break;

					default:
						break;
				}

				return Task.CompletedTask;
			};

			await _channel.BasicConsumeAsync(_queueName, true, _consumer);
		}

		private void ConsumeEntityTypeResponse(byte[]? responseBytes)
		{
			TEntity? entity = null;
			string entityString = System.Text.Encoding.UTF8.GetString(responseBytes);
			if (!string.IsNullOrEmpty(entityString))
			{
				entity = JsonConvert.DeserializeObject<TEntity>(entityString);
			}

			if (entity is not null)
			{
				_entites.Add(entity);
			}
			else
			{
				Type entityType = typeof(TEntity);
				object? entityInstace = Activator.CreateInstance(entityType);
				_entites.Add((TEntity)entityInstace);
			}
		}

		/// <summary>
		/// clients can sending themself requests with this method to the server
		/// server consume client comments and send appopreiate reposnse to that client was sended the request

		/// in this method we assigning _entites variable to maintain response result
		/// this variable is a BlockingCollection for concurent environments and
		/// when web using the take methpd in this state waiting for initializing blocking collection and remove the value exist in collection and return that value

		/// for take the collections values from blocking collection we using the GetConsumingEnumerable method ans then using it in spread operator instead using for-loop for assigning collection values
		/// </summary>
		/// <returns></returns>
		public async Task PublishCommandAsync()
		{
			/*
				create new BlockCollection instace after every adding items to 
				BlockingCollection beacase when adding collections of items to
				BlockingCollection after that we must incoke CompleteAdding method
				to return callback and showing collection items
				When you call CompleteAdding on a BlockingCollection, 
				it signifies that no more items will be added. 
				
				If you attempt to take an item from it after this and the collection 
				is empty, you'll get this error.
			 */
			_entites = new BlockingCollection<TEntity>();

			var props = new BasicProperties();
			props.ReplyTo = _replyTo;

			// serialize and mke byte array from client commnd and send it to server
			byte[] commandBytes = System.Text.Encoding.UTF8.GetBytes(
				JsonConvert.SerializeObject(_command));

			string commandType = !string.IsNullOrWhiteSpace(_command["command"])
				? _command["command"]
				: "getall";

			switch (commandType)
			{
				case "getbyid":
					props.CorrelationId = _command["data"];
					break;

				case "getall":
					Console.WriteLine("Wait for receiving data ...");
					break;

				case "update":
					props.CorrelationId = _command["data"];
					Console.WriteLine("update command sended to server");
					break;

				case "delete":
					props.CorrelationId = _command["data"];
					Console.WriteLine("delete command sended to server");
					break;

				case "create":
					Console.WriteLine("adding new entity command received");
					break;

				default:
					break;
			}

			await _channel.BasicPublishAsync(exchangeName, _routingKey, mandatory: true, basicProperties: props, commandBytes);

			switch (commandType)
			{
				case "getall":
					_entityList = [.. _entites.GetConsumingEnumerable()];
					break;

				case "getbyid":
				case "create":
				case "update":
				case "delete":
					_entity = _entites.Take();
					break;

				default:
					break;
			}

		}
	}
}

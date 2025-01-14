using RabbitMQ.Client;
using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Model;
using System.Collections.Concurrent; 
using System.Text.Json;

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

		//public static async Task InitializeRabbitComponentAsync(
		//	string routingKey,
		//	string replyTo,
		//	Dictionary<string, string> command)
		//{
		//	Thread.Sleep(8000);

		//	await InitializeConfig(routingKey, replyTo, command); 
		//}

		public async Task<IReadOnlyList<TEntity>?> GetAllEntitesAsync()
		{
			await PublishCommandAsync();

			return _entityList;
		}

		public async Task<TEntity?> GetEntityById(int? id)
		{
			if (id is null or <= 0) return null;

			await PublishCommandAsync();

			return _entity;
		}

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
					case "getbyid": 
						TEntity? entity = null;
						string entityString = System.Text.Encoding.UTF8.GetString(responseBytes);
						if (!string.IsNullOrEmpty(entityString))
						{
							entity = JsonSerializer.Deserialize<TEntity>(entityString);
						}

						if (entity is not null)
						{
							_entites.Add(entity);
						}
						else
						{
							Console.WriteLine("Entity not found ...");
							Type entityType = typeof(TEntity);
							object? entityInstace = Activator.CreateInstance(entityType);
							_entites.Add((TEntity)entityInstace);
						} 
						break;

					case "getall":
						IReadOnlyList<TEntity>? entites = new List<TEntity>();

						if (responseBytes is not null)
						{
							entites = JsonSerializer.Deserialize<IReadOnlyList<TEntity>>(System.Text.Encoding.UTF8.GetString(responseBytes));
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

					default:
						break;
				}

				return Task.CompletedTask;
			};

			await _channel.BasicConsumeAsync(_queueName, true, _consumer);
		}

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
				JsonSerializer.Serialize(_command));

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

				default:
					break;
			}

			await _channel.BasicPublishAsync(exchangeName, _routingKey, mandatory: true, basicProperties: props, commandBytes);

			switch (commandType)
			{
				case "getbyid":
					_entity = _entites.Take();
					break;

				case "getall":
					_entityList = [.. _entites.GetConsumingEnumerable()];
					break;

				default:
					break;
			}

		}
	}
}

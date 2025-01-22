using RabbitMQ.Client;
using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Contracts;
using SampleRabbitmq_RPC.Repository.Model;
using Newtonsoft.Json;

namespace SampleRabbitmq_RPC.Common.RabbitCommnads
{
	/// <summary>
	/// rabbitmq server that receive(consume) clients commands
	/// process that commands and sending(publish) response to the clients 
	/// this class implement base interface IRabbitCommand that produce
	/// consume and publish methods
	/// 
	/// inherit from IRabbitmqSenderCommand to implement own methods in server side
	/// </summary>
	/// <typeparam name="TEntity">refers to whose entnity we apply crud operation on it</typeparam>
	public class RabbitmqCommandServer<TEntity> :
		RabbitmqConfiguration,
		IRabbitmqSenderCommand<TEntity>
		where TEntity : BaseEntity
	{

		public TEntity? Entity { get; private set; }

		private static IBaseRepository<TEntity>? _repository;

		private static RabbitmqCommandServer<TEntity> _server;

		public static IBaseRepository<TEntity>? CommandServerRepository
		{
			get => _repository;
			set => _repository ??= value;
		}

		public AcademyDbContext DbContext { get; set; }

		public static RabbitmqCommandServer<TEntity> Server
			=> _server ??= new RabbitmqCommandServer<TEntity>();

		private RabbitmqCommandServer()
		{
		}

		/// <summary>
		/// consume responses sended from server
		/// we using switch case to identify client comment
		/// for implement crud operation
		/// </summary>
		/// <returns></returns>
		public async Task ConsumerCommandAsync()
		{
			_consumer.ReceivedAsync += async (sender, e) =>
			{
				byte[]? resposeBytes = null;
				resposeBytes = e.Body.ToArray();
				var requestProps = e.BasicProperties;
				_replyTo = requestProps.ReplyTo;

				string commandDictioanry = System.Text.Encoding.UTF8.GetString(resposeBytes);
				_command = JsonConvert.DeserializeObject<Dictionary<string, string>>(commandDictioanry) ?? new Dictionary<string, string>();

				string commandType = !string.IsNullOrWhiteSpace(_command["command"])
					? _command["command"]
					: "getall";

				switch (commandType)
				{
					case "getbyid":
						_correlationId = requestProps.CorrelationId;
						Console.WriteLine($"Entity with id: {_correlationId} received ...");
						await PublishCommandAsync();

						break;

					case "getall":
						Console.WriteLine("Get all entites command received ...");
						await PublishCommandAsync();
						break;

					case "create": 
						Console.WriteLine("Create new entity command received ...");
						await PublishCommandAsync();
						break;

					case "update":
						_correlationId = requestProps.CorrelationId; 
						Console.WriteLine("Update comand received ...");
						await PublishCommandAsync();
						break;

					case "delete":
						_correlationId = requestProps.CorrelationId; 
						Console.WriteLine("Delete comand received ...");
						await PublishCommandAsync();
						break; 

					default:
						break;
				}
			};

			await _channel.BasicConsumeAsync(_queueName, true, _consumer);
		}

		/// <summary>
		/// public processed clients request responses
		/// in this methpd we assigning correlation-id and reply-to
		/// for specifiing whose client response must be send to it
		/// </summary>
		/// <returns></returns>
		public async Task PublishCommandAsync()
		{
			byte[]? responseBytes = null;
			var responseProps = new BasicProperties();
			responseProps.ReplyTo = _replyTo;

			string commandType = !string.IsNullOrWhiteSpace(_command["command"])
					? _command["command"]
					: "getall";

			switch (commandType)
			{
				case "getbyid":
					responseProps.CorrelationId = _correlationId;
					FindEntityByCorrelationId(out int id, out TEntity? entity);

					if (entity is not null)
					{
						responseBytes = System.Text.Encoding.UTF8.GetBytes(
							JsonConvert.SerializeObject(entity));
						Console.WriteLine($"Entity has been sent\n");
					}
					else
					{
						Console.WriteLine("Entity not found ...");
					}
					break;

				case "getall":
					IReadOnlyList<TEntity> entities = _repository.GetAll();
					responseBytes = System.Text.Encoding.UTF8.GetBytes(
						JsonConvert.SerializeObject(entities));
					Console.WriteLine("Entity list has been sent");
					break;

				case "create":
					TEntity? entityToAdd = null;

					// check if create new entity command is existt and has valid data
					if (_command.ContainsKey("entity_to_add") && !string.IsNullOrEmpty(_command["entity_to_add"]))
					{
						TEntity? newEntity = JsonConvert.DeserializeObject<TEntity>(_command["entity_to_add"]);
						if (newEntity is null)
						{
							Console.WriteLine("New entity received is null");
							break;
						}

						entityToAdd = _repository.Create(newEntity);
					}

					// check created new entity not null and repository create method has been run successfully
					if (entityToAdd is not null)
					{
						string entityString = JsonConvert.SerializeObject(entityToAdd);
						responseBytes = System.Text.Encoding.UTF8.GetBytes(entityString);
					}
					else
					{
						Console.WriteLine("error in adding new entity");
						break;
					}
					break;

				case "update":
					responseProps.CorrelationId = _correlationId;
					FindEntityByCorrelationId(out id, out entity);
					if (entity is null)
					{
						Console.WriteLine("entity not found to update ...");
						break;
					}

					TEntity? updatedEntity = null;
					if (_command.ContainsKey("edited_entity") && !string.IsNullOrEmpty(_command["edited_entity"]))
					{
						TEntity? editedEntity = JsonConvert.DeserializeObject<TEntity>(_command["edited_entity"]);

						if (editedEntity is null)
						{
							Console.WriteLine("Edited Entity is null");
						}
						else
						{
							updatedEntity = _repository.Edit(id, editedEntity);
						}
					}

					if (updatedEntity is not null)
					{
						responseBytes = System.Text.Encoding.UTF8.GetBytes(
							JsonConvert.SerializeObject(updatedEntity));
						Console.WriteLine("entity has been updated successfully ...\n");
					}
					else
					{
						Console.WriteLine("error in entity updating\n");
					}
					break;

				case "delete":
					responseProps.CorrelationId = _correlationId;

					// calling repository delete method
					int entityId = int.TryParse(_correlationId, out id) ? id : 0;
					TEntity? deletedEntity = _repository.Delete(entityId); 

					// check repository delete method has been running successfully
					if (deletedEntity is not null)
					{
						responseBytes = System.Text.Encoding.UTF8.GetBytes(
							JsonConvert.SerializeObject(deletedEntity));
						Console.WriteLine("entity has been deleted successfully ...\n");
					}
					else
					{
						Console.WriteLine("error in deleting entity\n");
					}
					break;

				default:
					break;
			}

			// publish response to that whose client that binded with repl-to routing key
			await _channel.BasicPublishAsync(exchangeName, _replyTo, mandatory: true,
				 basicProperties: responseProps, responseBytes);
		}

		private void FindEntityByCorrelationId(out int entityId, out TEntity? entity)
		{
			if (string.IsNullOrEmpty(_correlationId))
			{
				entity = null;
				entityId = 0;
				return;
			}

			entityId = int.TryParse(_correlationId, out int id) ? id : 0;
			if (entityId <= 0)
			{
				entity = null;
				entityId = 0;
				return;
			}

			entity = _repository.GetById(entityId);
		}
	}
}

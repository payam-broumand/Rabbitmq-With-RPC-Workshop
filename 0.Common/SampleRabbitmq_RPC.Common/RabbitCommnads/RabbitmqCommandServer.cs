using RabbitMQ.Client;
using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Contracts;
using SampleRabbitmq_RPC.Repository.Model;
using System.Text.Json;

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

		private static IBaseRepository<TEntity> _repository; 

		private static RabbitmqCommandServer<TEntity> _server;

		public static IBaseRepository<TEntity> Repository
		{
			get => _repository;
			set => _repository ??= value;
		}

		public static RabbitmqCommandServer<TEntity> Server
			=> _server ??= new RabbitmqCommandServer<TEntity>(Repository); 

		private RabbitmqCommandServer(IBaseRepository<TEntity> repository)
		{
			_repository = repository;
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

				string commandDictioanry = System.Text.Encoding.UTF8.GetString(resposeBytes);
				_command = JsonSerializer.Deserialize<Dictionary<string, string>>(commandDictioanry) ?? new Dictionary<string, string>();

				string commandType = !string.IsNullOrWhiteSpace(_command["command"])
					? _command["command"]
					: "getall";

				switch (commandType)
				{
					case "getbyid":
						_correlationId = requestProps.CorrelationId;
						_replyTo = requestProps.ReplyTo;
						Console.WriteLine($"Entity with id: {_correlationId} received ...");
						await PublishCommandAsync();

						break;

					case "getall":
						_replyTo = requestProps.ReplyTo;
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
					int entityId = int.TryParse(_command["data"], out int id) ? id : 0;
					TEntity? entity = _repository.GetById(entityId);

					if (entity is not null)
					{
						responseBytes = System.Text.Encoding.UTF8.GetBytes(
							JsonSerializer.Serialize(entity));
					}
					Console.WriteLine($"Entity by id: {entityId} has been sent\n");
					break;

				case "getall":
					IReadOnlyList<TEntity> entities = _repository.GetAll();
					responseBytes = System.Text.Encoding.UTF8.GetBytes(
						JsonSerializer.Serialize(entities));
					Console.WriteLine("Entity list has been sent");
					break;

				default:
					break;
			}

			// publish response to that whose client that binded with repl-to routing key
			await _channel.BasicPublishAsync(exchangeName, _replyTo, mandatory: true,
				 basicProperties: responseProps, responseBytes); 
		} 
	}
}

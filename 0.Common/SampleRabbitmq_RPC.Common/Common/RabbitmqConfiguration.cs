using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SampleRabbitmq_RPC.Common.Common
{
	/// <summary>
	/// setting common rabbitmq configuration in server / client in base abstract class
	/// </summary>
	public abstract class RabbitmqConfiguration
	{
		private ConnectionFactory _connectionFactory;
		private IConnection _connection;
		protected IChannel _channel;
		protected readonly string exchangeName = "SampleRabbitmqRPC";
		protected AsyncEventingBasicConsumer _consumer;
		protected string _queueName = string.Empty;
		protected string _routingKey = string.Empty;
		protected string _replyTo = string.Empty;
		protected string _correlationId = string.Empty;

		protected Dictionary<string, string> _command = new();

		public Dictionary<string, string> Command
		{
			get => _command;
			set => _command = value ??= new Dictionary<string, string>()
			{
				{ "command", "getall" }
			};
		}

		/// <summary>
		/// implement common rabbitmq configuration in abstract base class
		/// using command type to identify what value setting for queue binding to exchange
		/// in client we using reply-to value for routing key beacause in client we
		/// define server response target by reply-to and reply-to is the client routing key
		/// </summary>
		/// <param name="routingkey">identify routing key that binding queue to exchange</param>
		/// <param name="replyTo">identify whose wueue that reponse by server send to it</param>
		/// <param name="commandType">identify server/client for defining what setting value for routing key in queue binding</param>
		/// <returns></returns>
		public virtual async Task InitializeConfig(
			string routingkey,
			string replyTo,
			RabbitCommandType commandType)
		{
			// set Rabbitmq base configuration
			_connectionFactory = new ConnectionFactory()
			{
				HostName = "localhost"
			};
			_connection = await _connectionFactory.CreateConnectionAsync();
			_channel = await _connection.CreateChannelAsync();
			_routingKey = routingkey;
			_replyTo = replyTo;

			// Exchange Declare with Topic exchange type
			await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic);

			// Declare Queue and getting queue name after declaring the queue
			// Rabbitmq set accident name to queue and we can return it by QueueName property
			_queueName = _channel.QueueDeclareAsync().Result.QueueName;
			string routingKeyName = commandType == RabbitCommandType.sender ? routingkey : replyTo;
			await _channel.QueueBindAsync(_queueName, exchangeName, routingKeyName);

			// set consumer member value with new EventBasicConsume instance
			// and call received event separately in client and server to
			// consume received messages from server or client
			_consumer = new AsyncEventingBasicConsumer(_channel);
		}
	}

	public enum RabbitCommandType : byte
	{
		sender,
		client,
		clientasync
	}

	public enum CrudCommand : byte
	{
		getall,
		getbyid,
		create,
		update,
		delete
	}
}

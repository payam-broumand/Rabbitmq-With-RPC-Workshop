using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Model;
using Newtonsoft.Json;

namespace SampleRabbitmq_RPC.Common.RabbitCommnads
{
	/// <summary>
	/// create new client service class for handling logic and send
	/// requests to the client and receive response from client
	/// the real client apps (console app | web | mobile) consuming 
	/// client service responses
	/// </summary>
	/// <typeparam name="TEntity">Refers to target entity model</typeparam>
	public class ClientCrudService<TEntity> where TEntity : BaseEntity
	{
		private IRabbitmqClientCommand<TEntity>? _client;

		private static ClientCrudService<TEntity>? _crudComment;

		public static ClientCrudService<TEntity> CrudComment
			=> _crudComment ??= new ClientCrudService<TEntity>();

		private ClientCrudService()
		{
			_crudComment = null;
		}

		public async Task InitializeRabbitConfiguration(string routingKey, string replyTo)
		{
			BaseRabbitmq<TEntity> baseRabbitmq = RabbitmqFactory<TEntity>.DefaultRabbitmqFactory;
			_client ??= (IRabbitmqClientCommand<TEntity>?)await baseRabbitmq.InitializeRabbitConfigurationAsync(
							routingKey,
							replyTo,
							RabbitCommandType.client);
		}

		public async Task<IReadOnlyList<TEntity>?> GetEntitesList()
		{
			// with dictionary we define command type (CRUD) and any more required data
			// set client command (CRUD commands)
			_client.Command = new Dictionary<string, string>
			{
				{ "command", "getall" }
			};

			/*
				after create client and initialize rabbit config
				we sending first request with rabbit to server with synchronous method
			 */
			IReadOnlyList<TEntity>? entites = await _client.GetAllEntitesAsync();

			return entites;
		}

		public async Task<TEntity?> FindEntityByIdAsync(int? id)
		{
			if (id is null or <= 0) return null;

			// with dictionary we define command type (CRUD) and any more required data 
			// set client command (CRUD commands)
			_client.Command = new Dictionary<string, string>
			{
				{ "command", "getbyid" },
				{ "data", id?.ToString() ?? "0" }
			};

			/*
				after create client and initialize rabbit config
				we sending first request with rabbit to server with synchronous method
			 */
			TEntity? entity = await _client.GetEntityByIdAsync();

			return entity;
		}

		public async Task<TEntity?> UpdateEntnityAsync(int? id, TEntity editedEntity)
		{
			if (id is null or <= 0) return null;

			_client.Command = new Dictionary<string, string>
			{
				{ "command", "update" },
				{ "data", id?.ToString() ?? "0" },
				{ "edited_entity", JsonConvert.SerializeObject(editedEntity) }
			};

			TEntity? entity = await _client.UpdateEntityAsync();

			return entity;
		}

		public async Task<TEntity?> DeleteEntityAsync(int? id)
		{
			if (id is null or <= 0) return null;

			_client.Command = new Dictionary<string, string>
			{
				{ "command", "delete" },
				{ "data", id?.ToString() ?? "0" }
			};

			TEntity? entity = await _client.DeleteEntityAsync();

			return entity;
		}

		public async Task<TEntity?> CreateEntityAsync(TEntity entity)
		{
			// set create command
			_client.Command = new Dictionary<string, string>
			{
				{ "command", "create" },
				{ "entity_to_add", JsonConvert.SerializeObject(entity) }
			};

			TEntity? createdEntity = await _client.CreateEntityAsync(entity);

			return createdEntity;
		}
	} 
}

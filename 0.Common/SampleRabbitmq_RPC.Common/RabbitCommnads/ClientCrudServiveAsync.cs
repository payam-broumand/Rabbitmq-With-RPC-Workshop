using Newtonsoft.Json;
using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Model;
using System.Data;

namespace SampleRabbitmq_RPC.Common.RabbitCommnads
{
	public class ClientCrudServiveAsync<TEntity> where TEntity : BaseEntity
	{
		private IRabbitmqClientCommandAsync<TEntity>? _client;

		private static ClientCrudServiveAsync<TEntity> _clientService;

		public static ClientCrudServiveAsync<TEntity> ClientService
			=> _clientService ??= new ClientCrudServiveAsync<TEntity>();

		private ClientCrudServiveAsync() { } 

		public async Task InitializeRabbitmqConfiguration(string routingKey, string replyTo)
		{
			BaseRabbitmq<TEntity> baseRabbitmq = RabbitmqFactory<TEntity>.DefaultRabbitmqFactory;
			_client ??= (IRabbitmqClientCommandAsync<TEntity>?)await baseRabbitmq.InitializeRabbitConfigurationAsync(
				routingKey, 
				replyTo, 
				RabbitCommandType.clientasync);
		}

		public async Task GetEntityListAsync(Action<IReadOnlyList<TEntity>?> actionEntityList)
		{
			// with dictionary we define command type (CRUD) and any more required data
			// set client command (CRUD commands)
			_client.Command = new Dictionary<string, string>
			{
				{ "command", CrudCommand.getall.ToString() }
			};

			/*
				after create client and initialize rabbit config
				we sending first request with rabbit to server with Asynchronous method
				we using Action delegate method to sending target method that hold
				and display output results 
			 */
			await _client.GetAllEntnitesAsync(actionEntityList);
		}

		public async Task FindEntityByIdAsync(int? id, Action<TEntity?> actionEntity)
		{
			if (id is null or <= 0)
			{
				Console.WriteLine("Entity not found ...");
				return;
			}

			// with dictionary we define command type (CRUD) and any more required data 
			// set client command (CRUD commands)
			_client.Command = new Dictionary<string, string>
			{
				{ "command", CrudCommand.getbyid.ToString() },
				{ "data", id?.ToString() ?? "0" }
			};

			await _client.GetEntityByIdAsync(actionEntity);
		}

		public async Task CreateEntityAsync(TEntity newEntity, Action<TEntity?> actionEntity)
		{
			// with dictionary we define command type (CRUD) and any more required data 
			// set client command (CRUD commands)
			_client.Command = new Dictionary<string, string>
			{
				{ "command", CrudCommand.create.ToString() },
				{ "entity_to_add", JsonConvert.SerializeObject(newEntity) }
			};

			await _client.CreateEntityAsync(actionEntity);
		}

		public async Task UpdateEntityAsync(int? id, Category updatedEntity, Action<TEntity?> actionEntity)
		{
			if(id is null or <= 0)
			{
				Console.WriteLine("Entity id to update is null");
				return;
			} 

			_client.Command = new Dictionary<string, string>
			{
				{ "command", CrudCommand.update.ToString() },
				{ "data", id?.ToString() ?? "0" },
				{ "edited_entity", JsonConvert.SerializeObject(updatedEntity) }
			};

			await _client.UpdateEntityAsync(actionEntity);
		}

		public async Task DeleteEntityAsync(int? id, Action<TEntity?> actionEntity)
		{
			if (id is null or <= 0)
			{
				Console.WriteLine("Entity id to delete is null");
				return;
			}

			_client.Command = new Dictionary<string, string>
			{
				{ "command", CrudCommand.delete.ToString() },
				{ "data", id?.ToString() ?? "0" }
			};

			await _client.DeleteEntityAsync(actionEntity);
		}
	}
}

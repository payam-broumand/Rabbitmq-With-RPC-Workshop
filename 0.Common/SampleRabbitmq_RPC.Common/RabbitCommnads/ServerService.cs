using Microsoft.EntityFrameworkCore;
using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Contracts;
using SampleRabbitmq_RPC.Repository.Model;
using SampleRabbitmq_RPC.Repository.Repositories;
using System.Reflection;

namespace SampleRabbitmq_RPC.Common.RabbitCommnads
{
	public class ServerService<TEntity> where TEntity : BaseEntity
	{
		public static string RoutingKey { get; set; }
		public static string ReplyTo { get; set; }
		private static IRabbitmqSenderCommand<Category>? _sender; 

		private static AcademyDbContext _dbContext;

		private static ServerService<TEntity> _serverService; 

		private ServerService()
		{
			_sender = null;

			ConfigDbContext();
		}

		public static ServerService<TEntity> SenderService
			=> _serverService ??= new ServerService<TEntity>(); 

		public IBaseRepository<TEntity>? GetRepositoryType(string repositoryClassName)
		{
			Assembly assembly = Assembly.Load("SampleRabbitmq_RPC.Repository");
			Type[] types = assembly.GetTypes();
			Type? type = types.FirstOrDefault(a => typeof(IBaseRepository<TEntity>).IsAssignableFrom(a) && a.IsClass && !a.IsAbstract && a.Name.ToLower() == $"{repositoryClassName}Repository");

			if (type == null)
			{
				return null;
			}

			object? classRepositoryInstance = Activator.CreateInstance(type, _dbContext);
			return classRepositoryInstance is not null
				? (IBaseRepository<TEntity>)classRepositoryInstance
				: null;
		}

		public async Task InitializeRabbitConfiguration()
		{
			// specify repository instance for base rabbitmq configuration class 
			ICategoryRepository categoryRepository = new CategoryRepository(_dbContext);
			RabbitmqFactory<Category>._repository ??= categoryRepository;
			BaseRabbitmq<Category> baseRabbitmq = RabbitmqFactory<Category>.RabbitmqFactoryWithRepository;

			_sender ??= (IRabbitmqSenderCommand<Category>?)await baseRabbitmq.InitializeRabbitConfigurationAsync(
							RoutingKey,
							ReplyTo,
							RabbitCommandType.sender);

			if (_sender is null)
			{
				Console.WriteLine("sender not working please try later ...");
				return;
			}

			// set DbContext for server that it can use this for identify repository
			_sender.DbContext = _dbContext; 
		}

		private void ConfigDbContext()
		{
			DbContextOptionsBuilder<AcademyDbContext> optionsBuilder = new DbContextOptionsBuilder<AcademyDbContext>();
			optionsBuilder.UseInMemoryDatabase("sample_rabbitmq_rpc");
			_dbContext ??= new AcademyDbContext(optionsBuilder.Options);
		}
	}
}

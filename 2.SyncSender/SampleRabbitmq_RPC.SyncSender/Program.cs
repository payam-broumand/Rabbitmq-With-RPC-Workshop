using SampleRabbitmq_RPC.Common.RabbitCommnads;
using SampleRabbitmq_RPC.Repository.Model;
using SampleRabbitmq_RPC.Repository.Repositories;


/*
	Create new instance of DbContext class with DbContextOption 
 */
//DbContextOptionsBuilder<AcademyDbContext> optionsBuilder = new DbContextOptionsBuilder<AcademyDbContext>();
//optionsBuilder.UseInMemoryDatabase("sample_rabbitmq_rpc");
//using AcademyDbContext _dbContext = new AcademyDbContext(optionsBuilder.Options);

//ICategoryRepository categoryRepository = new CategoryRepository(_dbContext);

/*
	getting rabbitmq command server instace from BaseRabbitmq class 
 */
//BaseRabbitmq<Category> baseRabbitmq = RabbitmqFactory<Category>.RabbitmqFactoryWithRepository(categoryRepository);
//IRabbitmqCommand<Category> rabbitmqCommand = baseRabbitmq.SetRabbitmqCommand("sender");
//IRabbitmqSenderCommand<Category>? rabbitmqSenderCommand = rabbitmqCommand as IRabbitmqSenderCommand<Category>;

//if(rabbitmqSenderCommand is null)
//{
//	Console.WriteLine("sender not working please try later ...");
//	return;
//}

// initialize rabbitmq config
//await rabbitmqSenderCommand.InitializeConfig(routingKey, string.Empty, RabbitCommandType.sender);

// start consuming
//await rabbitmqSenderCommand.ConsumerCommandAsync();

ServerService<Category>.RoutingKey = "rpc.get.category.request";
ServerService<Category> serverService = ServerService<Category>.SenderService;
await serverService.InitializeRabbitConfiguration();


Console.WriteLine("Sender is ready ...");

Console.ReadKey();
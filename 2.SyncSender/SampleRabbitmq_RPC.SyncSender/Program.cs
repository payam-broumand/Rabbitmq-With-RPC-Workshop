using Microsoft.EntityFrameworkCore;
using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common; 
using SampleRabbitmq_RPC.Repository.Contracts;
using SampleRabbitmq_RPC.Repository.Model;

string routingKey = "rpc.get.category.request";

/*
	Create new instance of DbContext class with DbContextOption 
 */
DbContextOptionsBuilder<AcademyDbContext> optionsBuilder = new DbContextOptionsBuilder<AcademyDbContext>();
optionsBuilder.UseInMemoryDatabase("sample_rabbitmq_rpc");
using AcademyDbContext _dbContext = new AcademyDbContext(optionsBuilder.Options);

ICategoryRepository categoryRepository = new CategoryRepository(_dbContext);

/*
	getting rabbitmq command server instace from BaseRabbitmq class 
 */
BaseRabbitmq<Category> baseRabbitmq = new RabbitmqFactory<Category>(categoryRepository);
IRabbitmqCommand<Category> rabbitmqCommand = baseRabbitmq.SetRabbitmqCommand("sender");
IRabbitmqSenderCommand<Category>? rabbitmqSenderCommand = rabbitmqCommand as IRabbitmqSenderCommand<Category>;

if(rabbitmqSenderCommand is null)
{
	Console.WriteLine("sender not working please try later ...");
	return;
}

// initialize rabbitmq config
await rabbitmqSenderCommand.InitializeConfig(routingKey, string.Empty, RabbitCommandType.sender);

// start consuming
await rabbitmqSenderCommand.ConsumerCommandAsync();

Console.WriteLine("Sender is ready ...");

Console.ReadKey();
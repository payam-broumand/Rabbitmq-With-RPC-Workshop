using SampleRabbitmq_RPC.Common.BaseContract;
using SampleRabbitmq_RPC.Common.Common;
using SampleRabbitmq_RPC.Repository.Model;

// waiting while server completely loading
Thread.Sleep(6000);

/*
	you must precisely assigning routing-key and reply-to in client
	the routing key refers to the server routing key 
	the reply-to refers to the client routing key and client with reply-to
	must bind to the exchange so therefore binding queue to the exchange
	in client side we must using reply-to for rtouing key
	
	in the below code when we initialize rabbitmq configuration with 
	InitializeConfig method we need identify server or client 
	because when initialize in client we must using reply-to for
	routing key that bind queue to the exchange
 */
string routingKey = "rpc.get.category.request";
string replyTo = "rpc.get.category.sync.client.request";
bool toBeContinue = false;

// create new client instance for sending request to server with rabbit 
BaseRabbitmq<Category> baseRabbitmq = new RabbitmqFactory<Category>();
IRabbitmqCommand<Category> rabbitmqCommand = baseRabbitmq.SetRabbitmqCommand("client");
IRabbitmqClientCommand<Category>? _client = rabbitmqCommand as IRabbitmqClientCommand<Category>;
if(_client is null)
{
	Console.WriteLine("Client turn it down please try later ...");
	return;
}

// initialize base rabbitmq configuration like connection and channel
await _client.InitializeConfig(routingKey, replyTo, RabbitCommandType.client);

// invoke client consumer
await _client.ConsumerCommandAsync();

Console.WriteLine("client is ready ...");

do
{
	Console.WriteLine("\n".PadRight(44, '-'));
	string menu =
		"""
		1. Category List
		2. Find Category By id
		------------------------------------
		Enter your choice: 
		""";
	Console.Write(menu);
	int selectedItem = int.TryParse(Console.ReadLine(), out int id)
		? id : 0; 

	switch (selectedItem)
	{
		case 1:
			await GetCategorireList(_client);
			break;

		case 2:
			await FindCategoryById(_client);
			break;

		default:
			Console.WriteLine("invalue item number ...\n");
			break;
	} 

	Console.Write("\nDo you want to contine (y/n): ");
	toBeContinue = Console.ReadLine() == "y";
} while (toBeContinue);   


// getting categories list by setting command to getakk 
static async Task GetCategorireList(IRabbitmqClientCommand<Category> client)
{
	// with dictionary we define command type (CRUD) and any more required data
	Dictionary<string, string> command = new Dictionary<string, string>
	{
		{ "command", "getall" }
	};

	// set client command (CRUD commands)
	client.Command = command;

	/*
		after create client and initialize rabbit config
		we sending first request with rabbit to server with synchronous method
	 */
	IReadOnlyList<Category> categories = await client.GetAllEntitesAsync();
	foreach (var item in categories)
	{
		Console.WriteLine($"{item.Id}: {item.Title}");
	}
	Console.WriteLine();
}

// getting category by id by setting command to getbyid
static async Task FindCategoryById(IRabbitmqClientCommand<Category>? client)
{
	Console.Write("\nEnter category id: ");
	int categoryId = int.TryParse(Console.ReadLine(), out int id) ? id : 0;

	// with dictionary we define command type (CRUD) and any more required data
	Dictionary<string, string> command = new Dictionary<string, string>
	{
		{ "command", "getbyid" },
		{ "data", categoryId.ToString() }
	};

	// set client command (CRUD commands)
	client.Command = command;

	/*
		after create client and initialize rabbit config
		we sending first request with rabbit to server with synchronous method
	 */
	Category? category = await client.GetEntityById(categoryId);

	if (category is not null)
	{
		Console.WriteLine($"{category.Id}: {category.Title}");
	}
	else
	{
		Console.WriteLine("category not found");
	}
}
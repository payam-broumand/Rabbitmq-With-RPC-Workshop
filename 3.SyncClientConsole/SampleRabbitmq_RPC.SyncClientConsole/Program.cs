using SampleRabbitmq_RPC.Common.RabbitCommnads;
using SampleRabbitmq_RPC.Repository.Model;

// waiting while server completely loading
Thread.Sleep(4000);

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

ClientCrudService<Category> crudService = ClientCrudService<Category>.CrudComment;
await crudService.InitializeRabbitConfiguration(routingKey, replyTo);
bool toBeContinue = false;
Console.WriteLine("Sync Console Client is ready ...");
do
{
	Console.WriteLine("\n".PadRight(44, '-'));
	string menu =
		"""
		1. Category List
		2. Find Category By id
		3. Update Category By Id
		4. Delete Category By Id
		5. Add New Category
		------------------------------------
		Enter your choice: 
		""";
	Console.Write(menu);
	int selectedItem = int.TryParse(Console.ReadLine(), out int id) ? id : 0;
	switch (selectedItem)
	{
		case 1:
			IReadOnlyList<Category>? categories = await crudService.GetEntitesList();
			if (categories is not null)
			{
				foreach (var item in categories)
				{
					Console.WriteLine($"{item.Id}: {item.Title}");
				}
			}
			else
			{
				Console.WriteLine("not any category found ...");
			}

			break;

		case 2:
			Console.Write("\nEnter category id: ");
			int categoryId = int.TryParse(Console.ReadLine(), out id) ? id : 0;
			Category? category = await crudService.FindEntityByIdAsync(categoryId);

			if (category is not null && category.Id > 0)
			{
				Console.WriteLine($"{category.Id}: {category.Title}");
			}
			else
			{
				Console.WriteLine("category not found");
			}
			break;

		case 3:
			Console.Write("\nEnter category id to update: ");
			categoryId = int.TryParse(Console.ReadLine(), out id) ? id : 0;
			category = await crudService.FindEntityByIdAsync(categoryId);

			if (category is null || category.Id == 0)
			{
				Console.WriteLine("category not found ...");
				break;
			}

			Console.Write("Enter new category title: ");
			string? categoryName = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(categoryName))
			{
				Console.WriteLine("Invalid category title");
				break;
			}

			category.Title = categoryName;
			Category? editedCategory = await crudService.UpdateEntnityAsync(categoryId, category);
			if (editedCategory is not null)
			{
				Console.WriteLine($"Category Updated {editedCategory.Id}: {editedCategory.Title}");
			}
			else
			{
				Console.WriteLine("category update error");
			}
			break;

		case 4:
			Console.Write("\nEnter category id to delete: ");
			categoryId = int.TryParse(Console.ReadLine(), out id) ? id : 0; 

			Category? deletedCategory = await crudService.DeleteEntityAsync(categoryId);
			if (deletedCategory is not null && deletedCategory.Id > 0)
			{
				Console.WriteLine($"Category {deletedCategory.Id}: {deletedCategory.Title} Deleted Successfully");
			}
			else
			{
				Console.WriteLine("category not found to delete");
			}
			break;

		case 5:
			Console.Write("Enter new category title: ");
			string? title = Console.ReadLine();
			if(string.IsNullOrEmpty(title))
			{
				Console.WriteLine("invalid category title");
				break;
			}

			Category newCategory = new Category { Title = title };

			Category? categoryCreated = await crudService.CreateEntityAsync(newCategory);
			if(categoryCreated is not null)
			{
				Console.WriteLine("New Category created successfully");
			}
			else
			{
				Console.WriteLine("error in adding new category");
			}
			break;

		default:
			Console.WriteLine("invalue item number ...\n");
			break;
	}

	Console.Write("\nDo you want to contine (y/n): ");
	toBeContinue = Console.ReadLine() == "y";
} while (toBeContinue); 
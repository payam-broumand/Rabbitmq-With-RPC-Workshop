using SampleRabbitmq_RPC.AsyncClientConsole;
using SampleRabbitmq_RPC.Common.RabbitCommnads;
using SampleRabbitmq_RPC.Repository.Model;

internal class Program
{
	static string routingKey = "rpc.get.category.request";
	static string replyTo = "rpc.get.category.async.client.request";

	static ClientCrudServiveAsync<Category> client = ClientCrudServiveAsync<Category>.ClientService;

	private static async Task Main(string[] args)
	{
		// waiting while server completely loading
		Thread.Sleep(4000);

		Console.WriteLine("Async Console Client is ready ...");

		/*
			You must precisely assiging routing key and reply-to
			routing key referes to the server routing key and
			reply-to refers to the which of the clients routing key
			that with this key binded to the exchange

			when server process data and wanna to send results to the cient
			it send data to which of the clients that routing key is reply-to 
			and therefore server reply the client request with reply-to address
		 */
		await client.InitializeRabbitmqConfiguration(routingKey, replyTo);

		bool toBeContinue = false;
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
					await client.GetEntityListAsync(ActionEntityResult.PrintEntityList);
					break;

				case 2:
					Console.Write("Enter category id: ");
					int categoryId = int.TryParse(Console.ReadLine(), out id) ? id : 0;
					await client.FindEntityByIdAsync(categoryId, ActionEntityResult.PrintEntity);
					break;

				case 3:
					Console.Write("Enter Category id to update: ");
					categoryId = int.TryParse(Console.ReadLine(), out id) ? id : 0;

					await client.FindEntityByIdAsync(categoryId, ActionEntityResult.FindEntityToUpdate);
					break;

				case 4:
					Console.Write("Enter Cateogry id to delete: ");
					categoryId = int.TryParse(Console.ReadLine(), out id) ? id : 0;

					await client.DeleteEntityAsync(categoryId, ActionEntityResult.DeleteEntity);
					break;

				case 5:
					Console.Write("Enter Category Title: ");
					string? title = Console.ReadLine();
					if (string.IsNullOrEmpty(title))
					{
						Console.WriteLine("invalid category title");
						break;
					}

					Category newCategory = new Category { Title = title };
					await client.CreateEntityAsync(newCategory, ActionEntityResult.PrintEntity);
					break;

				default:
					Console.WriteLine("Invalid item number");
					break;
			}

			Console.Write("\nDo You want to continue: ");
			toBeContinue = Console.ReadLine() == "y";
		} while (toBeContinue);
	}

	/// <summary>
	/// After receiving category exist to update
	/// we must getting new category title and send update command 
	/// 
	/// IMPORTANT NOTE: Considering that we sending and receiving requests in
	/// asynchronous mthod therefore we must carefull about inputing data
	/// in console because in the main method getting user input choices 
	/// regulary so we must enter new data for updating entity AFTER
	/// user choiced item for crud operation
	/// 
	/// If you have multiple choices items therefore you must be carful about
	/// order of your inputs
	/// </summary>
	/// <param name="category">Refers to category we want to update it</param>
	/// <returns></returns>
	public static async Task SendUpdateCommand(Category? category)
	{
		Console.Write("\nEnter new title for category: ");
		string? newTitle = Console.ReadLine();
		if (string.IsNullOrEmpty(newTitle))
		{
			Console.WriteLine("invalid category title");
			return;
		}

		category.Title = newTitle;

		await client.UpdateEntityAsync(category.Id, category, ActionEntityResult.UpdateEntity);
	}
}
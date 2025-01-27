using SampleRabbitmq_RPC.Repository.Model;

namespace SampleRabbitmq_RPC.AsyncClientConsole
{
	public class ActionEntityResult
	{ 
		public static void PrintEntityList(IReadOnlyList<Category>? categories)
		{
			Console.WriteLine("\n".PadRight(20, '-') +
							Environment.NewLine +
							"Gettgin Categories List Response");
			foreach (var item in categories)
			{
				Console.WriteLine($"{item.Id}: {item.Title}");
			}
			Console.WriteLine("\n".PadRight(20, '-'));
		}

		public static void PrintEntity(Category? category)
		{
			if (category is not null)
			{
				Console.WriteLine("\n".PadRight(20, '-') +
							Environment.NewLine +
							"Getting category by id request response id:");
				Console.WriteLine($"{category.Id}: {category.Title}");
				Console.WriteLine("\n".PadRight(20, '-'));
			}
			else
			{
				Console.WriteLine("category not found ...");
			} 
		}

		public static void FindEntityToUpdate(Category? category)
		{
			if (category is not null)
			{
				Task.Run(async () => await Program.SendUpdateCommand(category));
			}
			else
			{
				Console.WriteLine("Category not found to update");
			}

		}

		public static void UpdateEntity(Category? category)
		{
			if (category is not null)
			{
				Console.WriteLine("\nCategory has been updated successfully");
			}
			else
			{
				Console.WriteLine("\nAn error occured in category update");
			}
		}

		public static void DeleteEntity(Category? category)
		{
			if (category is not null)
			{
				Console.WriteLine("\nCategory has been deleted successfully");
			}
			else
			{
				Console.WriteLine("\nAn error occured in category delete");
			}
		}
	}
}

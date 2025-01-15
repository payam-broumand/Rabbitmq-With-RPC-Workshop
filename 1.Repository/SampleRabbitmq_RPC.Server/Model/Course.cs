namespace SampleRabbitmq_RPC.Repository.Model
{
	public class Course : BaseEntity
	{
		public string Title { get; set; } = "null"
		public int Price { get; set; }

		public int CategoryId { get; set; }
		public Category? Category { get; set; } = new();
	}
}

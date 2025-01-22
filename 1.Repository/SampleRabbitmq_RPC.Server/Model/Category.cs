namespace SampleRabbitmq_RPC.Repository.Model
{
	public class Category : BaseEntity
	{
		public string Title { get; set; } = "null";

		public IReadOnlyCollection<Course>? Courses { get; set; }
	}
}

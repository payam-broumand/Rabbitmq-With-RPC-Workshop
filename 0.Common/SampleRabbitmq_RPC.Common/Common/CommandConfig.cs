namespace SampleRabbitmq_RPC.Common.Common
{
	/// <summary>
	/// With this class we can store and maintain client request
	/// Every client request consist of command type (refers to the CRUD operation)
	/// and data if it has data for send to the server
	/// 
	/// In this class we using indexer to quick and eday access to the client command
	/// Every indexer item it's generic dictionary with stting keys that refer to the
	/// client request segments like command type or data and values like keys it's string too
	/// so like that
	/// Command<string, string> clientCommand = Dictiony<string, string> 
	/// {
	///		{ "command", "delete" },
	///		{ "data", "id" }
	/// }
	/// </summary>
	public class CommandConfig
	{
		private readonly Dictionary<Guid, CrudCommand> _commands = new Dictionary<Guid, CrudCommand>();

		private static CommandConfig? _commandConfig;

		public static CommandConfig GetCommandConfig => _commandConfig ??= new CommandConfig();

		private CommandConfig() { }

		public CrudCommand this[Guid index] =>
			_commands.ContainsKey(index)
					? _commands[index]
					: CrudCommand.getall;

		public Dictionary<Guid, CrudCommand> CommandsList => _commands;

		public int CountCommands => _commands.Count;

		public Guid Add(CrudCommand command)
		{
			Guid commandKey = Guid.NewGuid();
			_commands.Add(commandKey, command);

			return commandKey;
		}

		public void Remove(Guid key)
		{
			if (_commands.ContainsKey(key))
				_commands.Remove(key);
		}
	}
}

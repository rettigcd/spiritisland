
namespace SpiritIsland;

public class UnitOfWork {

	public Guid Id { get; }

	public UnitOfWork() {
		Id = Guid.NewGuid();
	}

	// String / object dictionary to track action things
	public bool ContainsKey(string key) => dict != null && dict.ContainsKey( key );
	public object this[string key]{
		get => ContainsKey(key) ? dict[key] : throw new InvalidOperationException($"{key} was not set");
		set => (dict??= new())[key] = value;
	}
	Dictionary<string, object> dict;

}


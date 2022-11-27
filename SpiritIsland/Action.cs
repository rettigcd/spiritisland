
namespace SpiritIsland;

/// <summary>
/// A Spirit Island 'Action'
/// </summary>
public sealed class UnitOfWork : IAsyncDisposable 
{

	// spirit (if any) that owns the action. Null for non-spirit actions
	public Spirit Owner { get; set; }

	public Guid Id { get; }

	public UnitOfWork( DualAsyncEvent<UnitOfWork> endOfAction) {
		Id = Guid.NewGuid();
		EndOfAction = endOfAction;
	}

	// String / object dictionary to track action things
	public bool ContainsKey(string key) => dict != null && dict.ContainsKey( key );

	public object this[string key]{
		get => ContainsKey(key) ? dict[key] : throw new InvalidOperationException($"{key} was not set");
		set => (dict??= new())[key] = value;
	}


	public async ValueTask DisposeAsync() {
		await EndOfAction.InvokeAsync(this);
	}

	DualAsyncEvent<UnitOfWork> EndOfAction;

	Dictionary<string, object> dict;
}


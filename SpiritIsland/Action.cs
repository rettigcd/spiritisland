
namespace SpiritIsland;

/// <summary>
/// A Spirit Island 'Action'
/// </summary>
public sealed class UnitOfWork : IAsyncDisposable
//	, IDisposable
{

	#region constructor
	public UnitOfWork( DualAsyncEvent<UnitOfWork> endOfAction, ActionCategory actionCategory, TerrainMapper terrainMapper ) {
		Id = Guid.NewGuid();
		_endOfAction = endOfAction;
		Category = actionCategory;
		TerrainMapper = terrainMapper;
	}
	#endregion

	public readonly ActionCategory Category;
	public readonly TerrainMapper TerrainMapper;

	// spirit (if any) that owns the action. Null for non-spirit actions
	public Spirit Owner { get; set; }

	public Guid Id { get; }

	#region action-scoped data
	// String / object dictionary to track action things
	public bool ContainsKey(string key) => dict != null && dict.ContainsKey( key );

	public object this[string key]{
		get => ContainsKey(key) ? dict[key] : throw new InvalidOperationException($"{key} was not set");
		set => (dict??= new())[key] = value;
	}
	#endregion

	public async ValueTask DisposeAsync() {
		if(_endOfThisAciton != null)
			await _endOfThisAciton.InvokeAsync(this);
		await _endOfAction.InvokeAsync(this);
	}

	//public void Dispose() {
	//	// DANGEROUS - Only use this for Tests.
	//	if(_endOfThisAciton != null)
	//		_endOfThisAciton.InvokeAsync( this ).Wait();
	//	_endOfAction.InvokeAsync( this ).Wait();
	//}

	public void AtEndOfThisAction(Func<UnitOfWork,Task> action ) => (_endOfThisAciton ??= new AsyncEvent<UnitOfWork>()).Add( action );
	public void AtEndOfThisAction( Action<UnitOfWork> action ) => (_endOfThisAciton ??= new AsyncEvent<UnitOfWork>()).Add( action );

	AsyncEvent<UnitOfWork> _endOfThisAciton;

	#region private
	readonly DualAsyncEvent<UnitOfWork> _endOfAction;
	Dictionary<string, object> dict;
	#endregion
}


public enum ActionCategory {

	Default, // nothing

	// Spirit
	Spirit_Growth,
	Spirit_Power,
	Spirit_SpecialRule, // which specified After X, do Y
	Spirit_PresenceTrackIcon,
	//	GainEnery, // specifiec on preence track
	//	PlayingAllPowerCards, // !!! Is this real? It means Finder couldn't switch between cards.
	//	SpiritualRituals,

	Invader,
	//	One Ravage, Build, or Explore in one land

	Blight,
	//	The effects of a Blight Card

	Fear,
	//	Everything one Fear Card does

	Event,
	//	Everything a Main Event Does
	//	Everything a Token Event does
	//	Everything a dahan event does

	Adversary,
	//	An adversary's escalation effects (except englind as it invokes a bild)
	//	Instructions on an adversary to perform some effect.
	//	Actions written on the scenario panel.

	Special
    // Command the Beasts

}
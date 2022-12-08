
namespace SpiritIsland;

/// <summary>
/// A Spirit Island 'Action'
/// </summary>
public sealed class UnitOfWork : IAsyncDisposable {

	#region constructor
	public UnitOfWork( DualAsyncEvent<UnitOfWork> endOfAction, ActionCategory actionCategory ) {
		Id = Guid.NewGuid();
		_endOfAction = endOfAction;
		_actionCategory = actionCategory;
	}
	#endregion

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
		await _endOfAction.InvokeAsync(this);
	}

	#region private
	readonly DualAsyncEvent<UnitOfWork> _endOfAction;
	Dictionary<string, object> dict;
#pragma warning disable IDE0052 // Remove unread private members
	readonly ActionCategory _actionCategory;
#pragma warning restore IDE0052 // Remove unread private members
	#endregion
}


public enum ActionCategory {

	Default, // nothing

	Spirit,
	//	Growth,
	//	GainEnery, // specifiec on preence track
	//	PlayingAllPowerCards, // !!! Is this real? It means Finder couldn't switch between cards.
	//	PresenceTrackIcon,
	//	Power,
	//	SpecialRule, // which specified After X, do Y
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
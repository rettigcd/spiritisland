namespace SpiritIsland;

public interface IFearCard : IOption {

	int? ActivatedTerrorLevel { get; set; }
	bool Flipped { get; set; } // set is for Memento use

	/// <summary>
	/// Pulls this card off of the top of the deck, and puts it on the 'Activated Cards' pile.
	/// </summary>
	/// <remarks>!!! This method should really be on the deck, not on the card.</remarks>
	void Activate( GameState gameState );

	/// <summary> Flips card face up and performs associated action. </summary>
	Task ActAsync( int terrorLevel );
}

public abstract class FearCardBase {
	public int? ActivatedTerrorLevel { get; set; }
	public bool Flipped { 
		get => _flipped;
		set {
			if(_flipped == value) return;
			_flipped = value;
			if(_flipped)
				ActionScope.Current.Log(new Log.FearCardRevealed((IFearCard)this)); // !!! after we remove Level1,2,3 from IFearCard, make FearCardBase implement this
		}
	}
	bool _flipped;

	public virtual void Activate( GameState gameState ) {
		var topCard = gameState.Fear.Deck.Pop();
		if(topCard != this)
			throw new InvalidOperationException( "Fear card must be on top of deck to activate it." );
		gameState.Fear.ActivatedCards.Push( topCard );
		gameState.Fear.OnCardActivated(topCard);
	}

	public Task ActAsync( int terrorLevel) {
		// show card to each user
		ActivatedTerrorLevel = terrorLevel; // this needs set BEFORE we generate the log
		Flipped = true; // this needs sent before the action occurs.
		var gs = GameState.Current;
		
		return terrorLevel switch {
			1 => Level1(gs),
			2 => Level2(gs), // !!!  Add abstract virtual
			3 => Level3(gs), // !!!  Add abstract virtual
			_ => throw new ArgumentOutOfRangeException(nameof(terrorLevel)),
		};
	}

	abstract public Task Level1( GameState gameState );
	abstract public Task Level2(GameState gameState);
	abstract public Task Level3(GameState gameState);
}

static public class IFearOptionsExtension {

	static public string GetDescription( this IFearCard options, int activation ) {
		var memberName = "Level" + activation;

		// This does not find interface methods declared as: void IFearCardOption.Level2(...)
		var member = options.GetType().GetMethod( memberName )
			?? throw new Exception( memberName + " not found on " + options.GetType().Name );

		var attr = (FearLevelAttribute)member.GetCustomAttributes( typeof( FearLevelAttribute ) ).Single();
		string description = attr.Description;
		return description;
	}

}

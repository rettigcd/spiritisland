using SpiritIsland.Log;

namespace SpiritIsland;

public interface IAdversary {
	int Level { get; set; }
	InvaderDeckBuilder InvaderDeckBuilder { get; }
	int[] FearCardsPerLevel { get; }

	/// <summary> Decks are already built, but tokens have not been placed yet. </summary>
	void Init( GameState gameState );
	/// <summary> Adjusts Tokens that are already placed. </summary>
	void AdjustPlacedTokens( GameState gamestate );

	AdversaryLevel[] Levels { get; }

	IEnumerable<AdversaryLevel> ActiveLevels { get; }

	AdversaryLossCondition LossCondition { get; }
}

abstract public class AdversaryBase : IAdversary {

	// Get: Available Levels
	public abstract AdversaryLevel[] Levels { get; }

	// Set: user selected levels
	public int Level { get; set; }

	// Before Decks are built: Config Fear and Invader Decks
	public int[] FearCardsPerLevel => Levels[Level].FearCards;
	
	public virtual InvaderDeckBuilder InvaderDeckBuilder { 
		get{
			return ActiveLevels
				.Select( m=>m.InvaderDeckBuilder )
				.LastOrDefault(x=>x is not null)
				?? InvaderDeckBuilder.Default;
		}
	}

	// After Decks are built, before Tokens are placed
	public virtual void Init( GameState gameState ) {

		foreach(var mod in ActiveLevels) {
			ActionScope.Current.Log( new SetupDescription( $"{mod.Title} - {mod.Description}" ) );
			mod.Init( gameState, this );
		}

	}
	class SetupDescription( string msg ) : ILogEntry {
		public LogLevel Level => LogLevel.Info;

		public string Msg( LogLevel level ) => msg;
	}

	// After Tokens have been placed
	public virtual void AdjustPlacedTokens( GameState gameState ) {
		foreach(var mod in ActiveLevels)
			mod.Adjust( gameState, this );

		// do this LAST since France needs access to Tokens placed at startup
		LossCondition?.Init( gameState );
	}

	public IEnumerable<AdversaryLevel> ActiveLevels => Levels.Take(Level+1);

	public virtual AdversaryLossCondition LossCondition => null;

}

public class AdversaryLevel( int _level, int _difficulty, int _fear1, int _fear2, int _fear3, string _title, string _description = "" ) {

	public int Level => _level;
	public int Difficulty { get; } = _difficulty;
	public int[] FearCards { get; } = [_fear1, _fear2, _fear3];
	public string Title { get; } = _title;
	public string Description { get; private set; } = _description;

	#region public - called by GameConfig to setup game

	public void Init(GameState gs, IAdversary adversary ) { 
		if(_escalation is not null)
			gs.InvaderDeck.Explore.Engine.Escalation = _escalation;
		InitFunc?.Invoke( gs, adversary );
	}

	public void Adjust( GameState gs, IAdversary adversary ) => AdjustFunc?.Invoke( gs, adversary );

	public InvaderDeckBuilder InvaderDeckBuilder { get; private set; }

	#endregion public - called by GameConfig to setup game

	public Action<GameState, IAdversary> InitFunc { get; set; }
	public Action<GameState, IAdversary> AdjustFunc { get; set; }

	#region public WithXXX() to setup config

	public AdversaryLevel WithDeckBuilder( InvaderDeckBuilder deckBuilder ) {
		InvaderDeckBuilder = deckBuilder;
		return this;
	}

	public AdversaryLevel WithInvaderCardOrder( string levels ) {
		if(0<Description.Length) Description += " ";
		Description += $"Invader Deck: {levels}";
		InvaderDeckBuilder = new InvaderDeckBuilder( levels );
		return this;
	}

	public AdversaryLevel WithEscalation( Func<GameState, Task> escalation ) {
		_escalation = escalation; return this;
	}

	#endregion public WithXXX() to setup config

	/// <summary>
	/// Just shows the Level/Escalation and the Title.
	/// </summary>
	public override string ToString() => _level == 0 
		? $"Escalation ({Title})" 
		: $"Level {_level} ({Title})";

	#region private 

	Func<GameState, Task> _escalation;

	#endregion
}

public class AdversaryLossCondition( string description, Action<GameState> additionalWinLossCondition ) {
	public virtual void Init( GameState gs ) {
		gs.AddWinLossCheck( _additionalWinLossCondition );
	}
	public string Description { get; } = description;
	readonly protected Action<GameState> _additionalWinLossCondition = additionalWinLossCondition;
}
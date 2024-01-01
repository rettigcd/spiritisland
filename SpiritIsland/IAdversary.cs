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
			return LevelMods
				.Select( m=>m.InvaderDeckBuilder )
				.LastOrDefault(x=>x is not null)
				?? InvaderDeckBuilder.Default;
		}
	}

	// After Decks are built, before Tokens are placed
	public virtual void Init( GameState gameState ) {
		foreach(var mod in LevelMods) {
			gameState.Log( new SetupDescription( $"{mod.Title} - {mod.Description}" ) );
			mod.Init( gameState, this );
		}
	}
	class SetupDescription : ILogEntry {
		public SetupDescription(string msg ) { _msg = msg; }
		readonly string _msg;
		public LogLevel Level => LogLevel.Info;

		public string Msg( LogLevel level ) => _msg;
	}

	// After Tokens have been placed
	public virtual void AdjustPlacedTokens( GameState gameState ) {
		foreach(var mod in LevelMods)
			mod.Adjust( gameState, this );
	}

	protected IEnumerable<AdversaryLevel> LevelMods => Levels.Take(Level+1);
}

public class AdversaryLevel {

	public AdversaryLevel( int difficulty, int fear1, int fear2, int fear3, string title, string description ) {
		Difficulty = difficulty;
		FearCards = new int[] { fear1, fear2, fear3 };
		Title = title;
		Description = description;
	}

	public int Difficulty { get; }
	public int[] FearCards { get; }
	public string Title { get; }
	public string Description { get; }

	public Action<GameState,IAdversary> InitFunc { get; set; }
	public Action<GameState, IAdversary> AdjustFunc { get; set; }

	public void Init(GameState gs, IAdversary adversary ) { 
		if(_escalation is not null)
			gs.InvaderDeck.Explore.Engine.Escalation = _escalation;
		if(_additionalWinLossCondition is not null)
			gs.AddWinLossCheck( _additionalWinLossCondition );
		InitFunc?.Invoke( gs, adversary );
	}
	public void Adjust( GameState gs, IAdversary adversary ) => AdjustFunc?.Invoke( gs, adversary );

	public AdversaryLevel WithInvaderDeck(params int[] cardLevels ) {
		InvaderDeckBuilder = new InvaderDeckBuilder( cardLevels );
		return this;
	}

	public InvaderDeckBuilder InvaderDeckBuilder { get; set; }

	public AdversaryLevel WithEscalation( Func<GameState, Task> escalation ) {
		_escalation = escalation; return this;
	}

	public AdversaryLevel WithWinLossCondition( Action<GameState> winLossCondition ) {
		_additionalWinLossCondition = winLossCondition; return this;
	}

	Func<GameState, Task> _escalation;
	Action<GameState> _additionalWinLossCondition;
}
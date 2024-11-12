namespace SpiritIsland;

public class AdversaryLevel( int _level, int _difficulty, int _fear1, int _fear2, int _fear3, string _title, string _description = "" ) {

	public int Level => _level;
	public int Difficulty { get; } = _difficulty;
	public int[] FearCards { get; } = [_fear1, _fear2, _fear3];
	public string Title { get; } = _title;
	public string Description { get; private set; } = _description;

	#region public - called by GameConfig to setup game

	/// <summary> Called just before gameState.Initialize(); </summary>
	public void Init(GameState gs, IAdversary adversary ) { 

		if(_escalation is not null)
			gs.InvaderDeck.Explore.Engine.Escalation = _escalation;

		InitFunc?.Invoke( gs, adversary );
	}

	/// <summary> Called after gameState.Initialize() </summary>
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

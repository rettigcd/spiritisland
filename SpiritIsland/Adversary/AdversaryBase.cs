using SpiritIsland.Log;

namespace SpiritIsland;

abstract public class AdversaryBase : IAdversary {

	public string AdvName { get; }

	#region constructor
	public AdversaryBase(string name) { 
		AdvName = name;
	}
	#endregion constructor

	// Get: Available Levels
	public abstract AdversaryLevel[] Levels { get; }

	// Set: user selected levels
	public int Level { get; set; }

	// Before Decks are built: Config Fear and Invader Decks
	public int[] FearCardsPerLevel => Levels[Level].FearCards;
	
	public virtual InvaderDeckBuilder InvaderDeckBuilder => ActiveLevels
		.Select( m=>m.InvaderDeckBuilder )
		.LastOrDefault(x=>x is not null)
		?? InvaderDeckBuilder.Default;

	public void AdjustFearCardCounts(int[] counts) {
		if( FearCardsPerLevel != null ) {
			counts[0] += (FearCardsPerLevel[0] - 3);
			counts[1] += (FearCardsPerLevel[1] - 3);
			counts[2] += (FearCardsPerLevel[2] - 3);
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

	public string Describe() {

		var rows = new List<string> {
			$"==== {AdvName} - Level:{Level} - Difficulty:{Levels[Level].Difficulty} ===="
		};
		// Loss
		if( LossCondition is not null ) {
			rows.Add($"\r\n-- Additional Loss Condition --");
			rows.Add(LossCondition.Description);
		}
		// Levels
		for( int i = 0; i <= Level; ++i ) {
			var lvlMod = Levels[i];
			string label = i == 0 ? "Escalation: " : "Level:" + i;
			rows.Add($"\r\n-- {label} {lvlMod.Title} --");
			rows.Add($"{lvlMod.Description}");
		}

		return rows.Join("\r\n");
	}


}

using SpiritIsland.Log;

namespace SpiritIsland;

public class Adversary : IAdversary {

	public IAdversaryBuilder Builder => _builder;
	readonly IAdversaryBuilder _builder;

	int IAdversary.Level => TopLevel.Level;
	string IAdversary.Name => _builder.Name;
	public AdversaryLevel[] ActiveLevels { get; }

	AdversaryLevel TopLevel => ActiveLevels[^1];

	public InvaderDeckBuilder InvaderDeckBuilder { get; }

	#region constructor
	public Adversary( IAdversaryBuilder builder, int level ) {
		_builder = builder;
		ActiveLevels = _builder.Levels.Take(level+1).ToArray();

		InvaderDeckBuilder = ActiveLevels
			.Select(m => m.InvaderDeckBuilder)
			.LastOrDefault(x => x is not null)
			?? InvaderDeckBuilder.Default;
	}

	#endregion constructor

	public void AdjustFearCardCounts(int[] counts) {
		var fearCards = TopLevel.FearCards;
		if( fearCards is not null ) {
			counts[0] += (fearCards[0] - 3);
			counts[1] += (fearCards[1] - 3);
			counts[2] += (fearCards[2] - 3);
		}
	}

	public void AdjustPlacedTokens(GameState gameState) {
		foreach( var mod in ActiveLevels )
			mod.Adjust(gameState, this);

		// do this LAST since France needs access to Tokens placed at startup
		_builder.LossCondition?.Init(gameState);
	}

	public string Describe() {
		var rows = new List<string> {
			$"==== {_builder.Name} - Level:{TopLevel.Level} - Difficulty:{TopLevel.Difficulty} ===="
		};
		// Loss
		if( _builder.LossCondition is not null ) {
			rows.Add($"\r\n-- Additional Loss Condition --");
			rows.Add(_builder.LossCondition.Description);
		}
		// Levels
		int i=0;
		foreach( var lvlMod in ActiveLevels ) {
			string label = i == 0 ? "Escalation: " : "Level:" + i; 
			rows.Add($"\r\n-- {label} {lvlMod.Title} --");
			rows.Add($"{lvlMod.Description}");
			++i;
		}

		return rows.Join("\r\n");
	}

	// After Decks are built, before Tokens are placed
	public virtual void Init(GameState gameState) {
		foreach( var mod in ActiveLevels ) {
			ActionScope.Current.Log(new SetupDescription($"{mod.Title} - {mod.Description}"));
			mod.Init(gameState, this);
		}
	}

	class SetupDescription(string msg) : ILogEntry {
		public LogLevel Level => LogLevel.Info;
		public string Msg(LogLevel level) => msg;
	}
}
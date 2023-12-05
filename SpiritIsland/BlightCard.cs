namespace SpiritIsland;

public abstract class BlightCard : IBlightCard {

	static readonly public FakeSpace Space = new FakeSpace( "BlightCard" ); // stores slow blight

	protected BlightCard(string name, string description, int side2BlightPerPlayer ) {
		Name = name;
		Description = description;
		this.startingBlightPerPlayer = 2;
		this.side2BlightPerPlayer = side2BlightPerPlayer;
	}

	public string Name { get; }
	public string Description { get; }
	public bool CardFlipped { get; set; }

	string IOption.Text => Name;

	public void OnGameStart( GameState gs ) {
		Tokens(gs).Init( startingBlightPerPlayer * gs.Spirits.Length + 1 );// +1 from Jan 2021 errata
	}

	public async Task OnBlightDepleated( GameState gs ) {
		if(!CardFlipped) {
			await Side1Depleted( gs );

		} else
			Side2Depleted(gs);

	}

	async Task Side1Depleted( GameState gs ) {
		CardFlipped = true;

		Tokens( gs ).Adjust( side2BlightPerPlayer * gs.Spirits.Length ); // ! Don't call .Add() here or token events will auto-remove what we just added.

		// Execute Immediate command
		var immediately = Immediately;
		if(immediately != null) {
			await using ActionScope actionScope = await ActionScope.Start( ActionCategory.Blight );
			await immediately.ActAsync( gs );
		}
	}

	static BlightTokenBinding Tokens(GameState gs) => gs.Tokens[Space].Blight;

	public abstract IActOn<GameState> Immediately { get; }

	protected virtual void Side2Depleted(GameState gameState) 
		=> GameOverException.Lost( "Blighted Island-" + Name );

	#region private

	readonly int startingBlightPerPlayer;
	readonly int side2BlightPerPlayer;

	#endregion
}
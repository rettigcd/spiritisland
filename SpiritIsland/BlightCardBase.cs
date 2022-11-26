namespace SpiritIsland;

public abstract class BlightCardBase : IBlightCard {

	protected BlightCardBase(string name, int side2BlightPerPlayer ) {
		Name = name;
		this.startingBlightPerPlayer = 2;
		this.side2BlightPerPlayer = side2BlightPerPlayer;
	}

	public string Name { get; }
	public bool CardFlipped { get; set; }

	public void OnGameStart( GameState gs ) {
		gs.blightOnCard = startingBlightPerPlayer * gs.Spirits.Length + 1; // +1 from Jan 2021 errata
	}

	public async Task OnBlightDepleated( GameState gs ) {
		if(!CardFlipped) {
			CardFlipped = true;
			gs.blightOnCard += side2BlightPerPlayer * gs.Spirits.Length;

			// Execute Immediate command
			var immediately = Immediately;
			if(immediately != null)
				await immediately.Execute( gs );

		} else
			Side2Depleted(gs);

	}

	public abstract DecisionOption<GameState> Immediately { get; }

	protected virtual void Side2Depleted(GameState gameState) 
		=> GameOverException.Lost( "Blighted Island-" + Name );

	#region private

	readonly int startingBlightPerPlayer;
	readonly int side2BlightPerPlayer;

	#endregion
}
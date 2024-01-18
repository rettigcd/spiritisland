namespace SpiritIsland;

public class NullBlightCard : IBlightCard {

	public bool CardFlipped {get; set; }

	public string Name => "[null]";
	public string Description => "[null]";
	string IOption.Text => Name;

	public IActOn<GameState> Immediately => new BaseCmd<GameState>("no action", (Action<GameState>)(_ => { }));

	int IBlightCard.Side2BlightPerPlayer => 1;

	public async Task OnBlightDepleated( GameState gs ) {
		if(!CardFlipped) {
			CardFlipped = true;
			await Tokens( gs ).AddAsync( 4 * gs.Spirits.Length );
			gs.blightOnCard_Add( 4 * gs.Spirits.Length );
		}
	}

	public void OnGameStart( GameState gs ) {
		Tokens(gs).Init( 5 * gs.Spirits.Length );
	}

	static BlightTokenBinding Tokens(GameState gs) => gs.Tokens[BlightCard.Space].Blight;
}
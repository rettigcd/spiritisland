namespace SpiritIsland;

public class NullBlightCard : BlightCard {
	public NullBlightCard():base("-null-","no action", 1) { }
	public override void OnGameStart(GameState gs) {
		base.OnGameStart(gs);
		InitBlight( 1000 );
	}
	public override IActOn<GameState> Immediately => new BaseCmd<GameState>("no action", (Action<GameState>)(_ => { }));
}
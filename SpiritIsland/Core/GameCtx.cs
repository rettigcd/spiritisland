namespace SpiritIsland;

public class GameCtx {

	public readonly GameState GameState;
	public readonly ActionCategory Category = ActionCategory.Fear;

	public GameCtx( GameState gs ) {
		this.GameState = gs;
	}

}

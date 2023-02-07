namespace SpiritIsland;

public class GameCtx {

	public readonly GameState GameState;

	#region constructor

	public GameCtx( GameState gs ) {
		this.GameState = gs;
	}

	#endregion constructor

	public IEnumerable<SelfCtx> Spirits {
		get {
			return this.GameState.Spirits.Select( s => s.BindSelf() );
		}
	}

}

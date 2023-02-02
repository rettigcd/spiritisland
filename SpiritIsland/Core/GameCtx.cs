namespace SpiritIsland;

public class GameCtx {

	public readonly GameState GameState;
	public readonly UnitOfWork ActionScope; // !!! Check if GameCtx is ever used or are new action-scopes created with foreach land, board, spirit

	#region constructor

	/// <summary> Caller is responsible for disposing of UnitOfWork </summary>
	public GameCtx( GameState gs, UnitOfWork actionScope ) {
		this.GameState = gs;
		ActionScope = actionScope;
	}

	#endregion constructor

	public IEnumerable<SelfCtx> Spirits {
		get {
			return this.GameState.Spirits.Select( s => s.BindSelf( GameState, ActionScope ) );
		}
	}

}

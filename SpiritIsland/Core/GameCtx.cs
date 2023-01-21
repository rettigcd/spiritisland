namespace SpiritIsland;

public class GameCtx {

	public readonly GameState GameState;
	public readonly UnitOfWork ActionScope;

	#region constructor

	/// <summary> Caller is responsible for disposing of UnitOfWork </summary>
	public GameCtx( GameState gs, UnitOfWork actionScope ) {
		this.GameState = gs;
		ActionScope = actionScope;
	}

	public GameCtx( GameState gs, ActionCategory cat ) {
		// !!! Nothing that uses this is disposing of the UnitOfWork at the end  (France & Tests)
		this.GameState = gs;
		ActionScope = gs.StartAction( cat );
	}

	#endregion constructor

	public IEnumerable<SelfCtx> Spirits {
		get {
			return this.GameState.Spirits.Select( s => s.BindSelf( GameState, ActionScope ) );
		}
	}

}
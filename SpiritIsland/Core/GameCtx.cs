namespace SpiritIsland;

public class GameCtx {

	public readonly GameState GameState;
	public readonly UnitOfWork UnitOfWork;

	#region constructor

	/// <summary> Caller is responsible for disposing of UnitOfWork </summary>
	public GameCtx( GameState gs, UnitOfWork unitOfWork ) {
		this.GameState = gs;
		UnitOfWork = unitOfWork;
	}

	public GameCtx( GameState gs, ActionCategory cat ) {
		// !!! Nothing that uses this is disposing of the UnitOfWork at the end  (France & Tests)
		this.GameState = gs;
		UnitOfWork = gs.StartAction( cat );
	}

	#endregion constructor

	public IEnumerable<SelfCtx> Spirits {
		get {
			return this.GameState.Spirits.Select( s => s.Bind( GameState, UnitOfWork ) );
		}
	}

	// !!! Remove these Lands... so we can get rid of FearCtx

	public IEnumerable<SpaceState> Lands( Func<SpaceState,bool> withCondition ) => GameState.AllActiveSpaces
		.Where( GameState.Island.Terrain_ForFear.IsInPlay )
		.Where( withCondition );

	public IEnumerable<SpaceState> LandsWithDisease() => GameState.AllActiveSpaces
		.Where( s => s.Disease.Any);

	public IEnumerable<SpaceState> LandsWithBeastDiseaseDahan() => GameState.AllActiveSpaces
		.Where( s => s.Dahan.Any || s.Beasts.Any || s.Disease.Any );

	public IEnumerable<SpaceState> LandsWithBeasts() => GameState.AllActiveSpaces.Where( SpaceFilters.HasBeast );

	public IEnumerable<SpaceState> LandsWithOrAdjacentToBeasts() => GameState.AllActiveSpaces
		.Where( SpaceFilters.HasBeastOrIsAdjacentToBeast );

}

public static class FearCtxExtensionForBac {

	// Extension to SpiritGameStateCtx
	public static async Task<SpaceState> AddStrifeToOne( this SelfCtx spirit, IEnumerable<SpaceState> options, params TokenClass[] groups ) {
		static bool HasInvaders( SpaceState s ) => s.HasInvaders();
		TargetSpaceCtx spaceCtx = await spirit.SelectSpace( "Add strife", options.Where( HasInvaders ).Select(x=>x.Space) );
		if(spaceCtx != null)
			await spaceCtx.AddStrife( groups );
		return spaceCtx?.Tokens;
	}

}
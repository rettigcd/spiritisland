namespace SpiritIsland;

public class FearCtx {

	public readonly GameState GameState;

	#region constructor

	public FearCtx(GameState gs) {
		this.GameState = gs;
	}

	#endregion constructor

	public IEnumerable<SelfCtx> Spirits {
		get {
			var actionId = Guid.NewGuid();
			return this.GameState.Spirits.Select( s => s.Bind( GameState, actionId ) );
		}
	}

	public async Task EachPlayerTakesActionInALand( SpaceAction action, Func<TargetSpaceCtx,bool> filter, bool differentLands = false ) {
		var used = new List<Space>();
		foreach(var spiritCtx in Spirits) {
			var spaceOptions = GameState.Island.AllSpaces
				.Where(s=>filter(spiritCtx.Target(s)))
				.Except(used)
				.ToArray();
			var spaceCtx = await spiritCtx.SelectSpace( action.Description, spaceOptions );
			if(spaceCtx == null) continue;
			if(differentLands)
				used.Add( spaceCtx.Space );
			await action.Execute(spaceCtx);
		}
	}

//	public InvaderBinding InvadersOn(Space space) => GameState.Invaders.On( space );

	#region Lands

	public IEnumerable<Space> Lands( Func<Space,bool> withCondition ) => GameState.Island.AllSpaces
		.Where( GameState.Island.Terrain_ForFear.IsInPlay )
		.Where( withCondition );

	public IEnumerable<Space> InlandLands => GameState.Island.AllSpaces.Where( s => s.IsInland );

	public bool WithDahanAndExplorers( Space space ) => WithDahan(space) && WithExplorers(space);
	public bool WithDahanAndInvaders( Space space ) => WithDahan( space ) && WithInvaders( space );

	public bool WithExplorers( Space space ) => GameState.Tokens[space].Has( Invader.Explorer );
	public bool WithInvaders( Space space ) => GameState.Tokens[space].HasInvaders();
	public bool WithDahan( Space space ) => GameState.Tokens[space].Dahan.Any;

	public bool WithDahanOrAdjacentTo5( Space space ) => WithDahanOrAdjacentTo(space,5);
	public bool WithDahanOrAdjacentTo3( Space space ) => WithDahanOrAdjacentTo(space,3);
	public bool WithDahanOrAdjacentTo1( Space space ) =>  WithDahanOrAdjacentTo(space,1);
	public bool WithDahanOrAdjacentTo( Space space, int count ) => GameState.DahanOn(space).Any || count <= space.Adjacent.Sum( a => GameState.DahanOn(a).Count );

	#endregion Lands

	public IEnumerable<Space> LandsWithStrife() => GameState.Island.AllSpaces
		.Where( s => GameState.Tokens[s].Keys.OfType<HealthToken>().Any( x => x.StrifeCount > 0 ) );

	public IEnumerable<Space> LandsWithDisease() => GameState.Island.AllSpaces
		.Where( s => GameState.Tokens[s].Disease.Any);

	public IEnumerable<Space> LandsWithBeastDiseaseDahan() => GameState.Island.AllSpaces
		.Where( s => {var tokens = GameState.Tokens[s]; return tokens.Dahan.Any || tokens.Beasts.Any || tokens.Disease.Any; } );

	public bool HasBeastOrIsAdjacentToBeast( Space space ) => space.Range( 1 ).Any( x => HasBeast(x) );
	public bool HasBeast( Space space ) => GameState.Tokens[space].Beasts.Any;
	public IEnumerable<Space> LandsWithBeasts() => GameState.Island.AllSpaces.Where( HasBeast );

	public IEnumerable<Space> LandsWithOrAdjacentToBeasts() => GameState.Island.AllSpaces.Where( HasBeastOrIsAdjacentToBeast );

}

public static class FearCtxExtensionForBac {

	// Extension to SpiritGameStateCtx
	public static async Task<Space> AddStrifeToOne( this SelfCtx spirit, IEnumerable<Space> options, params TokenClass[] groups ) {
		bool HasInvaders( Space s ) => spirit.Target(s).HasInvaders;
		TargetSpaceCtx spaceCtx = await spirit.SelectSpace( "Add strife", options.Where( HasInvaders ) );
		if(spaceCtx != null)
			await spaceCtx.AddStrife( groups );
		return spaceCtx?.Space;
	}

}
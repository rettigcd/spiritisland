using SpiritIsland.Select;

namespace SpiritIsland.JaggedEarth;

class VolcanoPresence : SpiritPresence {
	public VolcanoPresence(PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) {}

	public override bool CanBePlacedOn( SpaceState s, TerrainMapper tm ) => tm.MatchesTerrain( s, Terrain.Mountain );

	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
		Token = new VolcanoToken( spirit );
	}

	const string DontDestroyPresenceOnStr = "Don't Destroy Presence On Space";
	static public void SetDontDestroyPresenceOn( UnitOfWork actionScope, Space space )
		=> actionScope[DontDestroyPresenceOnStr] = space;

	public static bool DontDestroyPresenceOn( UnitOfWork actionScope, Space space )
		=> actionScope.ContainsKey( DontDestroyPresenceOnStr ) && space == (Space)actionScope[DontDestroyPresenceOnStr];


	const string DestroyedPresenceCount = "DestroyedPresenceCount";
	static public int GetPresenceDestroyedThisAction( UnitOfWork actionScope ) 
		=> actionScope.ContainsKey( DestroyedPresenceCount ) ? (int)actionScope[DestroyedPresenceCount] : 0;
	static public void AddPresenceDestroyedThisAction( UnitOfWork actionScope, int value ) 
		=> actionScope[DestroyedPresenceCount] = GetPresenceDestroyedThisAction(actionScope) + value;

}

public class VolcanoToken : SpiritPresenceToken, IHandleRemovingToken {
	readonly Spirit _spirit;
	public VolcanoToken(Spirit spirit ) {
		_spirit = spirit;
	}

	public Task ModifyRemoving( RemovingTokenArgs args ) {

		if( DestroysPresence( args ) && VolcanoPresence.DontDestroyPresenceOn( args.ActionScope, args.Space.Space )	)
			args.Count = 0;

		return Task.CompletedTask;
	}

	protected override async Task OnPresenceDestroyed( ITokenRemovedArgs args ) {
		await base.OnPresenceDestroyed( args );

		VolcanoPresence.AddPresenceDestroyedThisAction( args.ActionScope, args.Count );

		// Destroying Volcano presence, causes damage to Dahan and invaders
		// Create a TargetSpaceCtx to include Bandlands damage also.
		var gs = args.RemovedFrom.AccessGameState();
		var selfCtx = args.ActionScope.Category == ActionCategory.Spirit_Power // ??? is this needed => && actionScope.Owner == spirit
			? _spirit.BindMyPowers( gs, args.ActionScope )
			: _spirit.BindSelf( gs, args.ActionScope );
		var ctx = selfCtx.Target( args.RemovedFrom );

		await ctx.DamageInvaders( args.Count );

		// From BAC Rulebook p.16
		// When Spirit Powers Damage the Dahan, you may choose how that Damage is allocated, just like when you Damage Invaders.
		// Assuming Spirit-SpecialRule is more like Spirit-Powers and less like Ravage
		await ctx.DamageDahan( args.Count );

	}


}
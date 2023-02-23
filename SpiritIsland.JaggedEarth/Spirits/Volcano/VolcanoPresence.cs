namespace SpiritIsland.JaggedEarth;

class VolcanoPresence : SpiritPresence {
	public VolcanoPresence(PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) {}

	public override bool CanBePlacedOn( SpaceState s ) => ActionScope.Current.TerrainMapper.MatchesTerrain( s, Terrain.Mountain );

	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
		Token = new VolcanoToken( spirit );
	}

	static public ActionScopeValue<Space> SafeSpace = new( "Don't Destroy Presence On Space", (Space)default );

	const string DestroyedPresenceCount = "DestroyedPresenceCount";
	static public int GetPresenceDestroyedThisAction() => ActionScope.Current.SafeGet( DestroyedPresenceCount, 0 );
	static public void AddPresenceDestroyedThisAction( int value ) {
		ActionScope.Current[DestroyedPresenceCount] = GetPresenceDestroyedThisAction() + value;
	}

}

public class VolcanoToken : SpiritPresenceToken, IModifyRemovingToken {

	public VolcanoToken(Spirit spirit ):base(spirit) {}

	public void ModifyRemoving( RemovingTokenArgs args ) {
		if( DestroysMyPresence( args ) && VolcanoPresence.SafeSpace.Value == args.From.Space )
			args.Count = 0;
	}

	protected override async Task OnPresenceDestroyed( ITokenRemovedArgs args ) {
		await base.OnPresenceDestroyed( args );

		VolcanoPresence.AddPresenceDestroyedThisAction( args.Count );

		// Destroying Volcano presence, causes damage to Dahan and invaders
		// Create a TargetSpaceCtx to include Bandlands damage also.
		var selfCtx = ActionScope.Current.Category == ActionCategory.Spirit_Power // ??? is this needed => && actionScope.Owner == spirit
			? _spirit.BindMyPowers()
			: _spirit.BindSelf();
		var ctx = selfCtx.Target( args.From );

		await ctx.DamageInvaders( args.Count );

		// From BAC Rulebook p.16
		// When Spirit Powers Damage the Dahan, you may choose how that Damage is allocated, just like when you Damage Invaders.
		// Assuming Spirit-SpecialRule is more like Spirit-Powers and less like Ravage
		await ctx.DamageDahan( args.Count );

	}


}
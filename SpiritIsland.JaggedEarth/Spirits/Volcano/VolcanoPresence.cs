namespace SpiritIsland.JaggedEarth;

class VolcanoPresence( Spirit spirit, PresenceTrack t1, PresenceTrack t2 ) 
	: SpiritPresence( spirit, t1, t2, new VolcanoToken( spirit ) ) 
{
	public override bool CanBePlacedOn( Space s ) => ActionScope.Current.TerrainMapper.MatchesTerrain( s, Terrain.Mountain );

	static public ActionScopeValue<Space?> SafeSpace = new( "Don't Destroy Presence On Space", (Space?)default );

	#region Track Presence-Destroyed-This-Action
	const string DestroyedPresenceCount = "DestroyedPresenceCount";
	static public int GetPresenceDestroyedThisAction() => ActionScope.Current.SafeGet( DestroyedPresenceCount, 0 );
	static public void AddPresenceDestroyedThisAction( int value ) {
		ActionScope.Current[DestroyedPresenceCount] = GetPresenceDestroyedThisAction() + value;
	}
	#endregion Tracking Presence-Destroyed-This-Action
}

public class VolcanoToken( Spirit spirit ) : SpiritPresenceToken(spirit), IModifyRemovingToken {

	Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
		if( DestroysMyPresence( args ) && VolcanoPresence.SafeSpace.Value == args.From )
			args.Count = 0;
		return Task.CompletedTask;
	}

	protected override async Task OnPresenceDestroyed( ITokenRemovedArgs args ) {
		await base.OnPresenceDestroyed( args );

		VolcanoPresence.AddPresenceDestroyedThisAction( args.Count );

		// Destroying Volcano presence, causes damage to Dahan and invaders
		// Create a TargetSpaceCtx to include Bandlands damage also.

		var ctx = Self.Target( (Space)args.From );

		await ctx.DamageInvaders( args.Count );

		// From BAC Rulebook p.16
		// When Spirit Powers Damage the Dahan, you may choose how that Damage is allocated, just like when you Damage Invaders.
		// Assuming Spirit-SpecialRule is more like Spirit-Powers and less like Ravage
		await ctx.DamageDahan( args.Count );

	}

}
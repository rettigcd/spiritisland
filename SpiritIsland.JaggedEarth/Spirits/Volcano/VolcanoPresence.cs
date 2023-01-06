namespace SpiritIsland.JaggedEarth;

class VolcanoPresence : SpiritPresence {
	public VolcanoPresence(PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) {}

	public override bool CanBePlacedOn( SpaceState s, TerrainMapper tm ) => tm.MatchesTerrain( s, Terrain.Mountain );

	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
		DestroyBehavior = new DestroyPresence( (VolcanoLoomingHigh)spirit ); // ??? can't we just override instead of plugging this in.
	}

	class DestroyPresence : SpiritPresence.DefaultDestroyBehavior {
		readonly VolcanoLoomingHigh spirit;
		public DestroyPresence( VolcanoLoomingHigh spirit ) { this.spirit = spirit;}

		public override async Task DestroyPresenceApi( SpiritPresence presence, Space space, GameState gs, int count,DestoryPresenceCause cause, UnitOfWork actionScope ) {
			
			if( GetDontDesroyPresenceOn( actionScope, space ) ) return;

			await base.DestroyPresenceApi( presence, space, gs, count, cause, actionScope );

			AddPresenceDestroyedThisAction( actionScope, count );

			// Destroying Volcano presence, causes damage to Dahan and invaders
			// Create a TargetSpaceCtx to include Bandlands damage also.
			var selfCtx = actionScope.Category == ActionCategory.Spirit_Power // ??? is this needed => && actionScope.Owner == spirit
				? spirit.BindMyPowers( gs, actionScope )
				: spirit.BindSelf( gs, actionScope );
			var ctx = selfCtx.Target(space);

			await ctx.DamageInvaders( count );

			// From BAC Rulebook p.16
			// When Spirit Powers Damage the Dahan, you may choose how that Damage is allocated, just like when you Damage Invaders.
			// Assuming Spirit-SpecialRule is more like Spirit-Powers and less like Ravage
			await ctx.DamageDahan( count );
		}
	}
	const string DontDestroyPresenceOn = "Don't Destroy Presence On Space";
	static public void SetDontDestroyPresenceOn( UnitOfWork actionScope, Space space )
		=> actionScope[DontDestroyPresenceOn] = space;
	static bool GetDontDesroyPresenceOn( UnitOfWork actionScope, Space space )
		=> actionScope.ContainsKey(DontDestroyPresenceOn) && space == (Space)actionScope[DontDestroyPresenceOn];

	const string DestroyedPresenceCount = "DestroyedPresenceCount";
	static public int GetPresenceDestroyedThisAction( UnitOfWork actionScope ) 
		=> actionScope.ContainsKey( DestroyedPresenceCount ) ? (int)actionScope[DestroyedPresenceCount] : 0;
	static public void AddPresenceDestroyedThisAction( UnitOfWork actionScope, int value ) 
		=> actionScope[DestroyedPresenceCount] = GetPresenceDestroyedThisAction(actionScope) + value;

}

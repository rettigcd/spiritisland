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

		public override async Task DestroyPresenceApi( SpiritPresence presence, Space space, GameState gs, int count,DestoryPresenceCause cause, UnitOfWork unitOfWork ) {
			
			if( GetDontDesroyPresenceOn( unitOfWork, space ) ) return;

			await base.DestroyPresenceApi( presence, space, gs, count, cause, unitOfWork );

			AddPresenceDestroyedThisAction( unitOfWork, count );

			// Destroying Volcano presence, causes damage to Dahan and invaders
			// Create a TargetSpaceCtx to include Bandlands damage also.
			var selfCtx = unitOfWork.Category == ActionCategory.Spirit_Power // ??? is this needed => && unitOfWork.Owner == spirit
				? spirit.BindMyPowers( gs, unitOfWork )
				: spirit.BindSelf( gs, unitOfWork );
			var ctx = selfCtx.Target(space);

			await ctx.DamageInvaders( count );

			// From BAC Rulebook p.16
			// When Spirit Powers Damage the Dahan, you may choose how that Damage is allocated, just like when you Damage Invaders.
			// Assuming Spirit-SpecialRule is more like Spirit-Powers and less like Ravage
			await ctx.DamageDahan( count );
		}
	}
	const string DontDestroyPresenceOn = "Don't Destroy Presence On Space";
	static public void SetDontDestroyPresenceOn( UnitOfWork unitOfWork, Space space )
		=> unitOfWork[DontDestroyPresenceOn] = space;
	static bool GetDontDesroyPresenceOn( UnitOfWork unitOfWork, Space space )
		=> unitOfWork.ContainsKey(DontDestroyPresenceOn) && space == (Space)unitOfWork[DontDestroyPresenceOn];

	const string DestroyedPresenceCount = "DestroyedPresenceCount";
	static public int GetPresenceDestroyedThisAction( UnitOfWork unitOfWork ) 
		=> unitOfWork.ContainsKey( DestroyedPresenceCount ) ? (int)unitOfWork[DestroyedPresenceCount] : 0;
	static public void AddPresenceDestroyedThisAction( UnitOfWork unitOfWork, int value ) 
		=> unitOfWork[DestroyedPresenceCount] = GetPresenceDestroyedThisAction(unitOfWork) + value;

}

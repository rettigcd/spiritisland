using SpiritIsland.Select;
using System.Xml.Linq;

namespace SpiritIsland.JaggedEarth;

class VolcanoPresence : SpiritPresence {
	public VolcanoPresence(PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) {}

	public override bool CanBePlacedOn( SpaceState s ) => UnitOfWork.Current.TerrainMapper.MatchesTerrain( s, Terrain.Mountain );

	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
		Token = new VolcanoToken( spirit );
	}

	const string DontDestroyPresenceOnStr = "Don't Destroy Presence On Space";
	static public void SetDontDestroyPresenceOn( Space space ) => UnitOfWork.Current[DontDestroyPresenceOnStr] = space;

	public static bool DontDestroyPresenceOn( Space space )	=> space == UnitOfWork.Current.SafeGet<Space>( DontDestroyPresenceOnStr );

	const string DestroyedPresenceCount = "DestroyedPresenceCount";
	static public int GetPresenceDestroyedThisAction() => UnitOfWork.Current.SafeGet( DestroyedPresenceCount, 0 );
	static public void AddPresenceDestroyedThisAction( int value ) {
		UnitOfWork.Current[DestroyedPresenceCount] = GetPresenceDestroyedThisAction() + value;
	}

}

public class VolcanoToken : SpiritPresenceToken, IHandleRemovingToken {

	public VolcanoToken(Spirit spirit ):base(spirit) {}

	public Task ModifyRemoving( RemovingTokenArgs args ) {

		if( DestroysPresence( args ) && VolcanoPresence.DontDestroyPresenceOn( args.Space.Space )	)
			args.Count = 0;

		return Task.CompletedTask;
	}

	protected override async Task OnPresenceDestroyed( ITokenRemovedArgs args ) {
		await base.OnPresenceDestroyed( args );

		VolcanoPresence.AddPresenceDestroyedThisAction( args.Count );

		// Destroying Volcano presence, causes damage to Dahan and invaders
		// Create a TargetSpaceCtx to include Bandlands damage also.
		var gs = GameState.Current;
		var selfCtx = UnitOfWork.Current.Category == ActionCategory.Spirit_Power // ??? is this needed => && actionScope.Owner == spirit
			? _spirit.BindMyPowers( gs )
			: _spirit.BindSelf();
		var ctx = selfCtx.Target( args.RemovedFrom );

		await ctx.DamageInvaders( args.Count );

		// From BAC Rulebook p.16
		// When Spirit Powers Damage the Dahan, you may choose how that Damage is allocated, just like when you Damage Invaders.
		// Assuming Spirit-SpecialRule is more like Spirit-Powers and less like Ravage
		await ctx.DamageDahan( args.Count );

	}


}
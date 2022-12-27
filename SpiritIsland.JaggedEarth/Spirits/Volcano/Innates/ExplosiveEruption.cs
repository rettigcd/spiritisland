namespace SpiritIsland.JaggedEarth;

[InnatePower(Name), Fast, Erruption]
public class ExplosiveEruption {

	const string Name = "Explosive Eruption";

	[ExplosiveInnateOption( "2 fire, 2 earth", 2, "In one land within range 1, X Damage",0)]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		int destroyedCount = VolcanoPresence.GetPresenceDestroyedThisAction( ctx.CurrentActionId );
		TargetSpaceCtx spaceCtx = await ctx.SelectAdjacentLandOrSelf($"Apply {destroyedCount} damage to");
		await spaceCtx.DamageInvaders(destroyedCount );
	}

	[ExplosiveInnateOption( "3 fire, 3 earth", 4, "Generate X fear.",1)]
	static public Task Option2( TargetSpaceCtx ctx ) {
		int destroyedCount = VolcanoPresence.GetPresenceDestroyedThisAction( ctx.CurrentActionId );
		ctx.AddFear( destroyedCount );
		return Task.CompletedTask;
	}

	[ExplosiveInnateOption( "4 fire, 2 air, 4 earth", 6, "In each land within range 1, 4 Damage.  Add 1 blight to target land; doing so does not Destroy your presence.",2)]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		// In each land within range 1, 4 Damage. 
		await ctx.DamageInvaders(4);
		foreach(SpaceState adj in ctx.Adjacent.OrderBy(x=>x.Space.Text)) // ordered to help tests
			await ctx.Target(adj).DamageInvaders(4);

		// Add 1 blight to target land; doing so does not Destroy your presence.
		VolcanoPresence.SetDontDestroyPresenceOn( ctx.CurrentActionId, ctx.Space );
		await ctx.AddBlight(1);
	}

	[ExplosiveInnateOption( "5 fire, 3 air, 5 earth",10,"In each land within range 2, +4 Damage.  In each land adjacent to the target, add 1 blight if it doesn't have any.",3)]
	static public async Task Option4(TargetSpaceCtx ctx ) {
		// In each land within range 2, (This range cannot be extended)
		foreach(SpaceState space in ctx.GameState.Tokens.PowerUp( ctx.Space.Range( 2 ) ))
			// +4 Damage.
			await ctx.Target(space.Space).DamageInvaders(4);

		// In each land adjacent to the target
		// add 1 blight if it doesn't have any.
		foreach(var adj in ctx.Adjacent) {
			var adjCtx = ctx.Target(adj);
			if(!adjCtx.Blight.Any)
				await adjCtx.AddBlight(1);
		}
	}

}

// Allows user to enter # of presence to destroy while generating the CTX
class ErruptionAttribute : FromPresenceAttribute {
	public ErruptionAttribute() : base( 0 ) { }

	/// <summary>
	/// Override, so we can destroy presence After targeting, prior to Tier-evaluation
	/// </summary>
	public override async Task<object> GetTargetCtx( string powerName, SelfCtx ctx ) {

		var target = (TargetSpaceCtx)await base.GetTargetCtx( powerName, ctx );

		int count = await target.Self.SelectNumber( "# of presence to destroy?", target.Presence.Count, 0 );

		// Destroy them now
		await target.Presence.Destroy( target.Space, count, DestoryPresenceCause.SpiritPower );

		return target;
	}

	/// <summary>
	/// Override so we can return a custom a custom criteria that flags this as an Innate Power
	/// </summary>
	protected override async Task<TargetCriteria> GetCriteria( SelfCtx ctx ) { 
		return new VolcanicPeaksTowerOverTheLandscape.InnateTargetCriteria( ctx.TerrainMapper, await CalcRange( ctx ), _targetFilters );
	}
}

public class ExplosiveInnateOptionAttribute : InnateOptionAttribute {
	public ExplosiveInnateOptionAttribute( string elementText, int destroyedPresence, string description, int group )
		: base( elementText, description, group ) {
		DestroyedPresenceThreshold = destroyedPresence;
	}
	public int DestroyedPresenceThreshold { get; }

	public override string ThresholdString => base.ThresholdString + $" {DestroyedPresenceThreshold} destroyedpresence";
}


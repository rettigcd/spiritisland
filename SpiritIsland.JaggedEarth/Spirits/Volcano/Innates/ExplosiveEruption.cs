namespace SpiritIsland.JaggedEarth;

[InnatePower(Name), Fast, Erruption]
public class ExplosiveEruption {

	public const string Name = "Explosive Eruption";

	[ExplosiveInnateOption( "2 fire, 2 earth", 2, "In one land within range 1, X Damage",0)]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		int destroyedCount = VolcanoPresence.GetPresenceDestroyedThisAction();

		Space space = await ctx.Self.SelectAsync( new A.SpaceDecision(
			$"Apply {destroyedCount} damage to",
			ctx.Space.Range( 1 ),
			Present.Always
		) );
		if( space == null ) return;

		await ctx.Target( space ).DamageInvaders(destroyedCount );
	}

	[ExplosiveInnateOption( "3 fire, 3 earth", 4, "Generate X fear.",1)]
	static public Task Option2( TargetSpaceCtx ctx ) {
		int destroyedCount = VolcanoPresence.GetPresenceDestroyedThisAction();
		return ctx.AddFear( destroyedCount );
	}

	[ExplosiveInnateOption( "4 fire, 2 air, 4 earth", 6, "In each land within range 1, 4 Damage.  Add 1 blight to target land; doing so does not Destroy your presence.",2)]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		// In each land within range 1, 4 Damage. 
		await ctx.DamageInvaders(4);
		foreach(Space adj in ctx.Adjacent.OrderBy(x=>x.Label)) // ordered to help tests
			await ctx.Target(adj).DamageInvaders(4);

		// Add 1 blight to target land; doing so does not Destroy your presence.
		VolcanoPresence.SafeSpace.Value = ctx.Space;
		await ctx.AddBlight(1);
	}

	[ExplosiveInnateOption( "5 fire, 3 air, 5 earth",10,"In each land within range 2, +4 Damage.  In each land adjacent to the target, add 1 blight if it doesn't have any.",3)]
	static public async Task Option4(TargetSpaceCtx ctx ) {
		// In each land within range 2, (This range cannot be extended)
		foreach( Space space in ctx.Space.Range(2) )
			// +4 Damage.
			await ctx.Target(space).DamageInvaders(4);

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
	public override async Task<object> GetTargetCtx( string powerName, Spirit self ) {

		var target = (TargetSpaceCtx)await base.GetTargetCtx( powerName, self );

		int count = await target.Self.SelectNumber( "# of presence to destroy?", target.Presence.Count, 0 );

		// Destroy them now
		await target.Space.Destroy( self.Presence.Token, count );

		return target;
	}

	/// <summary>
	/// Override so we can return a custom criteria that flags this as an Innate Power so Volcano won't extend range.
	/// </summary>
	protected override async Task<TargetCriteria> ApplySpiritModsToGetTargetCriteria( Spirit self ) { 
		return new VolcanicPeaksTowerOverTheLandscape.InnateTargetCriteria( await CalcRange( self ), self, _targetFilters );
	}
}

public class ExplosiveInnateOptionAttribute( string elementText, int destroyedPresence, string description, int group ) 
	: InnateTierAttribute( elementText, description, group )
{
	public int DestroyedPresenceThreshold { get; } = destroyedPresence;

	public override string ThresholdString => base.ThresholdString + $" {DestroyedPresenceThreshold} destroyedpresence";
}


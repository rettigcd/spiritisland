namespace SpiritIsland.JaggedEarth;

public class ForestsOfLivingObsidian {

	[MajorCard("Forests of Living Obsidian",4,Element.Sun,Element.Fire,Element.Earth,Element.Plant), Slow, FromPresence(0)]
	[RepeatIf("2 sun,3 fire,3 earth")]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// add 1 badland.
		await ctx.Badlands.Add( 1 );

		// Push all dahan.
		await ctx.PushDahan( ctx.Dahan.CountAll );

		// 1 damage to each invader.

		// if the original land is your sacredsite, +1 Damage to each invader
		bool fromSS = ctx.Self.PowerRangeCalc.GetTargetOptionsFromKnownSource(new SpaceState[] {ctx.Tokens }, new TargetCriteria(0) )
			.Any(ctx.Self.Presence.IsSacredSite); // using range calculator in case they used range extender.

		int damageToEach = fromSS ? 2 : 1; 
		await ctx.DamageEachInvader( damageToEach );

		// if you have 2 sun 3 fire 3 earh: Repeat this power
	}

}
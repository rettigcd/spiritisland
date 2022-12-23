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
		int damageToEach = ctx.Presence.IsSelfSacredSite ? 2: 1; // !!! not correct if they use a range extender

		await ctx.DamageEachInvader( damageToEach );

		// if you have 2 sun 3 fire 3 earh: Repeat this power
	}

}
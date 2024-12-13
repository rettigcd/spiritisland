namespace SpiritIsland.JaggedEarth;

public class ForestsOfLivingObsidian {

	[MajorCard("Forests of Living Obsidian",4,Element.Sun,Element.Fire,Element.Earth,Element.Plant), Slow, FromPresence(0)]
	[RepeatIf("2 sun,3 fire,3 earth")]
	[Instructions( "Add 1 Badlands. Push all Dahan. 1 Damage to each Invader. If the origin land is your Sacred Site, +1 Damage to each Invader.  -If you have- 2 Sun, 3 Fire, 3 Earth: Repeat this Power." ), Artist( Artists.LucasDurham )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// add 1 badland.
		await ctx.Badlands.AddAsync( 1 );

		// Push all dahan.
		await ctx.PushDahan( ctx.Dahan.CountAll );

		// 1 damage to each invader.

		// if the original land is your sacredsite, +1 Damage to each invader
		bool fromSS = TargetSpaceAttribute.TargettedSpace.Sources.Any( ctx.Self.Presence.IsSacredSite );

		int damageToEach = fromSS ? 2 : 1; 
		await ctx.DamageEachInvader( damageToEach );

		// if you have 2 sun 3 fire 3 earh: Repeat this power
	}

}
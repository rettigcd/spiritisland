namespace SpiritIsland.JaggedEarth;

public class FavorOfTheSunAndStarlitDark{

	[MinorCard("Favor of the Sun and Star-Lit Dark",1,Element.Sun,Element.Moon,Element.Plant),Fast,FromSacredSite(1)]
	[Instructions( "Defend 4. Push up to 1 Blight. -If you have- 2 Sun: 1 Fear" ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// Defend 4
		ctx.Defend(4);

		// Push up to 1 blight.
		await ctx.PushUpTo(1,Token.Blight);

		// If you have 2 sun: 1 fear
		if(await ctx.YouHave("2 sun"))
			await ctx.AddFear(1);
	}

}
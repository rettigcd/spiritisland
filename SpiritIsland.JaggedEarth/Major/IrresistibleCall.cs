namespace SpiritIsland.JaggedEarth;

public class IrresistibleCall {

	const string Name = "Irresistible Call";

	[MajorCard(Name,6,Element.Sun,Element.Air,Element.Plant), Fast, FromSacredSite(2,Target.Inland)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {

		// Gather 5 town, 5 dahan, 5 beast, and 15 explorer.
		await ctx.Gatherer
			.AddGroup(5,Human.Town)
			.AddGroup(5,Human.Dahan)
			.AddGroup(5,Token.Beast)
			.AddGroup(15,Human.Explorer)
			.GatherN();

		// if you have 2 sun, 3 air, 2 plant:
		if( await ctx.YouHave("2 sun,3 air,2 plant" )) {
			// invaders skip all actions in target land.
			ctx.SkipAllInvaderActions( Name );
			// Isolate target land.
			ctx.Isolate();
		}

	}

}
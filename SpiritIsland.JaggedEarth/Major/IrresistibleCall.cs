namespace SpiritIsland.JaggedEarth;

public class IrresistibleCall {

	const string Name = "Irresistible Call";

	[MajorCard(Name,6,Element.Sun,Element.Air,Element.Plant), Fast, FromSacredSite(2,Filter.Inland)]
	[Instructions( "Gather 5 Town, 5 Dahan, 5 Beasts, and 15 Explorer. -If you have- 2 Sun, 3 Air, 2 Plant: Invaders skip all Actions in target land. Isolate target land." ), Artist( Artists.LucasDurham )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {

		// Gather 5 town, 5 dahan, 5 beast, and 15 explorer.
		await ctx.Gatherer
			.AddGroup(5,Human.Town)
			.AddGroup(5,Human.Dahan)
			.AddGroup(5,Token.Beast)
			.AddGroup(15,Human.Explorer)
			.DoN();

		// if you have 2 sun, 3 air, 2 plant:
		if( await ctx.YouHave("2 sun,3 air,2 plant" )) {
			// invaders skip all actions in target land.
			ctx.Tokens.SkipAllInvaderActions( Name );
			// Isolate target land.
			ctx.Isolate();
		}

	}

}
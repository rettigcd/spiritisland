namespace SpiritIsland.JaggedEarth;

public class MeltEarthIntoQuicksand {

	[MajorCard("Melt Earth into Quicksand",4,Element.Moon,Element.Water,Element.Earth), Fast, FromPresence(1, Target.SandOrWetland)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear.
		ctx.AddFear(1);

		// 2 damamge.
		await ctx.DamageInvaders(2);

		// Isolate target land.
		ctx.Isolate();

		// After invaders / dahan are Moved into target land, Destroy them.
		ctx.GameState.Tokens.TokenAdded.ForRound.Add( async ( args ) => {
			if( args.Space == ctx.Space
				&& args.Token.Class.IsOneOf(Invader.Explorer,Invader.Town,Invader.City,TokenType.Dahan ) 
			) 
				await args.GameState.Tokens[args.Space].Destroy(args.Token,args.Count);
		} );

		// if you have 2 moon, 4 water, 2 earth:
		if( await ctx.YouHave("2 moon,4 water,2 earth" )) {
			// +4 damamge,
			await ctx.DamageInvaders(4);
			// Add 1 badland.
			await ctx.Badlands.Add(1);
			// Add 1 wilds
			await ctx.Wilds.Add(1);

		}

	}

}
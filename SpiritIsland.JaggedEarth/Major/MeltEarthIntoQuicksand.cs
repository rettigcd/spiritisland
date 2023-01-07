namespace SpiritIsland.JaggedEarth;

public class MeltEarthIntoQuicksand {
	const string Name = "Melt Earth into Quicksand";

	[MajorCard(Name,4,Element.Moon,Element.Water,Element.Earth), Fast, FromPresence(1, Target.Sand, Target.Wetland )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear.
		ctx.AddFear(1);

		// 2 damamge.
		await ctx.DamageInvaders(2);

		// Isolate target land.
		ctx.Isolate();

		// After invaders / dahan are Moved into target land, Destroy them.
		ctx.Tokens.Adjust( new TokenAddedHandler(Name, async ( args ) => {
			if(args.Token.Class.Category.IsOneOf( TokenCategory.Invader, TokenCategory.Dahan ) )
				await args.AddedTo.Destroy( args.Token, args.Count ); // !!! This looks like it might go around Bringer's powers.  Test!
		} ), 1 );

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
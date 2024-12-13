namespace SpiritIsland.JaggedEarth;

public class MeltEarthIntoQuicksand {
	const string Name = "Melt Earth Into Quicksand";

	[MajorCard(Name,4,Element.Moon,Element.Water,Element.Earth), Fast, FromPresence(1, Filter.Sands, Filter.Wetland )]
	[Instructions( "1 Fear. 2 Damage. Isolate target land. After Invaders / Dahan are Moved into target land, Destroy them.  -If you have- 2 Moon, 4 Water, 2 Earth: +4 Damage. Add 1 Badlands. Add 1 Wilds." ), Artist( Artists.LucasDurham )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear.
		await ctx.AddFear(1);

		// 2 damamge.
		await ctx.DamageInvaders(2);

		// Isolate target land.
		ctx.Isolate();

		// After invaders / dahan are Moved into target land, Destroy them.
		ctx.Space.Adjust( new Quicksand(), 1 );

		// if you have 2 moon, 4 water, 2 earth:
		if( await ctx.YouHave("2 moon,4 water,2 earth" )) {
			// +4 damamge,
			await ctx.DamageInvaders(4);
			// Add 1 badland.
			await ctx.Badlands.AddAsync(1);
			// Add 1 wilds
			await ctx.Wilds.AddAsync(1);

		}

	}

	class Quicksand : BaseModEntity, IHandleTokenAdded, IEndWhenTimePasses {
		public async Task HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
			if(args.Added.HasAny(TokenCategory.Invader,TokenCategory.Dahan))
				await to.Destroy( args.Added, args.Count );
		}
	}
}
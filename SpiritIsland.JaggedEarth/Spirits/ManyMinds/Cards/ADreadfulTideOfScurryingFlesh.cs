namespace SpiritIsland.JaggedEarth;

public class ADreadfulTideOfScurryingFlesh {
	const string Name = "A Dreadful Tide of Scurrying Flesh";
	[SpiritCard(Name,0, Element.Moon, Element.Air,Element.Water, Element.Animal), Fast, FromSacredSite(1,Target.TwoBeasts)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// Remove up to half (round down) of beast in target land.
		int removable = ctx.Beasts.Count / 2;
		int removed = await ctx.Self.SelectNumber("# of Beasts to Remove for 2 fear & skip one invader action", removable,0);
		await ctx.Beasts.Remove( removed, RemoveReason.Removed );

		// For each beast Removed,
		// 2 fear
		ctx.AddFear( 2*removed );
		// and skip one Invader Action
		for(int i = 0; i < removed; ++i) {
			// !!! Maybe this is like Infestation of Venomous Spiders and user can select later.
			await ctx.SelectActionOption( "Skip Invader Action",
				new SpaceAction("Ravage",  ctx => ctx.SkipRavage()),
				new SpaceAction("Build",   ctx => ctx.Skip1Build(Name)),
				new SpaceAction("Explore", ctx => ctx.SkipExplore())
			);
		}
	}

}
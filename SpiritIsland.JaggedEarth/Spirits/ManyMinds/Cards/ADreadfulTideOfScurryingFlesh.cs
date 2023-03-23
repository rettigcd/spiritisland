namespace SpiritIsland.JaggedEarth;

public class ADreadfulTideOfScurryingFlesh {

	const string Name = "A Dreadful Tide of Scurrying Flesh";
	[SpiritCard(Name,0, Element.Moon, Element.Air,Element.Water, Element.Animal), Fast, FromSacredSite(1,Target.TwoBeasts)]
	[Instructions( "Remove up to half (round down) of Beasts in target land. For each Beasts Removed, 2 Fear and skip one Invader Action." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// Remove up to half (round down) of beast in target land.
		int removable = ctx.Beasts.Count / 2;
		int removed = await ctx.Self.SelectNumber("# of Beasts to Remove for 2 fear & skip one invader action", removable,0);
		await ctx.Beasts.Remove( removed, RemoveReason.Removed );

		// For each beast Removed,
		// 2 fear
		ctx.AddFear( 2*removed );
		// and skip one Invader Action
		while(0<removed--)
			ctx.Tokens.Adjust( new SkipAnyInvaderAction(Name, ctx.Self), 1 );
	}

}
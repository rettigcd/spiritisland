namespace SpiritIsland.Basegame;

public class ParalyzingFright {

	public const string Name = "Paralyzing Fright";

	[MajorCard(ParalyzingFright.Name,4,Element.Air,Element.Earth),Fast,FromSacredSite(1)]
	[Instructions( "4 Fear. Invaders skip all Actions in target land this turn. -If you have- 2 Air, 3 Earth: +4 Fear." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 4 fear
		await ctx.AddFear(4);

		// invaders skip all actions in target land this turn
		ctx.Space.SkipAllInvaderActions(Name);

		// if you have 2 air 3 earth, +4 fear
		if(await ctx.YouHave("2 air,3 earth"))
			await ctx.AddFear(4);

	}

}
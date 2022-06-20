namespace SpiritIsland.Basegame;

public class ParalyzingFright {

	public const string Name = "Paralyzing Fright";

	[MajorCard(ParalyzingFright.Name,4,Element.Air,Element.Earth)]
	[Fast]
	[FromSacredSite(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 4 fear
		ctx.AddFear(4);

		// invaders skip all actions in target land this turn
		ctx.SkipAllInvaderActions(Name);

		// if you have 2 air 3 earth, +4 fear
		if(await ctx.YouHave("2 air,3 earth"))
			ctx.AddFear(4);

	}

}
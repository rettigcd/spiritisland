namespace SpiritIsland.JaggedEarth;

public class StrikeLowWithSuddenFevers {

	const string Name = "Strike Low with Sudden Fevers";

	[SpiritCard(Name,2,Element.Fire,Element.Air,Element.Earth,Element.Animal), Fast, FromPresence(1,Target.Disease) ]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear.
		ctx.AddFear(1);
		// Invaders skip Ravage Actions.
		ctx.Tokens.SkipRavage(Name);
		return Task.CompletedTask;
	}

}
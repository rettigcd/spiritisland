namespace SpiritIsland.JaggedEarth;

public class StrikeLowWithSuddenFevers {

	[SpiritCard("Strike Low with Sudden Fevers",2,Element.Fire,Element.Air,Element.Earth,Element.Animal), Fast, FromPresence(1) ]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear.
		ctx.AddFear(1);
		// Invaders skip Ravage Actions.
		ctx.SkipRavage();
		return Task.CompletedTask;
	}

}
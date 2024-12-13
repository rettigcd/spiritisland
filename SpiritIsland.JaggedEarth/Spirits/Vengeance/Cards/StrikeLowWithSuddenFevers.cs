namespace SpiritIsland.JaggedEarth;

public class StrikeLowWithSuddenFevers {

	const string Name = "Strike Low With Sudden Fevers";

	[SpiritCard(Name,2,Element.Fire,Element.Air,Element.Earth,Element.Animal), Fast, FromPresence(1,Filter.Disease) ]
	[Instructions( "1 Fear. Invaders skip Ravage Actions." ), Artist( Artists.DamonWestenhofer )]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// Invaders skip Ravage Actions.
		ctx.Space.SkipRavage(Name);
		// 1 fear.
		return ctx.AddFear(1);
	}

}
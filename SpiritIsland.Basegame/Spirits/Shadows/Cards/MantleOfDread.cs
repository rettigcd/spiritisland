namespace SpiritIsland.Basegame;

public class MantleOfDread {

	[SpiritCard("Mantle of Dread",1,Element.Moon,Element.Fire,Element.Air),Slow,AnySpirit]
	[Instructions( "2 Fear. Target Spirit may Push 1 Explorer and 1 Town from a land where it has Presence." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpiritCtx ctx ){

		// 2 fear
		ctx.AddFear( 2 );

		// target spirit may push 1 explorer and 1 town from land where it has presence

		// Select Land
		Space? pushLand = await ctx.Other.Select("Select land to push 1 exploer & 1 town from", ctx.Other.Presence.Lands, Present.Done);
		if(pushLand is null) return;

		// Push Town / Explorer
		await pushLand.SourceSelector
			.AddGroup(1,Human.Town)
			.AddGroup(1,Human.Explorer)
			.PushUpToN(ctx.Self);
			
	}

}
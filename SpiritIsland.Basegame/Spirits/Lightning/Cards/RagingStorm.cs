namespace SpiritIsland.Basegame;
	
public class RagingStorm {
	public const string Name = "Raging Storm";

	[SpiritCard(RagingStorm.Name,3,Element.Fire,Element.Air,Element.Water)]
	[Slow]
	[FromPresence(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// 1 damange to each invader.
		await ctx.DamageEachInvader(1);
	}

}
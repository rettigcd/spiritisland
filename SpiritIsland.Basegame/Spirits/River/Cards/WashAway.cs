namespace SpiritIsland.Basegame;

public class WashAway {

	public const string Name = "Wash Away";

	[SpiritCard(WashAway.Name, 1, Element.Water, Element.Earth)]
	[Slow]
	[FromPresence(1,Target.ExplorerOrTown)]
//	[Preselect("Push up to 3", "Explorer,Town" )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){
		await ctx.PushUpTo( 3, Human.Explorer_Town );
	}

}
namespace SpiritIsland.JaggedEarth;

public class LavaFlows {

	[SpiritCard("Lava Flows", 1, Element.Fire, Element.Earth), Slow, FromPresence(1)]
	public static Task ActAsync(TargetSpaceCtx ctx ) { 
		return ctx.SelectActionOption(
			new SpaceAction("+1 badland, +1 wilds", ctx => { ctx.Badlands.Add(1); ctx.Wilds.Add(1); }),
			new SpaceAction("1 damage", ctx => ctx.DamageInvaders(1) )
		);
	}

}
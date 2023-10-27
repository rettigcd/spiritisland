namespace SpiritIsland.JaggedEarth;

public class LavaFlows {

	[SpiritCard("Lava Flows", 1, Element.Fire, Element.Earth), Slow, FromPresence(1)]
	[Instructions( "Add 1 Badlands and 1 Wilds. -or- 1 Damage." ), Artist( Artists.MoroRogers )]
	public static Task ActAsync(TargetSpaceCtx ctx ) { 
		return ctx.SelectActionOption(
			new SpaceCmd("+1 badland, +1 wilds", ctx => { ctx.Badlands.AddAsync(1); ctx.Wilds.AddAsync(1); }),
			new SpaceCmd("1 damage", ctx => ctx.DamageInvaders(1) )
		);
	}

}
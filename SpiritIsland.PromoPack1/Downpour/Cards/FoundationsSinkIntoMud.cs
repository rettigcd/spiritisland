namespace SpiritIsland.PromoPack1;

internal class FoundationsSinkIntoMud {

	const string Name = "Foundations Sink into Mud";

	[SpiritCard( Name, 1, Element.Water, Element.Earth )]
	[Slow]
	[FromPresence( 0 )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return Cmd.Pick1(
			// 2 Damage to Town.
			new DecisionOption<TargetSpaceCtx>("2 damage to towns", x=>x.DamageInvaders(2,Invader.Town)),
			// If target land is wetland, you may instead deal 1 Damage to each town / City
			new DecisionOption<TargetSpaceCtx>("deal 1 damage to each town/city", x=>x.DamageEachInvader(1,Invader.Town,Invader.City))
				.Matches(x=>x.IsOneOf(Terrain.Wetland))
		).Execute( ctx );
	}

}
namespace SpiritIsland.FeatherAndFlame;

internal class FoundationsSinkIntoMud {

	const string Name = "Foundations Sink Into Mud";

	[SpiritCard( Name, 1, Element.Water, Element.Earth ),Slow,FromPresence( 0 )]
	[Instructions( "2 damage to Town. If target land is Wetland, you may instead deal 1 Damage to each Town / City." ), Artist( Artists.DamonWestenhofer )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return Cmd.Pick1(
			// 2 Damage to Town.
			new SpaceCmd("2 damage to towns", x=>x.DamageInvaders(2,Human.Town)),
			// If target land is wetland, you may instead deal 1 Damage to each town / City
			new SpaceCmd("deal 1 damage to each town/city", x=>x.DamageEachInvader(1,Human.Town_City))
				.OnlyExecuteIf(x=>x.IsOneOf(Terrain.Wetland))
		).Execute( ctx );
	}

}
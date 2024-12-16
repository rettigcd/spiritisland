namespace SpiritIsland.Basegame;

[InnatePower( Name ),Slow, FromPresence( 0 )]
[RepeatIf("2 moon,3 plant","3 moon,4 plant")]
public class CreepersTearIntoMortar {
	public const string Name = "Creepers Tear into Mortar";

	[InnateTier( "1 moon,2 plant", "1 Damage to 1 town/city." )]
	static public Task Option1Async( TargetSpaceCtx ctx ) {
		return ctx.DamageInvaders(1,Human.Town_City);
	}

}
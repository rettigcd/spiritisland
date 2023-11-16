namespace SpiritIsland.JaggedEarth;

[InnatePower("Savage Revenge"), Slow]
[ExtendableRange( From.Presence, 0, "3 air", 1, Target.Town, Target.City )]
public class SavageRevenge {

	[InnateTier("3 fire,1 animal","1 Damage")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		return ctx.DamageInvaders(1);
	}

	[InnateTier("4 fire,2 animal","+2 Damage")]
	static public Task Option2(TargetSpaceCtx ctx ) {
		return ctx.DamageInvaders(1+2);
	}

	[InnateTier("5 fire,2 air,2 animal","+3 Damage")]
	static public Task Option3(TargetSpaceCtx ctx ) {
		return ctx.DamageInvaders(1+2+3);
	}

}
namespace SpiritIsland.Horizons;

[InnatePower(Name)]
[Slow,FromSacredSite(1,Filter.Any)]
public class ScorchWithWavesOfHeat {

	public const string Name = "Scorch with Waves of Heat";

	[InnateTier("2 fire,2 air","2 Damage to Explorer only.",0)]
	static public Task Option1(TargetSpaceCtx ctx) {
		return ctx.DamageInvaders(2,Human.Explorer);
	}

	[InnateTier("3 fire,2 earth","2 Damage",1)]
	static public Task Option2( TargetSpaceCtx ctx ) {
		return ctx.DamageInvaders(2);
	}

	[InnateTier("4 fire,1 air,3 earth", "2 Damage", 2)]
	static public Task Option3(TargetSpaceCtx ctx) {
		return ctx.DamageInvaders(2);
	}

	[InnateTier("5 fire,2 air,3 earth", "1 Damage to each Invader", 3)]
	static public Task Option4(TargetSpaceCtx ctx) {
		return ctx.DamageEachInvader(1);
	}

}

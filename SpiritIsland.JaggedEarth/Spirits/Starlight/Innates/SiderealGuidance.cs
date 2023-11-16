namespace SpiritIsland.JaggedEarth;

[InnatePower("Sidereal Guidance"), Slow, FromPresence(1)]
class SiderealGuidance{

	[InnateTier("2 moon","Gather up to 1 explorer/dahan.")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		return ctx.GatherUpTo(1,Human.Dahan,Human.Explorer);
	}

	[InnateTier("3 moon","Instead, Gather up to 3 explorer.")]
	static public Task Option2(TargetSpaceCtx ctx ) {
		return ctx.GatherUpTo( 3, Human.Explorer );
	}

}
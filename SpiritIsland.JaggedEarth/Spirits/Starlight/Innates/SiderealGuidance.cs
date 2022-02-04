namespace SpiritIsland.JaggedEarth;

[InnatePower("Sidereal Guidance"), Slow, FromPresence(1)]
class SiderealGuidance{

	[InnateOption("2 moon","Gather up to 1 explorer/dahan.")]
	static public Task Option1(TargetSpaceCtx ctx ) {
		return ctx.GatherUpTo(1,TokenType.Dahan,Invader.Explorer);
	}

	[InnateOption("3 moon","Instead, Gather up to 3 explorer.")]
	static public Task Option2(TargetSpaceCtx ctx ) {
		return ctx.GatherUpTo( 3, Invader.Explorer );
	}

}
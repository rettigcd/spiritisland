namespace SpiritIsland.Basegame;

public class CallToBloodshed {

	[MinorCard("Call to Bloodshed",1,Element.Sun,Element.Fire,Element.Animal)]
	[Slow]
	[FromPresence(2,Target.Dahan)]
	static public Task Act(TargetSpaceCtx ctx){
		return ctx.SelectActionOption(
			new SpaceAction( "1 damage per dahan", ctx => ctx.DamageInvaders( ctx.Dahan.Count ) ),
			new SpaceAction( "gather up to 3 dahan", ctx => ctx.GatherUpToNDahan( 3 ) )
		);
	}
}
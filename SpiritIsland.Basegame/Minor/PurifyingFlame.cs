namespace SpiritIsland.Basegame;

public class PurifyingFlame {

	[MinorCard("Purifying Flame",1,Element.Sun,Element.Fire,Element.Air,Element.Plant)]
	[Slow]
	[FromSacredSite(1)]
	static public Task Act(TargetSpaceCtx ctx){

		int blightCount = ctx.BlightOnSpace;
		return ctx.SelectActionOption(
			new SpaceAction($"{blightCount} damage", ctx=>ctx.DamageInvaders(blightCount) )
				.OnlyExecuteIf( x => x.Blight.Any ),
			new SpaceAction("Remove 1 blight", ctx=>ctx.RemoveBlight() )
				.OnlyExecuteIf( x=>x.Blight.Any && x.IsOneOf( Terrain.Mountain, Terrain.Sand ) )
		);

	}

}
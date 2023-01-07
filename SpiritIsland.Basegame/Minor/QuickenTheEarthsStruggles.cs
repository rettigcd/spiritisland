namespace SpiritIsland.Basegame;

public class QuickenTheEarthsStruggles {

	[MinorCard( "Quicken the Earths Struggles", 1, "moon, fire, earth, animal" )]
	[Fast]
	[FromSacredSite( 0 )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction( "1 damage to each town/city", ctx => ctx.DamageEachInvader( 1, Invader.Town_City ) ),
			new SpaceAction( "defend 10", ctx => ctx.Defend( 10 ) )
		);

	}

}
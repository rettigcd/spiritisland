namespace SpiritIsland.Basegame;

public class QuickenTheEarthsStruggles {

	[MinorCard( "Quicken the Earth's Struggles", 1, "moon, fire, earth, animal" ),Fast,FromSacredSite( 0 )]
	[Instructions( "1 Damage to each Town / City. -or- Defend 10." ), Artist( Artists.LucasDurham )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction( "1 damage to each town/city", ctx => ctx.DamageEachInvader( 1, Human.Town_City ) ),
			new SpaceAction( "defend 10", ctx => ctx.Defend( 10 ) )
		);

	}

}
namespace SpiritIsland.Basegame;

public class StemTheFlowOfFreshWater {

	[SpiritCard( "Stem the Flow of Fresh Water", 0, Element.Water, Element.Plant ), Slow, FromSacredSite( 1 )]
	[Instructions("1 Damage to 1 Town / City. If target land is Mountain / Sands, instead, 1 Damage to each Town / City."), Artist( Artists.JorgeRamos )]
	static public async Task ActionAsync( TargetSpaceCtx ctx ) {

		// If target land is mountain or sand, 
		if( ctx.IsOneOf( Terrain.Mountain, Terrain.Sand ) ) {
			// instead 1 damange to EACH town/city
			await ctx.DamageEachInvader(1, Human.Town_City);
		} else {
			await ctx.DamageInvaders( 1, Human.Town_City );
		}

	}

}
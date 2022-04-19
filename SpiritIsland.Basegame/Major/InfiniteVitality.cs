namespace SpiritIsland.Basegame;

public class InfiniteVitality {

	[MajorCard( "Infinite Vitality", 3, Element.Earth, Element.Plant, Element.Animal )]
	[Fast]
	[FromSacredSite( 1 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		if( await ctx.YouHave( "4 earth" )) {

			// !!! This should stop ALL blight added to this land, not just RAVAGE blight.

			// !!! also - this only stops dahan destruction during RAVAGE.  Needs to protect from power cards too.

			ctx.ModifyRavage( cfg => {
				// whenever blight would be added to target land, instead leave it on the card
				cfg.ShouldDamageLand = false;
				// dahan ignore damage and destruction effects, 
				cfg.ShouldDamageDahan = false;
			} );

			await RemoveBlightFromLandOrAdjacent( ctx );

		} else {

			ctx.ModifyRavage( cfg => {
				// whenever blight would be added to target land, instead leave it on the card
				cfg.ShouldDamageLand = false;
			} );

			// dahan have +4 health while in target land. (If played twice, would increase dahan health to +8)
			await DahanHelper.BoostDahanHealthForRound( ctx, 4 );
		}

	}

	static async Task RemoveBlightFromLandOrAdjacent( TargetSpaceCtx ctx ) {
		// remove 1 blight from target or adjacent land
		var blightedLands = ctx.Space.Range( 1 ).Where( s=>ctx.Target(s).HasBlight ).ToArray();
		var unblightLand = await ctx.Decision( new Select.Space( "Remove 1 blight from", blightedLands, Present.Always ));
		if(unblightLand != null)
			await ctx.Target( unblightLand ).RemoveBlight();
	}

}
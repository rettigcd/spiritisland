namespace SpiritIsland.Basegame;

public class InfiniteVitality {

	[MajorCard( "Infinite Vitality", 3, Element.Earth, Element.Plant, Element.Animal )]
	[Fast]
	[FromSacredSite( 1 )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		// Dahan have +4 health while in target land.
		await ctx.AdjustTokensHealthForRound( 4, TokenType.Dahan );

		// whenever blight would be added to target land, instead leave it on the card
		ctx.Blight.Blocked = true;

		if( await ctx.YouHave( "4 earth" )) {
			// Dahan ignore damage and destruction effects.
			ctx.ModifyRavage( behavior => behavior.DamageDefenders = ( _, _1, _2 ) => Task.CompletedTask ); // !!! this only stops dahan destruction during RAVAGE.  Needs to protect from power cards too.
			// Remove 1 blight from target or adjacent
			await RemoveBlightFromLandOrAdjacent( ctx );
		}

	}

	static async Task RemoveBlightFromLandOrAdjacent( TargetSpaceCtx ctx ) {
		// remove 1 blight from target or adjacent land
		var blightedLands = ctx.Tokens.InOrAdjacentTo.Where( s=>s.Blight.Any ).ToArray();
		var unblightLand = await ctx.Decision( new Select.Space( "Remove 1 blight from", blightedLands, Present.Always ));
		if(unblightLand != null)
			await ctx.Target( unblightLand ).RemoveBlight();
	}

}
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class InfiniteVitality {

		[MajorCard( "Infinite Vitality", 3, Speed.Fast, Element.Earth, Element.Plant, Element.Animal )]
		[FromSacredSite( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			ctx.GameState.ModRavage( ctx.Target, cfg => {
				// dahan have +4 health while in target land.
				cfg.DahanHitpoints += 4;
				// whenever blight would be added to target land, instead leave it on the card
				cfg.ShouldDamageLand = false;
				// if you have 4 earth,
				if(ctx.Self.Elements.Contains( "4 earth" ))
					// dahan ignore damage and destruction effects, 
					cfg.ShouldDamageDahan = false;
			} );

			await RemoveBlightFromLandOrAdjacent( ctx );
		}

		static async Task RemoveBlightFromLandOrAdjacent( TargetSpaceCtx ctx ) {
			// remove 1 blight from target or adjacent land
			var blightedLands = ctx.Target.SpacesWithin( 1 ).Where( ctx.GameState.HasBlight ).ToArray();
			var unblightLand = await ctx.Self.SelectSpace( "Remove 1 blight from", blightedLands );
			if(unblightLand != null)
				ctx.GameState.RemoveBlight( unblightLand );
		}
	}

}

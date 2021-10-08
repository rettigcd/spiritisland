using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class InfiniteVitality {

		[MajorCard( "Infinite Vitality", 3, Speed.Fast, Element.Earth, Element.Plant, Element.Animal )]
		[FromSacredSite( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			bool bonus = ctx.YouHave( "4 earth" );

			ctx.ModifyRavage( cfg => {
				// dahan have +4 health while in target land.
				cfg.DahanHitpoints += 4;

				// whenever blight would be added to target land, instead leave it on the card
				cfg.ShouldDamageLand = false;

				// if you have 4 earth,
				if(bonus)
					// dahan ignore damage and destruction effects, 
					cfg.ShouldDamageDahan = false;
			} );

			if(bonus)
				await RemoveBlightFromLandOrAdjacent( ctx );
		}

		static async Task RemoveBlightFromLandOrAdjacent( TargetSpaceCtx ctx ) {
			// remove 1 blight from target or adjacent land
			var blightedLands = ctx.Space.Range( 1 ).Where( s=>ctx.Target(s).HasBlight ).ToArray();
			var unblightLand = await ctx.Self.Action.Decision( new Decision.TargetSpace( "Remove 1 blight from", blightedLands, Present.Always ));
			if(unblightLand != null)
				ctx.Target( unblightLand ).RemoveBlight();
		}
	}

}

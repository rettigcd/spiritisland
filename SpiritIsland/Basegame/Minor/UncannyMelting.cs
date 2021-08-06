using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class UncannyMelting {

		public const string Name = "Uncanny Melting";

		[MinorCard(UncannyMelting.Name,1, Speed.Slow,Element.Sun,Element.Moon,Element.Water)]
		[FromSacredSite(1,Filter.None)] // !!! must include invaders, add unit test to make sure we don't accidentally switch 
		static public void ActAsync(ActionEngine eng,Space target){
			var (_,gameState) = eng;

//!!!!!!!  Sometimes cards have not targetable land and target==null
// when that happens, don't call this....

			// Invaders
			if(gameState.HasInvaders(target))
				gameState.AddFear(1);

			// !!! unit test - requires Sand / Water
			if(gameState.HasBlight(target) && target.Terrain.IsIn(Terrain.Wetland,Terrain.Sand))
				gameState.RemoveBlight(target);

		}

	}

}

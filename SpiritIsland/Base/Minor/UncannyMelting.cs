using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class UncannyMelting {

		public const string Name = "Uncanny Melting";

		[MinorCard(UncannyMelting.Name,1, Speed.Slow,Element.Sun,Element.Moon,Element.Water)]
		[FromSacredSite(1,Filter.SandOrWetland)]
		static public void ActAsync(ActionEngine eng,Space target){
			var (_,gameState) = eng;
			if(gameState.HasInvaders(target))
				gameState.AddFear(1);

			if(gameState.HasBlight(target))
				gameState.RemoveBlight(target);

		}

	}

}

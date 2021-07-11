using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(HarbingersOfTheLightning.Name,0,Speed.Slow,Element.Fire,Element.Air)]
	public class HarbingersOfTheLightning : BaseAction {
		public const string Name = "Harbingers of the Lightning";

		public HarbingersOfTheLightning(Spirit spirit,GameState gameState)
			:base(spirit,gameState)
		{
			_ = ActionAsync(spirit);
		}

		async Task ActionAsync(Spirit spirit){
			Space source = await engine.SelectSpace("Select target."
				,spirit.SacredSites.Range(1).Where(gameState.HasDahan)
			);

			// Push up to 2 dahan.

			// count dahan on neighbors prior to action
			var landsWithBuildings = source.Neighbors
				.Where(neighbor => {
					var grp = gameState.InvadersOn(neighbor);
					return grp.HasTown || grp.HasCity;
				})
				.ToDictionary(n=>n,n=>gameState.GetDahanOnSpace(n));

			int dahanToPush = Math.Min(2,gameState.GetDahanOnSpace(source));
			while(dahanToPush>0){
				Space destination = await engine.SelectSpace("Select destination for dahan"
					,source.Neighbors
					,true
				);
				gameState.AddDahan(source,-1);
				gameState.AddDahan(destination,1);
				--dahanToPush;
			}

			bool pushedToBuildingSpace = landsWithBuildings
				.Any(pair=>gameState.GetDahanOnSpace(pair.Key)>pair.Value);
			if(pushedToBuildingSpace)
				gameState.AddFear(1);
		}



	}
}

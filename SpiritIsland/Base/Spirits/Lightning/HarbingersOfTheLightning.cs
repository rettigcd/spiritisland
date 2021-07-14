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
			_ = ActionAsync();
		}

		async Task ActionAsync(){
			Space source = await engine.Api.TargetSpace_SacredSite(1,gameState.HasDahan);

			// Push up to 2 dahan.

			// count dahan on neighbors prior to action
			var landsWithBuildings = source.Neighbors
				.Where(neighbor => {
					var grp = gameState.InvadersOn(neighbor);
					return grp.HasTown || grp.HasCity;
				})
				.ToDictionary(n=>n,n=>gameState.GetDahanOnSpace(n));

			await engine.PushUpToNDahan(source,2);

			bool pushedToBuildingSpace = landsWithBuildings
				.Any(pair=>gameState.GetDahanOnSpace(pair.Key)>pair.Value);
			if(pushedToBuildingSpace)
				gameState.AddFear(1);
		}



	}
}

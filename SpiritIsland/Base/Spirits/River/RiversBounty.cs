using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class RiversBounty {

		public const string Name = "River's Bounty";
		[SpiritCard(RiversBounty.Name, 0, Speed.Slow,Element.Sun,Element.Water,Element.Animal)]
		static public async Task ActionAsync(ActionEngine engine, Spirit self,GameState gameState){
			bool HasCloseDahan(Space space)=> space.SpacesWithin(1).Any( gameState.HasDahan );
			var target = await engine.SelectSpace(
				"Select target space."
				,self.Presence.Range(0).Where(HasCloseDahan)
			);

			var ctx = new GatherDahanCtx(target,gameState);

			// Gather up to 2 Dahan
			int dahanToGather = 2;
			while(dahanToGather>0 && ctx.neighborCounts.Keys.Any()){
				Space source = await engine.SelectSpace(
					"Select source land to gather Dahan into "+ctx.Target.Label,
					ctx.neighborCounts.Keys,true
				);
				if(source == null) break;

				if(source != ctx.Target){
					++ctx.DestinationCount;
					--ctx.neighborCounts[source];
				}
				new MoveDahan(source,ctx.Target).Apply(gameState);
				--dahanToGather;
			}

			// If there are now at least 2 dahan, then add 1 dahan and gain 1 energy
			if(ctx.DestinationCount>=2){
				gameState.AddDahan(ctx.Target,1);
				++self.Energy;
			}
		}


	}

}




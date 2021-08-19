﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class NaturesResilience {

		public const string Name = "Nature's Resilience";

		[MinorCard(NaturesResilience.Name,1,Speed.Fast,Element.Earth,Element.Plant,Element.Animal)]
		[FromSacredSite(1)]
		static public async Task Act(TargetSpaceCtx ctx){
			var target = ctx.Target;
			var (self,gameState) = ctx;
			// if 2 water, you may INSTEAD remove 1 blight
			bool removeBlight = self.Elements[Element.Water]>=2
				&& await ctx.Self.SelectFirstText("Select option", "Remove Blight", "Defend 6" );

			if(removeBlight)
				gameState.RemoveBlight(target);
			else 
				gameState.Defend(target,6);

		}

	}
}

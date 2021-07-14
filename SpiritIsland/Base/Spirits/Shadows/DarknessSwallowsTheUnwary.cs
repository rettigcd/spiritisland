using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[InnatePower(DarknessSwallowsTheUnwary.Name,Speed.Fast)]
	[PowerLevel(0,Element.Sun,Element.Water,Element.Water)]
	class DarknessSwallowsTheUnwary : BaseAction {
		public DarknessSwallowsTheUnwary(Spirit spirit,GameState gs):base(spirit,gs){
			_ = ActAsync();	       
		}

		async Task ActAsync(){
			int moon = engine.Self.Elements(Element.Moon);
			int fire = engine.Self.Elements(Element.Fire);
			int air  = engine.Self.Elements(Element.Air);

			// level 1 - 2 moon, 1 fire: gather 1 explorer
			if(!(2<=moon && 1<=fire)) return;
			// range 1 from SS
			var target = await engine.Api.TargetSpace_SacredSite(1);
			await engine.GatherExplorer(target,1);

			// level 2 - 3 moon, 2 fire: destroy up to 2 explorers, 1 fear / explorer destroyed
			if(3<=moon && 2<=fire){
				var grp = engine.GameState.InvadersOn(target);
				int explorersToDestroy = Math.Min(grp[Invader.Explorer],2);
				grp[Invader.Explorer] -= explorersToDestroy;
				engine.GameState.UpdateFromGroup(grp);
				engine.GameState.AddFear(explorersToDestroy);
			}

			// level 3 - 4 moon, 3 fire, 2 air: 3 damage, 1 fear / invader destroyed by this damage
			if(4<=moon && 3<=fire && 2<=air){
				int startingCount = engine.GameState.InvadersOn(target).Total;
				engine.GameState.DamageInvaders(target,3);
				int endingCount = engine.GameState.InvadersOn(target).Total;
				int killed = startingCount - endingCount;
				engine.GameState.AddFear(killed);
			}
		} 

		public const string Name = "Darkness Swallows the Unwary";

	}

}

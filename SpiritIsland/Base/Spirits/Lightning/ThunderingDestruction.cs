using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	// Innate:  Thundering Destruction => slow, 1 from sacred, any
	// 3 fire 2 air    destroy 1 town
	// 4 fire 3 air    you may instead destroy 1 city
	// 5 fire 4 air 1 water    also, destroy 1 town or city
	// 5 fire 5 air 2 water    also, destroy 1 town or city

	[Core.InnatePower( ThunderingDestruction.Name, Speed.Fast )]
	[Core.InnateOption( Element.Fire, Element.Fire, Element.Fire, Element.Air, Element.Air )]
	class ThunderingDestruction : BaseAction {

		public const string Name = "Thundering Destruction";

		readonly Spirit lightning;

		public ThunderingDestruction(Spirit lightning,GameState gs)
			:base(lightning,gs)
		{
			this.lightning = lightning;

			int fire  = lightning.Elements(Element.Fire);
			int air   = lightning.Elements(Element.Air);
			int water = lightning.Elements(Element.Water);

			if(fire >=5 && air>=5 && water>=2)
				_ = DestroyTownsOrCities(3);
			else if(fire >=5 && air>=4 && water>=1)
				_ = DestroyTownsOrCities(2);
			else if(fire >=4 && air>=3)
				_ = DestroyTownsOrCities(1);
			else if(fire >=3 && air>=2)
				_ = DestroyTowns(1);
		}

		async Task DestroyTowns(int count){
			bool HasTown(Space space)=>gameState.InvadersOn(space).HasTown;
			var target = await engine.SelectSpace("Select target",
				lightning.SacredSites.Range(1).Where(HasTown)
			);
			var grp = gameState.InvadersOn(target);
			var invadersToDestroy = grp.Filter("T@2","T@1");
			while(count>0 && invadersToDestroy.Length >0){
				var invader = await engine.SelectInvader("Select town/city to destroy.",invadersToDestroy,true);
				if(invader==null) break;
				grp.ApplyDamage(new DamagePlan(invader.Health,invader));

				// next
				invadersToDestroy = grp.Filter("T@2","T@1");
				--count;
			}
			gameState.UpdateFromGroup(grp);
		}

		async Task DestroyTownsOrCities(int count){

			bool HasTownOrCity(Space space){ 
				var grp = gameState.InvadersOn(space);
				return grp.HasTown || grp.HasCity;
			}

			var target = await engine.SelectSpace(
				"Select target",
				lightning.SacredSites.Range(1).Where(HasTownOrCity)
			);
			var grp = gameState.InvadersOn(target);
			var invadersToDestroy = grp.Filter("C@3","C@2","C@1","T@2","T@1");
			while(count>0 && invadersToDestroy.Length >0){
				var invader = await engine.SelectInvader("Select town/city to destroy.",invadersToDestroy,true);
				if(invader==null) break;
				grp.ApplyDamage(new DamagePlan(invader.Health,invader));

				// next
				invadersToDestroy = grp.Filter("C@3","C@2","C@1","T@2","T@1");
				--count;
			}
			gameState.UpdateFromGroup(grp);
		}

	}

}

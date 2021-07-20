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

	[InnatePower( ThunderingDestruction.Name, Speed.Fast )]
	[FromSacredSite(1)]
	class ThunderingDestruction {

		public const string Name = "Thundering Destruction";

		[InnateOption( Element.Fire, Element.Fire, Element.Fire, Element.Air, Element.Air )]
		static public async Task ActAsync(ActionEngine engine,Space target){
			var (lightning,_) = engine;
			int fire  = lightning.Elements(Element.Fire);
			int air   = lightning.Elements(Element.Air);
			int water = lightning.Elements(Element.Water);

			if(fire >=5 && air>=5 && water>=2)
				_ = DestroyTownsOrCities(engine,target,3);
			else if(fire >=5 && air>=4 && water>=1)
				_ = DestroyTownsOrCities(engine,target,2);
			else if(fire >=4 && air>=3)
				_ = DestroyTownsOrCities(engine,target,1);
			else if(fire >=3 && air>=2)
				_ = DestroyTowns(engine,target,1);
		}

		static async Task DestroyTowns(ActionEngine engine, Space target, int count){
			var gameState = engine.GameState;
			var grp = gameState.InvadersOn(target);
			var invadersToDestroy = grp.FilterBy(Invader.Town);
			while(count>0 && invadersToDestroy.Length >0){
				var invader = await engine.SelectInvader("Select town/city to destroy.",invadersToDestroy,true);
				if(invader==null) break;
				grp.ApplyDamage(new DamagePlan(invader.Health,invader));

				// next
				invadersToDestroy = grp.FilterBy(Invader.Town);
				--count;
			}
			gameState.UpdateFromGroup(grp);
		}

		static async Task DestroyTownsOrCities(ActionEngine engine,Space target,int count){
			var gameState = engine.GameState;
			var grp = gameState.InvadersOn(target);
			var invadersToDestroy = grp.FilterBy(Invader.City,Invader.Town);
			while(count>0 && invadersToDestroy.Length >0){
				var invader = await engine.SelectInvader("Select town/city to destroy.",invadersToDestroy,true);
				if(invader==null) break;
				grp.ApplyDamage(new DamagePlan(invader.Health,invader));

				// next
				invadersToDestroy = grp.FilterBy(Invader.City,Invader.Town);
				--count;
			}
			gameState.UpdateFromGroup(grp);
		}

	}

}

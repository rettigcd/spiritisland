﻿using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	[InnatePower(MassiveFlooding.Name,Speed.Slow)]
	[FromSacredSite(1,Filter.TownOrExplorer)]
	public class MassiveFlooding {

		public const string Name = "Massive Flooding";

		[InnateOption(Element.Sun,Element.Water,Element.Water)]
		static public Task Option1Async(ActionEngine engine,Space target){
			// Push 1 Town/Explorer
			return engine.PushUpToNInvaders(target,1,Invader.Town,Invader.Explorer); 
		}

		[InnateOption(Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water)]
		static public async Task Option2Async(ActionEngine engine,Space target){
			engine.GameState.DamageInvaders(target,2);
			await engine.PushUpToNInvaders(target,3,Invader.Town,Invader.Explorer);
		}

		[InnateOption(Element.Sun,Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water,Element.Water,Element.Earth)]
		static public Task Option3Async(ActionEngine engine,Space target){
			var group = engine.GameState.InvadersOn(target);

			var invaderTypes = group.InvaderTypesPresent.ToDictionary(x=>x,x=>group[x]); // copy so we can modify
			foreach(var (invader,origCount) in invaderTypes.Select(x=>(x.Key,x.Value))){
				for(int i=0;i<origCount;++i)
					group.ApplyDamage( invader, 2 );
			}

			group.Commit();
			return Task.CompletedTask;
		}

	}

}

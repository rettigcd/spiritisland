using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland {

	[InnatePower(MassiveFlooding.Name,Speed.Slow)]
	public class MassiveFlooding : BaseAction {
		// Slow, range 1 from SS

		public const string Name = "Massive Flooding";

		public MassiveFlooding(Spirit spirit,GameState gameState):base(gameState){

			var elements = spirit.AllElements;

			count = new int[]{
				elements[Element.Sun],
				elements[Element.Water]-1,
				elements[Element.Earth]==0?2:3
			}.Min();

			if(count == 0) return;

			engine.decisions.Push( new TargetSpaceRangeFromSacredSite(spirit,1,
				HasExplorersOrTowns
				,SelectLevel
			));

		}

		bool HasExplorersOrTowns(Space space){
			var sum = gameState.InvadersOn(space);
			return sum.HasExplorer || sum.HasTown;
		}

		readonly int count;
		Space space;

		void SelectLevel(Space space){
			this.space = space;
			var invaders = gameState.InvadersOn(space);

			const string k1 = "Push 1 E/T";
			const string k2 = "2 damage, Push up to 3 explorers and/or towns";
			const string k3 = "2 damage to all";

			IEnumerable<string> d = new string[]{ k1, k2, k3 }.Take(count);

			var dict = new Dictionary<string,Action>{
				[k1] = Option1,
				[k2] = Option2,
				[k3] = Option3,
			};

			// Add Selection Decision
			engine.decisions.Push( new SelectText(engine,d,(string option,ActionEngine engine)=>{
				dict[option]();
			}) );
		}

		void Option1(){
			var invaders = gameState.InvadersOn(space);
			engine.decisions.Push( new SelectInvadersToPush(engine,invaders,1,"Town","Explorer") );
		}

		void Option2(){
			// * 2 sun, 3 water => Instead, 2 damage, Push up to 3 explorers and/or towns
			gameState.DamageInvaders(space,2);
			engine.decisions.Push( new SelectInvadersToPush(engine, gameState.InvadersOn(space),3,"Town","Explorer") );
		}

		void Option3(){
			//` * 3 sun, 4 water, 1 earth => Instead, 2 damage to each invader
			var group = gameState.InvadersOn(space);
			var invaderTypes = group.InvaderTypesPresent.ToArray(); // copy so we can modify
			foreach(var invader in invaderTypes){
				// add the damaged invaders
				group[ invader.Damage(2) ] += group[invader];
				// clear the healthy invaders
			}
			gameState.UpdateFromGroup(group);
		}


	}

}

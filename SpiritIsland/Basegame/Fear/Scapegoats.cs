﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class Scapegoats : IFearCard {

		//"Each Town destroys 1 Explorer in its land.", 
		public Task Level1( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces) {
				var grp = gs.InvadersOn(space);
				if(EachTownDestroys1Explorer(grp))
					gs.UpdateFromGroup(grp);
			}
			// !! not unit tested
			return Task.CompletedTask;
		}

		//"Each Town destroys 1 Explorer in its land. Each City destroys 2 Explorer in its land.", 
		public Task Level2( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces) {
				var grp = gs.InvadersOn( space );
				if(EachTownDestroys1AndEachCityDestoys2( grp ))
					gs.UpdateFromGroup( grp );
			}
			return Task.CompletedTask;
		}

		//"Destroy all Explorer in lands with Town / City. Each City destroys 1 Town in its land."),
		public Task Level3( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces) {
				var grp = gs.InvadersOn( space );
				bool destoringExplorers = grp[Invader.Explorer] > 0;
				grp.DestroyAll(Invader.Explorer);
				bool destroyedTowns = EachCityDestroys1Town( grp );
				
				if(destroyedTowns || destoringExplorers)
					gs.UpdateFromGroup( grp );
			}
			return Task.CompletedTask;
		}

		static bool EachCityDestroys1Town( InvaderGroup grp ) {
			int destroyCount = grp[Invader.City] + grp[Invader.City2] + grp[Invader.City1];
			int healthyToDestroy = Math.Min( destroyCount, grp[Invader.Town] );
			int damagedToDestory = Math.Min( destroyCount - healthyToDestroy, grp[Invader.Town1] );
			// healthy - Move into Group
			if(healthyToDestroy > 0)
				grp.Destroy( Invader.Town, healthyToDestroy );
//				grp[Invader.Town] -= healthyToDestroy;
			// damaged
			if(damagedToDestory > 0)
				grp.Destroy( Invader.Town1, damagedToDestory );
//			grp[Invader.Town1] -= damagedToDestory;
			return damagedToDestory + healthyToDestroy > 0; // needs saved
		}

		static bool EachTownDestroys1Explorer( InvaderGroup grp ) {
			int numToDestory = Math.Min(
				grp[Invader.Town] + grp[Invader.Town1],
				grp[Invader.Explorer]
			);
			if(numToDestory == 0) return false;
			grp.Destroy(Invader.Explorer, numToDestory );
			return true;
		}

		static bool EachTownDestroys1AndEachCityDestoys2( InvaderGroup grp ) {
			int cityCount = grp[Invader.City] + grp[Invader.City2] + grp[Invader.City1];
			int townCount = grp[Invader.Town] + grp[Invader.Town1];
			int numToDestory = Math.Min(
				townCount + cityCount * 2,
				grp[Invader.Explorer]
			);
			if(numToDestory == 0) return false;
			grp.Destroy(Invader.Explorer, numToDestory );
			return true;
		}


	}
}


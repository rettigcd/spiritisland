using System;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(HarbingersOfTheLightning.Name,0,Speed.Slow,Element.Fire,Element.Air)]
	public class HarbingersOfTheLightning : TargetSpaceAction {
		public const string Name = "Harbingers of the Lightning";

		public HarbingersOfTheLightning(Spirit spirit,GameState gameState)
			:base(spirit,gameState,1,From.SacredSite)
		{}

		protected override bool FilterSpace( Space space ) {
			return gameState.HasDahan( space );
		}

		protected override void SelectSpace( Space space ) {

			// Push up to 2 dahan.
			// if you pushed any dahan into a land with town or city -
			//		then 1 fear.

			// count dahan on neighbors prior to action
			var landsWithBuildings = space.SpacesExactly(1)
				.Where(neighbor => {
					var grp = gameState.InvadersOn(neighbor);
					return grp.HasTown || grp.HasCity;
				})
				.ToDictionary(n=>n,n=>gameState.GetDahanOnSpace(n));

			// ending check
			engine.actions.Add(new SimpleAction(()=>{
				bool pushedToBuildingSpace = landsWithBuildings
					.Any(pair=>gameState.GetDahanOnSpace(pair.Key)>pair.Value);
				if(pushedToBuildingSpace)
					gameState.AddFear(1);
			} ));

			int count = Math.Min(2,gameState.GetDahanOnSpace( space ));
			engine.decisions.Push(new SelectPushedDahanDestination(engine,space,gameState,count,true));
		}

	}
}

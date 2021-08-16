using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ShadowsOfTheBurningForest {

		[MinorCard("Shadows of the Burning Forest",0,Speed.Slow,Element.Moon,Element.Fire,Element.Plant)]
		[FromPresence(0,Target.Invaders)]
		static public async Task Act(ActionEngine engine,Space target){
			// 2 fear
			engine.GameState.AddFear(2);

			// if target is M/J, Push 1 explorer and 1 town

			// duplicated in Mantle of dread
			var grp = engine.GameState.InvadersOn(target);
			// Push Town
			var townInvaders = grp.FilterBy(Invader.Town);
			if(townInvaders.Length>0){
				var townInvader = await engine.SelectInvader("Select town to push",townInvaders,true);
				if(townInvader != null)
					await engine.PushInvader(target,townInvader);
			}
			// Push Explorer
			if(grp.HasExplorer){
				// allow short-circuit
				var explorerInvader = await engine.SelectInvader("Select town to push",grp.FilterBy(Invader.Explorer),true);
				if(explorerInvader != null)
					await engine.PushInvader(target,explorerInvader);
			}

		}

	}
}

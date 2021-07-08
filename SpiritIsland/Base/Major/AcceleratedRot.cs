using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[MajorCard(AcceleratedRot.Name,4,Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
	public class AcceleratedRot : BaseAction {

		public const string Name = "Accelerated Rot";

		public Spirit spirit;

		public AcceleratedRot(Spirit spirit,GameState gs):base(gs){
			this.spirit = spirit;

			// range 2, Jungle or Wetland
			engine.decisions.Push(new SelectSpaceGeneric(
				"Select target space."
				,spirit.Presence.Range(2).Where(JungleOrWetland)
				,SelectSpace
			));


		}

		static bool JungleOrWetland(Space space)=>space.Terrain.IsIn(Terrain.Jungle,Terrain.Wetland);

		void SelectSpace(Space space){
			// 2 fear, 4 damage
			int damageToInvaders = 4;
			gameState.AddFear(2);

			if(spirit.HasElements(
				Element.Sun,Element.Sun,Element.Sun,
				Element.Water,Element.Water,
				Element.Plant,Element.Plant,Element.Plant
			)){
				// +5 damage, remove 1 blight
				damageToInvaders += 5;
				gameState.AddBlight(space,-1);
			}				

			gameState.DamageInvaders(space, damageToInvaders);
		}

	}

}

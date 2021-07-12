using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[MajorCard(AcceleratedRot.Name,4,Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
	public class AcceleratedRot : BaseAction {

		public const string Name = "Accelerated Rot";

		public AcceleratedRot(Spirit spirit,GameState gs):base(spirit,gs){
			_=ActAsync(spirit);
		}

		async Task ActAsync(Spirit spirit){
			static bool JungleOrWetland(Space space)=>space.Terrain.IsIn(Terrain.Jungle,Terrain.Wetland);
			var space = await engine.TargetSpace_Presence(2,JungleOrWetland);
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

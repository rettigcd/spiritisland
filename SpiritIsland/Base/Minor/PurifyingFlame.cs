using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class PurifyingFlame {

		[MinorCard("Purifying Flame",1,Speed.Slow,Element.Sun,Element.Fire,Element.Air,Element.Plant)]
		static public async Task Act(ActionEngine engine){
			var (spirit,gameState) = engine;

			static bool CanRemoveBlight(Space space) => space.Terrain.IsIn(Terrain.Mountain,Terrain.Sand);
			bool CanDamageOrRemoveBlight(Space space) => gameState.HasBlight(space)
				&& (CanRemoveBlight(space) || gameState.HasInvaders(space));

			// range 1 from SS
			var target = await engine.TargetSpace_SacredSite(1,CanDamageOrRemoveBlight); 
			// !!! there might be 0 spaces that qualify

			async Task<bool> UserSelectsDamage(){
				const string damageInvaderKey = "damageInvaders";
				return damageInvaderKey == await engine.SelectText("Select option",damageInvaderKey,"remove blight");
			}

			// on target, spirit should be able to do one or both
			bool doDamage = gameState.HasInvaders(target) && (!CanRemoveBlight(target) || await UserSelectsDamage());
			if(doDamage)
				// 1 damage per blight
				gameState.DamageInvaders(target,gameState.GetBlightOnSpace(target));
			else
				// if target land is M/S, you may INSTEAD remove 1 blight
				gameState.AddBlight(target,-1);

		}

	}

}

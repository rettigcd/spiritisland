using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class PurifyingFlame {

		[MinorCard("Purifying Flame",1,Speed.Slow,Element.Sun,Element.Fire,Element.Air,Element.Plant)]
		[FromSacredSite(1)]
		static public async Task Act(TargetSpaceCtx ctx){
			var (self,gameState) = ctx;
			var target = ctx.Target;

			static bool CanRemoveBlight(Space space) => space.Terrain.IsIn(Terrain.Mountain,Terrain.Sand);

			async Task<bool> UserSelectsDamage(){
				const string damageInvaderKey = "damageInvaders";
				return damageInvaderKey == await self.SelectText("Select option",damageInvaderKey,"remove blight");
			}

			// on target, spirit should be able to do one or both
			bool doDamage = gameState.HasInvaders(target) && (!CanRemoveBlight(target) || await UserSelectsDamage());
			if(doDamage)
				// 1 damage per blight
				await ctx.DamageInvaders(target,gameState.GetBlightOnSpace(target));
			else
				// if target land is M/S, you may INSTEAD remove 1 blight
				gameState.AddBlight(target,-1);

		}

	}

}

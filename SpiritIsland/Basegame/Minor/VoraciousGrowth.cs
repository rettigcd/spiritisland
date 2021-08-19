using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class VoraciousGrowth {

		[MinorCard("Voracious Growth",1,Speed.Slow,Element.Water,Element.Plant)]
		[FromSacredSite(1,Target.JungleOrWetland)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			var (_,gs) = ctx;
			var target = ctx.Target;

			const string blightKey = "Remove 1 Blight";

			bool removeBlight = gs.HasBlight(target) 
				&& blightKey == await ctx.Self.SelectText("Select action","2 Damage",blightKey);

			if(removeBlight)
				gs.AddBlight(target,-1);
			else
				// 2 damage
				await ctx.DamageInvaders(target,2);
		}

	}

}

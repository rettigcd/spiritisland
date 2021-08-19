using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class VoraciousGrowth {

		[MinorCard("Voracious Growth",1,Speed.Slow,Element.Water,Element.Plant)]
		[FromSacredSite(1,Target.JungleOrWetland)]
		static public async Task ActAsync(TargetSpaceCtx ctx){

			bool removeBlight = ctx.HasBlight
				&& !await ctx.Self.UserSelectsFirstText("Select action", "2 Damage","Remove 1 Blight" );

			if(removeBlight)
				ctx.RemoveBlight();
			else
				// 2 damage
				await ctx.DamageInvaders(2);
		}

	}

}

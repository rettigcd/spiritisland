using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class VoraciousGrowth {

		[MinorCard("Voracious Growth",1,Speed.Slow,Element.Water,Element.Plant)]
		[FromSacredSite(1,Target.JungleOrWetland)]
		static public Task ActAsync(TargetSpaceCtx ctx){

			return ctx.SelectActionOption(
				new ActionOption( "2 Damage", () => ctx.DamageInvaders( 2 ), ctx.HasInvaders ),
				new ActionOption( "Remove 1 Blight", () => ctx.RemoveBlight(), ctx.HasBlight )
			);

		}

	}

}

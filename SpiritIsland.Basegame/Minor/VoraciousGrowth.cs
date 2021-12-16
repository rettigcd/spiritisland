using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class VoraciousGrowth {

		[MinorCard("Voracious Growth",1,Element.Water,Element.Plant)]
		[Slow]
		[FromSacredSite(1,Target.JungleOrWetland)]
		static public Task ActAsync(TargetSpaceCtx ctx){

			return ctx.SelectActionOption(
				new SpaceAction( "2 Damage", ctx => ctx.DamageInvaders( 2 ) ).Cond( ctx.HasInvaders ),
				new SpaceAction( "Remove 1 Blight", ctx => ctx.RemoveBlight() ).Cond( ctx.HasBlight )
			);

		}

	}

}

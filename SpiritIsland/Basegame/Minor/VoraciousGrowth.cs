using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class VoraciousGrowth {

		[MinorCard("Voracious Growth",1,Speed.Slow,Element.Water,Element.Plant)]
		[FromSacredSite(1,Target.JungleOrWetland)]
		static public Task ActAsync(TargetSpaceCtx ctx){

			return ctx.SelectPowerOption(
				new PowerOption( "2 Damage", ctx => ctx.DamageInvaders( 2 ), ctx.HasInvaders ),
				new PowerOption( "Remove 1 Blight", ctx => ctx.PushUpToNTokens( 3, TokenType.Dahan ), ctx.Tokens.Has( TokenType.Blight ) )
			);

		}

	}

}

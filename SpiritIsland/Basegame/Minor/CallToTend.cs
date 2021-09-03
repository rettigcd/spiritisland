using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToTend {

		[MinorCard("Call to Tend",1,Speed.Slow,Element.Water,Element.Plant,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {

			return ctx.SelectPowerOption(
				new PowerOption( "remove 1 blight", ctx => ctx.RemoveBlight() ),
				new PowerOption( "push up to 3 dahan", ctx => ctx.PushUpToNTokens( 3, TokenType.Dahan ) )
			);

		}

	}
}

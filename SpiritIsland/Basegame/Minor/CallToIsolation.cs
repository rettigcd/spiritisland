using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallToIsolation {

		[MinorCard("Call to Isolation",0,Speed.Fast,Element.Sun,Element.Air,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public Task Act(TargetSpaceCtx ctx){
			int pushCount = ctx.DahanCount; // push 1 explorer/town per dahan
			return ctx.SelectPowerOption(
				new PowerOption( "push 1 dahan", () => ctx.PushUpToNTokens( 1, TokenType.Dahan ) ),
				new PowerOption( "push {pushCount} explorer or towns", () => ctx.PushUpToNTokens( pushCount, Invader.Town, Invader.Explorer ), pushCount>0 )
			);
		}

	}
}

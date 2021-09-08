using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class HereThereBeMonsters {

		[MinorCard( "Here There Be Monsters", 0, Speed.Slow, Element.Moon, Element.Air, Element.Animal )]
		[FromPresence( 0, Target.Inland )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			ctx.AddFear(2 + (ctx.Tokens.Beasts().Any?1:0));
			return ctx.PushUpToNTokens(1,Invader.Explorer,Invader.Town,TokenType.Dahan);
		}

	}

}

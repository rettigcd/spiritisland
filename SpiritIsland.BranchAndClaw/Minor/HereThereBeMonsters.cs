using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class HereThereBeMonsters {

		[MinorCard( "Here There Be Monsters", 0, Speed.Slow, Element.Moon, Element.Air, Element.Animal )]
		[FromPresence( 0, Target.Inland )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// you may push 1 explorer / town / dahan
			await ctx.PushUpTo(1,Invader.Explorer,Invader.Town,TokenType.Dahan);
			// 2 fear
			ctx.AddFear(2);
			// if target land has any beasts, 1 fear
			if( ctx.Tokens.Beasts().Any )
				ctx.AddFear(1);
		}

	}

}

using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class RitesOfTheLandsRejection {

		[MinorCard( "Rites of the Land's Rejection", 1, Speed.Fast, Element.Moon, Element.Fire, Element.Earth )]
		[FromSacredSite( 2, Target.Dahan )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			void StopBuild_FearForCitiesTownsAndDahan() {
				ctx.GameState.SkipBuild( ctx.Target );

				int cityTownCount = ctx.Tokens.SumAny( Invader.Town, Invader.City );
				ctx.AddFear( Math.Min( ctx.DahanCount, cityTownCount ) );
			}

			return ctx.SelectActionOption(
				new ActionOption( "Stop build, 1 fear / (Dahan+T/C)", StopBuild_FearForCitiesTownsAndDahan ),
				new ActionOption( "Push up to 3 dahan", () => ctx.PushUpToNTokens(3,TokenType.Dahan ))
			);
		}

	}
}

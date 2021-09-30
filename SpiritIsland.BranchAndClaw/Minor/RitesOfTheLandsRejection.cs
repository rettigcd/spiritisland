using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class RitesOfTheLandsRejection {

		public const string Name = "Rites of the Land's Rejection";

		[MinorCard( RitesOfTheLandsRejection.Name, 1, Speed.Fast, Element.Moon, Element.Fire, Element.Earth )]
		[FromSacredSite( 2, Target.Dahan )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			void StopBuild_FearForCitiesTownsAndDahan() {
				// Invaders Do not build in target land this turn
				ctx.GameState.Skip1Build( ctx.Space );

				// 1 fear per town/city OR 1 fear per dahan, whichever is less
				int cityTownCount = ctx.Tokens.SumAny( Invader.Town, Invader.City );
				ctx.AddFear( Math.Min( ctx.DahanCount, cityTownCount ) );
			}

			return ctx.SelectActionOption(
				new ActionOption( "Stop build - 1 fear / (Dahan or T/C)", StopBuild_FearForCitiesTownsAndDahan ),
				new ActionOption( "Push up to 3 dahan", () => ctx.PushUpToNDahan(3), ctx.HasDahan )
			);
		}

	}
}

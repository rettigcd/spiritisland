using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class RitesOfTheLandsRejection {

		public const string Name = "Rites of the Land's Rejection";

		[MinorCard( RitesOfTheLandsRejection.Name, 1, Element.Moon, Element.Fire, Element.Earth )]
		[Fast]
		[FromSacredSite( 2, Target.Dahan )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			static void StopBuild_FearForCitiesTownsAndDahan(TargetSpaceCtx ctx) {
				// Invaders Do not build in target land this turn
				ctx.Skip1Build();

				// 1 fear per town/city OR 1 fear per dahan, whichever is less
				int cityTownCount = ctx.Tokens.SumAny( Invader.Town, Invader.City );
				ctx.AddFear( Math.Min( ctx.Dahan.Count, cityTownCount ) );
			}

			return ctx.SelectActionOption(
				new SpaceAction( "Stop build - 1 fear / (Dahan or T/C)", StopBuild_FearForCitiesTownsAndDahan ),
				new SpaceAction( "Push up to 3 dahan", ctx => ctx.PushUpToNDahan(3) ).Cond( ctx.Dahan.Any )
			);
		}

	}
}

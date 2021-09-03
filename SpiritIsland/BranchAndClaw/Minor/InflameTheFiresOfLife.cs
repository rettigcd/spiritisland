﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InflameTheFiresOfLife {

		[MinorCard( "Inflame the Fires of Life", 1, Speed.Slow, Element.Moon, Element.Fire, Element.Plant, Element.Animal )]
		[FromSacredSite( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectPowerOption(
				new PowerOption( "add disease ", AddDisease),
				new PowerOption( "1 fear and 1 strife", FearAndStrife )
			);
		}

		static void AddDisease( TargetSpaceCtx ctx ) => ctx.Tokens[BacTokens.Disease]++;

		static Task FearAndStrife( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			return ctx.Self.SelectInvader_ToStrife( ctx.Tokens );
		}


	}

}

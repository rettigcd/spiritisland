﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ProwlingPanthers {

		[MinorCard( "Prowling Panthers", 1, Speed.Slow, Element.Moon, Element.Fire, Element.Animal )]
		[FromPresence( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new ActionOption( "1 fear, add beast", ()=>FearAndBeast(ctx) ),
				new ActionOption( "destroy 1 explorer/town", ()=>DestroyExplorerTown(ctx), ctx.Tokens.Beasts().Any )
			);
		}

		static void FearAndBeast( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			ctx.Tokens.Beasts().Count++;
		}

		static Task DestroyExplorerTown( TargetSpaceCtx ctx ) {
			return ctx.PowerInvaders.DestroyAny( 1, Invader.Explorer, Invader.Town );
		}

	}

}
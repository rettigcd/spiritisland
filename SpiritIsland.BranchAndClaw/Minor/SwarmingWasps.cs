﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SwarmingWasps {

		[MinorCard( "Swarming Wasps", 0, Speed.Fast, Element.Fire, Element.Air, Element.Animal )]
		[FromPresence( 1, Target.NoBlight )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( "Add 1 beast", () => ctx.Tokens.Beasts().Count++),
				new ActionOption( "Push up to 2 explorers", () => ctx.PushUpTo( 2, Invader.Explorer ), ctx.Tokens.Beasts().Any )
			);

		}

	}

}

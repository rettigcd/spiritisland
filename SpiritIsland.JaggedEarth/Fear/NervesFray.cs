using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class NervesFray : IFearOptions {

		public const string Name = "Nerves Fray";

		[FearLevel(1, "Each player adds 1 Strife in a land not matching a Ravage Card." )]
		public Task Level1( FearCtx ctx ) {
			return ctx.EachPlayerTakesActionInALand( Cmd.AddStrife(1), NotMatchingRavageCard(ctx) );
		}

		[FearLevel(2, "Each player adds 2 Strife in a single land not matching a Ravage Card." )]
		public Task Level2( FearCtx ctx ) { 
			return ctx.EachPlayerTakesActionInALand( Cmd.AddStrife(2), NotMatchingRavageCard(ctx) );
		}

		[FearLevel(3, "Each player adds 2 Strife in a single land not matching a Ravage Card. 1 Fear per player." )]
		public async Task Level3( FearCtx ctx ) { 

			await ctx.EachPlayerTakesActionInALand( Cmd.AddStrife(2), NotMatchingRavageCard(ctx) );

			// 1 Fear per player.
			ctx.GameState.Fear.AddDirect(new FearArgs { count = ctx.GameState.Spirits.Length });
		}

		static Func<TargetSpaceCtx,bool> NotMatchingRavageCard( FearCtx ctx ) {
			List<InvaderCard> ravageCards = ctx.GameState.InvaderDeck.Ravage;
			return (spaceCtx) => !ravageCards.Any( card => card.Matches(spaceCtx.Space) );
		}

	}

}

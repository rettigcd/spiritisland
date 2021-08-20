﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	// Innate 1 - Spirits May Yet Dream => fast any spirit
	[InnatePower( "Spirits May Yet Dream", Speed.Fast )]
	[TargetSpirit]
	class SpiritsMayYetDream {

		//		[InnateOption( "1 animal" )] // testing
		[InnateOption( "2 moon,2 air" )]
		static public async Task Option1( TargetSpiritCtx ctx ) {

			// Turn any face-down fear card face-up
			var cards = ctx.GameState.FearDeck.Concat(ctx.GameState.ActivatedFearCards).ToArray();
			var cardToShow = (NamedFearCard)await ctx.Self.SelectOption("Select fear to reveal", cards, Present.Always);

			await ctx.Self.SelectOption("Done",new IOption[] { new DisplayFearCard{ Text = cardToShow.CardName } }, Present.Always );
		}

		[InnateOption( "3 moon" )]
		static public async Task Option2( TargetSpiritCtx ctx ) {
			// Target spirit gains an element they have at least 1 of
			Element el = await ctx.Target.SelectElementAsync("Gain element",ctx.Target.Elements.Keys);
			++ctx.Target.Elements[el];
		}

		// Opt 1 & 2 don't build on each other, this is the union
		[InnateOption( "3 moon,2 air" )]
		static public async Task Both( TargetSpiritCtx ctx ) {
			await Option1(ctx);
			await Option2(ctx);
		}

	}

}

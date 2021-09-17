using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	// Innate 1 - Spirits May Yet Dream => fast any spirit
	[InnatePower( "Spirits May Yet Dream", Speed.Fast )]
	[TargetSpirit]
	class SpiritsMayYetDream {

		//		[InnateOption( "1 animal" )] // testing
		[InnateOption( "2 moon,2 air","Turn any face down Card face-up. (It's earned/resolved normally, but players can see what's coming)" )]
		static public async Task Option1( TargetSpiritCtx ctx ) {

			// Turn any face-down fear card face-up
			PositionFearCard[] cards = ctx.GameState.Fear.Deck.Concat( ctx.GameState.Fear.ActivatedCards ).ToArray();
			var cardToShow = await ctx.Self.Select( "Select fear to reveal", cards, Present.Always );

			await ctx.Self.ShowFearCardToUser( "Done", cardToShow );
		}

		[InnateOption( "3 moon","Target Spirit gains an element that they have at least 1 of." )]
		static public async Task Option2( TargetSpiritCtx ctx ) {
			// Target spirit gains an element they have at least 1 of
			Element el = (await ctx.Other.SelectElements(1,ctx.Other.Elements.Keys.ToArray())).FirstOrDefault();
			if(el != default)
				++ctx.Other.Elements[el];
		}

		// Opt 1 & 2 don't build on each other, this is the union
		[InnateOption( "3 moon,2 air","Combo - separate me!", AttributePurpose.ExecuteOnly )]
		static public async Task Both( TargetSpiritCtx ctx ) {
			await Option1(ctx);
			await Option2(ctx);
		}

	}

}

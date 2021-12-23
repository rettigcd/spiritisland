using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class SenseOfDread : IFearOptions {

		public const string Name = "Sense of Dread";

		[FearLevel(1, "On Each Board: Remove 1 Explorer from a land matching a Ravage card." )]
		public Task Level1( FearCtx ctx ) {
			// On Each Board
			return ctx.OnEachBoard(
				Cmd.OnBoardPickSpaceThenTakeAction("Remove 1 Explorer from a land matching a Ravage card."
					// Remove 1 Explorer
					,Cmd.RemoveExplorers(1)
					// from a land matching a Ravage card.
					,MatchingRavageCard( ctx )
				)
			);
		}

		[FearLevel(2, "On Each Board: Remove 1 Explorer / Town from a land matching a Ravage card." )]
		public Task Level2( FearCtx ctx ) { 
			// On Each Board
			return ctx.OnEachBoard(
				Cmd.OnBoardPickSpaceThenTakeAction("Remove 1 Explorer from a land matching a Ravage card."
					// Remove 1 Explorer/Town
					,Cmd.RemoveExplorersOrTowns(1)
					// from a land matching a Ravage card.
					,MatchingRavageCard( ctx )
				)
			);
		}

		[FearLevel(3, "On Each Board: Remove 1 Invader from a land matching a Ravage card." )]
		public Task Level3( FearCtx ctx ) {
			// On Each Board
			return ctx.OnEachBoard(
				Cmd.OnBoardPickSpaceThenTakeAction("Remove 1 Explorer from a land matching a Ravage card."
					// Remove 1 Invader
					,Cmd.RemoveInvaders(1)
					// from a land matching a Ravage card.
					,MatchingRavageCard( ctx )
				)
			);
		}

		static Func<Space,bool> MatchingRavageCard( FearCtx ctx ) {
			List<InvaderCard> ravageCards = ctx.GameState.InvaderDeck.Ravage;
			return (space) => ravageCards.Any( card => card.Matches(space) );
		}
	}


}

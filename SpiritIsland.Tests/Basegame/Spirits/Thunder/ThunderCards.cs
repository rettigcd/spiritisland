using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland;
using System.Linq;

namespace SpiritIsland.Tests.Basegame.Spirits.Thunder {

	public class ThunderCards {
		public ThunderCards() {

			// Given: empty board
			spirit = new Thunderspeaker { Energy = 20 };
			a = Board.BuildBoardA();
			gs = new GameState( spirit, a );

			// And: Spirit in spot 1
			spirit.Presence.PlaceOn( a[1] );

			action = spirit.Action;
		}

		protected void When_ActivateCard( string cardName ) {
			spirit.Hand.Single( x => x.Name == cardName ).ActivateAsync( spirit, gs);
		}

		protected void Step( string expectedPrompt, string expectedOptions, string optionTextToSelect, bool expectedResolved ) {
			var current = action.GetCurrent();
			current.Prompt.ShouldBe( expectedPrompt );
			current.Options.Select( o => o.Text ).OrderBy( x => x ).Join( "," ).ShouldBe( expectedOptions );
			var optionToSelect = current.Options.First(o=>o.Text == optionTextToSelect);
			action.Choose( optionToSelect );
			action.IsResolved.ShouldBe( expectedResolved );
		}

		protected readonly Board a;
		protected readonly Spirit spirit;
		protected readonly GameState gs;
		protected readonly BaseAction action;

	}
}

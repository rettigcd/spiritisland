using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.Core;
using System.Linq;

namespace SpiritIsland.Tests.Basegame.Spirits.Thunder {

	public class ThunderCards {
		public ThunderCards() {

			// Given: empty board
			spirit = new ThunderSpeaker { Energy = 20 };
			a = Board.BuildBoardA();
			var gs = new GameState( spirit ) {
				Island = new Island( a )
			};

			// And: Spirit in spot 1
			spirit.Presence.Add( a[1] );

			eng = new ActionEngine( spirit, gs );
			action = new BaseAction( eng );
		}

		protected void When_ActivateCard( string cardName ) {
			spirit.Hand.Single( x => x.Name == cardName ).Activate( eng );
		}

		protected void Step( string expectedPrompt, string expectedOptions, IOption optionToSelect, bool expectedResolved ) {
			action.Prompt.ShouldBe( expectedPrompt );
			action.Options.Select( o => o.Text ).OrderBy( x => x ).Join( "," ).ShouldBe( expectedOptions );
			action.Select( optionToSelect );
			action.IsResolved.ShouldBe( expectedResolved );
		}

		protected readonly Board a;
		protected readonly Spirit spirit;
		protected readonly ActionEngine eng;
		protected readonly BaseAction action;

	}
}

using SpiritIsland.Basegame;
using SpiritIsland.Core;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame {

	public class SuddenAmbush_Tests {

		public SuddenAmbush_Tests() {
			// Given: empty board
			spirit = new ThunderSpeaker { Energy = 20 };
			a = Board.BuildBoardA();
			var gs = new GameState( spirit );
			gs.Island = new Island( a );

			// And: Spirit in spot 1
			spirit.Presence.Add( a[1] );


			eng = new ActionEngine( spirit, gs );
			action = new BaseAction( eng );
		}

		[Fact]
		public void NoDahanToGather() {
			When_ActivateCard( SuddenAmbush.Name );
			Step( "Select target.", "A1,A2,A4,A5,A6", a[1], true );
		}

		[Fact]
		public void Gather1_Kill1() {
			// Given: dahan on a2
			eng.GameState.AddDahan(a[2]);
			//  and: 2 explorers on a1
			eng.GameState.Adjust(a[1],Invader.Explorer,2);

			When_ActivateCard( SuddenAmbush.Name );
			Step( "Select target.", "A1,A2,A4,A5,A6", a[1], false );
			Step( "Gather dahan 1 of 1 from:", "A2,Done", a[2], true);

			// Then: 1 explorer left
			Assert.Equal("1@E1",eng.GameState.InvadersOn(a[1]).ToString());
		}


		void When_ActivateCard( string cardName ) {
			spirit.Hand.Single( x => x.Name == cardName ).Activate( eng );
		}

		Board a;
		Spirit spirit;
		ActionEngine eng;
		BaseAction action;

		void Step( string expectedPrompt, string expectedOptions, IOption optionToSelect, bool expectedResolved ) {
			Assert.Equal( expectedPrompt, action.Prompt );
			Assert.Equal( expectedOptions, action.Options.Select( o => o.Text ).OrderBy( x => x ).Join( "," ) );
			action.Select( optionToSelect );
			Assert.Equal( expectedResolved, action.IsResolved );
		}
	}
}

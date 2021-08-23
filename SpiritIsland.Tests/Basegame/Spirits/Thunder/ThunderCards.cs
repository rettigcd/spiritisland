﻿using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland;
using System.Linq;

namespace SpiritIsland.Tests.Basegame.Spirits.Thunder {

	public class ThunderCards {
		public ThunderCards() {

			// Given: empty board
			spirit = new ThunderSpeaker { Energy = 20 };
			a = Board.BuildBoardA();
			gs = new GameState( spirit ) {
				Island = new Island( a )
			};

			// And: Spirit in spot 1
			spirit.Presence.PlaceOn( a[1] );

			action = spirit.Action;
		}

		protected void When_ActivateCard( string cardName ) {
			spirit.Hand.Single( x => x.Name == cardName ).ActivateAsync( spirit, gs);
		}

		protected void Step( string expectedPrompt, string expectedOptions, IOption optionToSelect, bool expectedResolved ) {
			action.Prompt.ShouldBe( expectedPrompt );
			action.Options.Select( o => o.Text ).OrderBy( x => x ).Join( "," ).ShouldBe( expectedOptions );
			action.Select( optionToSelect );
			action.IsResolved.ShouldBe( expectedResolved );
		}

		protected readonly Board a;
		protected readonly Spirit spirit;
		protected readonly GameState gs;
		protected readonly BaseAction action;

	}
}

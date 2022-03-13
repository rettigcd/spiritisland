﻿using System;
using System.Collections.Generic;

namespace SpiritIsland.Tests {

	// Test spirit that has lots of energy and you can select the card they start with
	class TestSpirit : Spirit {

		public TestSpirit(PowerCard powerCard):base(
			new SpiritPresence(
				new TestPresenceTrack(Track.Energy5,Track.Energy9),
				new TestPresenceTrack(Track.Card1,Track.Card2,Track.Card3)
			)
			,powerCard
		) {
			Growth = new(
				new GrowthOption( new ReclaimAll() ) 
			);
		}

		public override string Text => "CardPlayTestSpirit";

		public override SpecialRule[] SpecialRules => throw new NotImplementedException();

		protected override void InitializeInternal( Board board, GameState gameState ) {
			// Has sacred site on space 5
			var space = board[5];
			Presence.PlaceOn(space,gameState);
			Presence.PlaceOn(space,gameState);
		}

		static public (VirtualTestUser, SelfCtx) SetupGame( 
			PowerCard powerCard, 
			Action<GameState> modGameState = null 
		) {
			var spirit = new TestSpirit( powerCard );
			var gs = new GameState( spirit, Board.BuildBoardA() ) {
				InvaderDeck = new InvaderDeck( null ) // Same order every time
			};
			modGameState?.Invoke( gs );

			_ = new SinglePlayer.SinglePlayerGame( gs );

			var user = new VirtualTestUser( spirit );
			var starterCtx = spirit.Bind( gs, Cause.None );

			// Disable destroying presence
			starterCtx.GameState.DetermineAddBlightEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };

			return (user,starterCtx);
		}

	}

	public class TestPresenceTrack : PresenceTrack {
		public TestPresenceTrack(params Track[] t ) : base( t ) { }
		public void OverrideTrack(int index, Track t) { slots[index]=t;}

	}


	public class VirtualTestUser : VirtualUser {

		public VirtualTestUser(Spirit spirit ) : base( spirit ) { }

		/// <summary> Growth for Test Spirit </summary>
		public void Grows() {
			Growth_SelectsOption( "ReclaimAll" );
		}

		public void DoesNothingForARound() {
			Grows();
			IsDoneBuyingCards();
		}

	}

}

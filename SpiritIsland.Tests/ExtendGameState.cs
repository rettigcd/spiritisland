﻿using Shouldly;
using SpiritIsland;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Tests {

	static public class ExtendGameState {

		/// <summary> Replaces all Invader Cards with null-cards that don't ravage/build/explore</summary>
		static public void DisableInvaderDeck(this GameState gs ) {
			var nullCard = new InvaderCard( Terrain.None );
			gs.InvaderDeck = InvaderDeck.BuildTestDeck( new byte[12].Select( _ => nullCard ).ToArray() );
		}

		static public void Assert_Invaders( this GameState gameState, Space space, string expectedString ) {
			gameState.Tokens[ space ].InvaderSummary.ShouldBe( expectedString );
		}

	}

	public static class InvaderEngine1 {

		public static async Task RavageCard( InvaderCard invaderCard, GameState gameState ) {
			if( invaderCard != null )
				await invaderCard.Ravage( gameState );
		}

	}

}

using SpiritIsland;

namespace SpiritIsland.Tests {

	static public class InvaderCardEx {

		static public InvaderCard For( Space space ) {
			var terrain = new[] { Terrain.Wetland, Terrain.Sand, Terrain.Jungle, Terrain.Mountain }.First( space.Is );
			return terrain != Terrain.Ocean ? InvaderCard.Stage1( terrain ) : throw new ArgumentException( "Can't invade oceans" );
		}

	}

	static public class ExtendGameState {

		/// <summary> Replaces all Invader Cards with null-cards that don't ravage/build/explore</summary>
		static public void DisableInvaderDeck(this GameState gs ) {
			var nullCard = InvaderCard.Stage1( Terrain.None );
			gs.InvaderDeck = InvaderDeck.BuildTestDeck( new byte[12].Select( _ => nullCard ).ToArray() );
		}

		static public void Assert_Invaders( this GameState gameState, Space space, string expectedString ) {
			gameState.Tokens[ space ].InvaderSummary().ShouldBe( expectedString );
		}

		static public void Assert_DreamingInvaders( this GameState gameState, Space space, string expectedString ) {

			static int Order_CitiesTownsExplorers( HealthToken invader )
				=> -(invader.FullHealth * 10 + invader.RemainingHealth);
			var tokens = gameState.Tokens[space];
			string dreamerSummary = tokens.OfCategory( TokenCategory.DreamingInvader )
				.Cast<HealthToken>()
				.OrderBy( Order_CitiesTownsExplorers )
				.Select( invader => tokens.counts[invader] + invader.ToString() )
				.Join( "," );
			dreamerSummary.ShouldBe( expectedString );
		}

	}

	public static class InvaderEngine1 {

		public static async Task RavageCard( IInvaderCard invaderCard, GameState gameState ) {
			if( invaderCard != null )
				await invaderCard.Ravage( gameState );
		}

	}

}

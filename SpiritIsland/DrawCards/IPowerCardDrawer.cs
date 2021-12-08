using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IPowerCardDrawer {

		Task<DrawCardResult> Draw( Spirit spirit, GameState gameState );

		/// <summary> Draws cards from Major Deck, does NOT Forget a card. For that go through the Spirit.DrawMajor </summary>
		Task<DrawCardResult> DrawMajor( Spirit spirit, GameState gameState, int numberToDraw, int numberToKeep );

		Task<DrawCardResult> DrawMinor( Spirit spirit, GameState gameState, int numberToDraw, int numberToKeep );
	}

}
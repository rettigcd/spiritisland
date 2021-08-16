using System.Threading.Tasks;

namespace SpiritIsland {
	public class DrawFromDeck : IPowerCardDrawer {
		public async Task Draw( ActionEngine engine ) {
			if(await engine.SelectTextIndex("Which type do you wish to draw","minor","major") == 0)
				await DrawMinor( engine );
			else
				await DrawMajor( engine );
		}

		public async Task DrawMajor( ActionEngine engine ) {
			var card = await engine.GameState.MajorCards.Draw( engine );
			engine.Self.Hand.Add( card );
			await engine.ForgetPowerCard();
		}

		public async Task DrawMinor( ActionEngine engine ) {
			var card = await engine.GameState.MinorCards.Draw( engine );
			engine.Self.Hand.Add( card );
		}
	}


}
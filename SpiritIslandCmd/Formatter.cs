using SpiritIsland;
using SpiritIsland.Invaders;
using System;
using System.Linq;

namespace SpiritIslandCmd {
	public class Formatter {
		readonly Spirit spirit;
		readonly GameState gameState;
		readonly InvaderDeck deck;
		public Formatter(Spirit spirit, GameState gameState,InvaderDeck deck){
			this.spirit = spirit;
			this.gameState = gameState;
			this.deck = deck;
		}

		public string Format(IActionFactory factory, int nameWidth=1){
			char speed = factory.Speed.ToString()[0];
			char isActive = spirit.PurchasedCards.Contains(factory) ? 'A' : ' ';
			char isDiscarded = spirit.DiscardPile.Contains(factory) ? 'D' : ' ';
			char isinHand = spirit.Hand.Contains(factory) ? 'H' : ' ';
			char unresolved = spirit.UnresolvedActionFactories.Contains(factory) ? '*' : ' ';

			string cost = factory is PowerCard card ? card.Cost.ToString() : "-";

			int padLength = nameWidth - factory.Name.Length;
			string padding = padLength <= 0 ? "" : new string(' ',padLength);

			return $"{factory.Name}{padding}  ({speed}/{cost})  {isinHand} {isActive} {unresolved} {isDiscarded}";
		}

		public string Format(IOption option){
			return option is Track track
					? Format( track )
				: option is Space space 
					? Format( space )
				: option.Text;
		}

		public string Format( Space space ) {

			bool ravage = deck.Ravage?.Matches(space) ?? false;
			bool build = deck.Build?.Matches(space) ?? false;
			string threat = (ravage&&build) ? "Rvg+Bld"
				: ravage ?"  Rvg  "
				: build ? "  Bld  "
				: "       "; 

			// invaders
			var details = gameState.InvadersOn( space ).ToString();

			// dahan
			int dahanCount = gameState.GetDahanOnSpace( space );
			string dahan = (dahanCount > 0) ? ("D" + dahanCount) :"  ";

			int blightCount = gameState.GetBlightOnSpace( space );
			string blight = (blightCount > 0) ? ("B" + blightCount) :"  ";

			// presence
			string pres = spirit.Presence.Where(p=>p==space).Select(x=>"P").Join("");
			return $"{space.Label} {threat} {Pad(space.Terrain)}\t{dahan}\t{details}\t{blight}\t{pres}";
		}
		string Pad(Terrain terrain) {
			string s= terrain.ToString();
			return s + new string(' ',8-s.Length);
		}

		public string Format( Track track ) {
			string details = track == Track.Energy
				? "$/turn = " + spirit.EnergyPerTurn // !!! Display track
				: "Card/turn = " + spirit.NumberOfCardsPlayablePerTurn; // !!! display track
			return $"{track.Text} {details}";
		}


	}

}

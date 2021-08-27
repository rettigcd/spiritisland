using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.SinglePlayer;

namespace SpiritIslandCmd {

	public class UiMap {

		public string Prompt;
		public Dictionary<string,IOption> dict;
		public List<string> descList;
		readonly SinglePlayerGame game;

		public UiMap(SinglePlayerGame game ){
			this.game=game;
			var decisionProvider = game.DecisionProvider;
			Prompt = decisionProvider.GetCurrent().Prompt;
			
			var cachedOptions = decisionProvider.GetCurrent().Options; // cache in case calculated on the fly

			int pad = 0;
			foreach(var o in cachedOptions){
				if(o is IActionFactory factory) pad = Math.Max(pad,factory.Name.Length);
			}

			dict = new Dictionary<string, IOption>();
			var tempList = new List<string[]>();

			int labelIndex=0;
			foreach(var option in cachedOptions){
				string key;
				string description;
				if(TextOption.Done.Matches(option)){
					key = option.Text.Substring(0,1).ToLower();
					description = option.Text;
				} else if(option is Space space) {
					key = space.Label.ToLower();
					description = FormatSpace( space );
				} else if(option is GrowthActionFactory gaf) {
					key = (++labelIndex).ToString();
					description = FormatFactory( gaf, pad );
				} else if(option is IActionFactory factory) {
					key = option.Text.Split(' ').Select(word=>word.Substring(0,1)).Join("").ToLower();
					description = FormatFactory( factory, pad );
				} else {
					key = (++labelIndex).ToString();
					description = FormatOption( option );
				}
				dict.Add(key,option);
				tempList.Add(new string[]{key,description});
			}

			int keyWidth = tempList.Select(x=>x[0].Length).Max();

			descList = tempList
				.Select(x=>Pad(x[0],keyWidth)+" : "+ x[1])
				.ToList();

		}

		public IOption GetOption(string cmd){
			return dict.ContainsKey(cmd) ? dict[cmd] : null;
		}
		public string ToPrompt() => Prompt + descList.Select( d => "\r\n\t" + d ).Join( "" );

		public string FormatOption(IOption option){
			return option is Track track
					? FormatTrack( track )
				: option is Space space 
					? FormatSpace( space )
				: option.Text;
		}

		public string FormatTrack( Track track ) {
			string details = track == Track.Energy1
				? "$/turn = " + game.Spirit.EnergyPerTurn
				: "Card/turn = " + game.Spirit.NumberOfCardsPlayablePerTurn; 
			return $"{track.Text} {details}";
		}

		public string FormatSpace( Space space ) {
			var gameState = game.GameState;
			var deck = gameState.InvaderDeck;
			bool ravage = deck.Ravage?.Matches(space) ?? false;
			bool build = deck.Build?.Matches(space) ?? false;
			string threat = (ravage&&build) ? "Rvg+Bld"
				: ravage ?"  Rvg  "
				: build ? "  Bld  "
				: "       "; 

			// invaders
			var details = gameState.InvadersOn( space ).ToString();

			// dahan
			int dahanCount = gameState.Dahan.Count( space );
			string dahan = (dahanCount > 0) ? ("D" + dahanCount) :"  ";

			int blightCount = gameState.GetBlightOnSpace( space );
			string blight = (blightCount > 0) ? ("B" + blightCount) :"  ";

			// presence
			string pres = game.Spirit.Presence.Placed.Where(p=>p==space).Select(x=>"P").Join("");
			return $"{space.Label} {threat} {Pad(space.Terrain)}\t{dahan}\t{details}\t{blight}\t{pres}";
		}

		static string Pad(Terrain terrain) => Pad(terrain.ToString(),8);

		static string Pad(string s, int length){
			int need = length - s.Length;
			if(0<need)
				s += new string(' ',need);
			return s;
		}

		public string FormatFactory(IActionFactory factory, int nameWidth=1){
			var spirit = game.Spirit;
			var speed = factory.Speed;
			char speedChar = speed.ToString()[0];
			char unresolved = spirit.GetAvailableActions(speed).Contains(factory) ? '*' : ' ';
			string cost = factory is PowerCard card ? card.Cost.ToString() : "-";

			string name = Pad(factory.Name,nameWidth);
			string text = $"{name}  ({speedChar}/{cost}) {unresolved}";

			if(factory is PowerCard pc){
				char isActive = spirit.PurchasedCards.Contains(factory) ? 'A' : ' ';
				char isDiscarded = spirit.DiscardPile.Contains(factory) ? 'D' : ' ';
				char isinHand = spirit.Hand.Contains(factory) ? 'H' : ' ';

				text += $" {isinHand} {isActive} {isDiscarded}"
					+ pc.Elements.Select(e=>e.ToString()).Join(" ");
			}

			return text;
		}

	}

}

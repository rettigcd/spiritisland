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
			var decisionProvider = game.UserPortal;
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
					key = option.Text[..1].ToLower();
					description = option.Text;
				} else if(option is Space space) {
					key = space.Label.ToLower();
					description = FormatSpace( space );
				} else if(option is GrowthActionFactory gaf) {
					key = (++labelIndex).ToString();
					description = FormatFactory( gaf, pad );
				} else if(option is IActionFactory factory) {
					key = option.Text.Split(' ').Select(word=>word[..1] ).Join("").ToLower();
					description = FormatFactory( factory, pad );
				} else {
					key = (++labelIndex).ToString();
					description = option.Text;
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

		public string FormatSpace( Space space ) {
			var gameState = game.GameState;
			var deck = gameState.InvaderDeck;
			bool ravage = deck.Ravage.Count > 0 && deck.Ravage[0].Matches(space); // !! show multiple, not just first
			bool build = deck.Build.Count > 0 && deck.Build[0].Matches(space); // !! show multiple, not just first
			string threat = (ravage&&build) ? "Rvg+Bld"
				: ravage ?"  Rvg  "
				: build ? "  Bld  "
				: "       ";

			// invaders
			var details = gameState.Tokens[ space ].InvaderSummary;

			// dahan
			int dahanCount = gameState.DahanOn( space ).Count;
			string dahan = (dahanCount > 0) ? ("D" + dahanCount) :"  ";

			int blightCount = gameState.Tokens[ space ].Blight.Count;
			string blight = (blightCount > 0) ? ("B" + blightCount) :"  ";

			// presence
			string pres = game.Spirit.Presence.Placed.Where(p=>p==space).Select(x=>"P").Join("");
			return $"{space.Label} {threat}\t{dahan}\t{details}\t{blight}\t{pres}";
		}

//		static string Pad(Terrain terrain) => Pad(terrain.ToString(),8);

		static string Pad(string s, int length){
			int need = length - s.Length;
			if(0<need)
				s += new string(' ',need);
			return s;
		}

		public string FormatFactory(IActionFactory factory, int nameWidth=1){
			var spirit = game.Spirit;
			var speed = factory is IFlexibleSpeedActionFactory flex ? flex.DisplaySpeed : Phase.None;
			char speedChar = speed.ToString()[0];
			char unresolved = spirit.GetAvailableActions(speed).Contains(factory) ? '*' : ' ';
			string cost = factory is PowerCard card ? card.Cost.ToString() : "-";

			string name = Pad(factory.Name,nameWidth);
			string text = $"{name}  ({speedChar}/{cost}) {unresolved}";

			if(factory is PowerCard pc){
				char isActive = spirit.InPlay.Contains(factory) ? 'A' : ' ';
				char isDiscarded = spirit.DiscardPile.Contains(factory) ? 'D' : ' ';
				char isinHand = spirit.Hand.Contains(factory) ? 'H' : ' ';

				text += $" {isinHand} {isActive} {isDiscarded}"
					+ pc.Elements.Select(e=>e.ToString()).Join(" ");
			}

			return text;
		}

	}

}

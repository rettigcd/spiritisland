using System;

namespace SpiritIsland {

	public class InvaderCard : IOption {

		public static readonly InvaderCard Costal = new InvaderCard( s => s.IsCoastal, "Costal" );
		
		public int InvaderStage { get; }
		public string Text { get; }
		public bool Escalation { get; }
		public Func<Space,bool> Matches { get; }

		/// <summary>
		/// Stage 1 or 2 constructor
		/// </summary>
		public InvaderCard(Terrain terrain, bool escalation=false){
			if(terrain==Terrain.Ocean) throw new ArgumentException("Can't invade oceans");
			InvaderStage = escalation ? 2 : 1;
			Matches = (s) => s.Terrain == terrain;
			Text = escalation
				? "2" +terrain.ToString()[..1].ToLower() 
				: terrain.ToString()[..1];
			Escalation = escalation;
		}


		InvaderCard( Func<Space, bool> matches, string text ) { // Costal
			InvaderStage = 2;
			Matches = matches;
			Text = text;
			Escalation = false;
		}


		public InvaderCard(Terrain t1, Terrain t2){
			Matches = (s) => s.Terrain == t1 || s.Terrain == t2;
			Text = t1.ToString()[..1] + "+" + t2.ToString()[..1];
			InvaderStage = 3;
		}

	}

}

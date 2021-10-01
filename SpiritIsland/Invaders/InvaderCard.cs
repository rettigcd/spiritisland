using System;

namespace SpiritIsland {

	public class InvaderCard{
		public static readonly InvaderCard Costal = new InvaderCard();
		
		public string Text { get; }
		public bool Escalation { get; }
		public Func<Space,bool> Matches { get; }

		#region constructors

		public InvaderCard(Terrain terrain, bool escalation=false){
			if(terrain==Terrain.Ocean) throw new ArgumentException("Can't invade oceans");
			Matches = (s) => s.Terrain == terrain;
			Text = terrain.ToString().Substring(0,1);
			Escalation = escalation;
		}

		public InvaderCard(Terrain t1, Terrain t2){
			Matches = (s) => s.Terrain == t1 || s.Terrain == t2;
			Text = t1.ToString().Substring(0,1)+"+"+t2.ToString().Substring(0,1);
		}

		InvaderCard(){ Matches = s=>s.IsCoastal; Text="Costal"; } // costal constructor

		#endregion
	}

}

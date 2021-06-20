using System;

namespace SpiritIsland.Tests.Invaders {
	public class InvaderCard{
		public static readonly InvaderCard Costal = new();
		
		public string Text { get; }
		public Func<Space,bool> Matches { get; }

		#region constructors

		public InvaderCard(Terrain terrain){
			Matches = (s) => s.Terrain == terrain;
			Text = terrain.ToString().Substring(0,1);
		}

		public InvaderCard(Terrain t1, Terrain t2){
			Matches = (s) => s.Terrain == t1 || s.Terrain == t2;
			Text = t1.ToString().Substring(0,1)+"+"+t2.ToString().Substring(0,1);
		}

		InvaderCard(){ Matches = s=>s.IsCostal; Text="Costal"; } // costal constructor

		#endregion
	}

}

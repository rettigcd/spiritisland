using System;
using System.Linq;

namespace SpiritIsland {

	public class Space1 : Space {

		#region constructor

		public Space1(Terrain terrain, string label,string startingItems="")
			:base(label)
		{
			this.terrain = terrain;
			this.StartUpCounts = new StartUpCounts(startingItems);
		}

		#endregion

		public override bool IsOneOf(params Terrain[] options) => options.Contains(terrain);

		public override bool Is( Terrain terrain ) => this.terrain == terrain;

		public StartUpCounts StartUpCounts { get; }

		public void InitTokens( TokenCountDictionary tokens ) {
			StartUpCounts initialCounts = StartUpCounts;
			tokens.Init( Invader.City.Default, initialCounts.Cities );
			tokens.Init( Invader.Town.Default, initialCounts.Towns );
			tokens.Init( Invader.Explorer.Default, initialCounts.Explorers );
			tokens.Dahan.Init( initialCounts.Dahan );
			tokens.Blight.Init( initialCounts.Blight ); // don't use AddBlight because that pulls it from the card and triggers blighted island
		}

		readonly Terrain terrain;

	}

}
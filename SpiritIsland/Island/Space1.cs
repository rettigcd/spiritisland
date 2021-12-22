using System;
using System.Collections.Generic;
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

	public class SpaceMulti : Space {
		public SpaceMulti(params Space[] spaces):base(string.Join(":", spaces.Select(s=>s.Label)) ){
			var parts = new List<Space1>();
			foreach(var space in spaces)
				if(space is Space1 one)
					parts.Add(one);
				else if(space is SpaceMulti many)
					parts.AddRange(many.parts);
			this.parts = parts.ToArray();
		}

		public override bool Is( Terrain terrain ) => parts.Any(part => part.Is(terrain));
		public override bool IsOneOf( params Terrain[] options ) => parts.Any(part => part.IsOneOf(options));

		readonly Space1[] parts;
	}

}
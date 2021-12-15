using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class MultiSpace : Space {

		public MultiSpace(params Space[] spaces)
			:base( string.Join(":", BuildParts(spaces).Select(p=>p.Label)) ) 
		{
			this.Parts = BuildParts( spaces );
		}

		static Space1[] BuildParts( Space[] spaces ) {
			var parts = new List<Space1>();
			foreach(var space in spaces)
				if(space is Space1 one)
					parts.Add( one );
				else if(space is MultiSpace many)
					parts.AddRange( many.Parts );
			return parts.OrderBy(p=>p.Text).ToArray();
		}

		public override bool Is( Terrain terrain ) => Parts.Any(part => part.Is(terrain));
		public override bool IsOneOf( params Terrain[] options ) => Parts.Any(part => part.IsOneOf(options));

		public Space1[] Parts { get; }
	}

}
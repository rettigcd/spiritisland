using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ElementalBoon {

		[MinorCard( "Elemental Boon", 1, Speed.Fast )]
		[TargetSpirit]
		static public async Task Act( TargetSpiritCtx ctx ) {
			var target = ctx.Target;

			var elements = new List<Element>{ Element.Sun, Element.Moon, Element.Air,Element.Fire,Element.Water,Element.Earth,Element.Plant,Element.Animal,};
			var selected = new List<Element>();

			// Target Spirit games 3 _different_ Elements of their choice
			const int totalToGain = 3;
			while(selected.Count < totalToGain) {

				var el = await target.SelectElementAsync($"Select {selected.Count + 1} of {totalToGain} element to gain",elements);
				selected.Add( el );

				++target.Elements[ el ];
				elements.Remove(el);
			}

			// if you target another spirit, you also gain the chosen elements
			if(target != ctx.Self)
				ctx.Self.Elements.AddRange(selected);
		}

	}

}

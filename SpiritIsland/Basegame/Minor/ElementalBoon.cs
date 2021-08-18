using SpiritIsland;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ElementalBoon {

		[MinorCard( "Elemental Boon", 1, Speed.Fast )]
		[TargetSpirit]
		static public async Task Act( IMakeGamestateDecisions engine, Spirit target ) {

			var elements = new List<Element>{
				Element.Sun,
				Element.Moon,
				Element.Air,
				Element.Fire,
				Element.Water,
				Element.Earth,
				Element.Plant,
				Element.Animal,
			};
			var selected = new List<Element>();

			// Target Spirit games 3 _different_ Elements of their choice
			const int totalToGain = 3;
			while(selected.Count < totalToGain) {
				var selection = await target.SelectOption($"Select {selected.Count+1} of {totalToGain} element to gain", elements.Select( x => new ItemOption<Element>( x ) ).ToArray(), true);
				if(selection == null) break;
				var el = ((ItemOption<Element>)selection).Item;
				selected.Add( el );
				++target.Elements[ el ];
				elements.Remove(el);
			}

			// if you target another spirit, you also gain the chosen elements
			if(target != engine.Self)
				engine.Self.Elements.AddRange(selected);
		}
	}

	class ItemOption<T> : IOption {
		public T Item { get; }
		public ItemOption(T item ) { Item = item; }
		public string Text => Item.ToString();
	}

}

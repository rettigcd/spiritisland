using System.Threading.Tasks;

namespace SpiritIsland {
	public class SelectAnyElements : GrowthActionFactory {

		readonly int count;

		public SelectAnyElements(int count ) {
			this.count = count;
		}

		public override async Task ActivateAsync( Spirit self, GameState _ ) {

			var elements = new Element[] { Element.Sun, Element.Moon, Element.Air, Element.Fire, Element.Water, Element.Earth, Element.Plant, Element.Animal };

			for(int i = 0; i < count; ++i) {
				var newEl = await self.SelectElementAsync($"Assign 'Any' element ({i+1}of{count})",elements);
				++self.Elements[newEl];
			}
		}

		public override string ShortDescription => $"Select elements ({count})";

	}


}

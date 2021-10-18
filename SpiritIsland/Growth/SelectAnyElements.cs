using System.Threading.Tasks;

namespace SpiritIsland {

	// !!! if this is used from the presence tracks, could place this directly on the Track.Action property
	public class SelectAnyElements : IActionFactory {

		readonly int count;

		public SelectAnyElements(int count ) {
			this.count = count;
		}

		public string Name => $"Select elements ({count})";

		public string Text => Name;

		public async Task ActivateAsync( Spirit self, GameState _ ) {

			var elements = new Element[] { Element.Sun, Element.Moon, Element.Air, Element.Fire, Element.Water, Element.Earth, Element.Plant, Element.Animal };

			var newElements = await self.SelectElements( count, elements );
			foreach(var newEl in newElements)
				++self.Elements[newEl];
			
		}
		public Task ActivateAsync(SpiritGameStateCtx ctx) => ActivateAsync( ctx.Self, ctx.GameState);

		public bool IsActiveDuring( Speed _, CountDictionary<Element> _1 ) => true;

	}


}

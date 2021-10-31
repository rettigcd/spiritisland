using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary>
	/// Used to replace 'Any' element with a specific element.
	/// </summary>
	public class SelectAnyElements : IActionFactory {

		readonly int count;

		public SelectAnyElements( int count ) {
			this.count = count;
		}

		public string Name => $"Select elements ({count})";

		public string Text => Name;

		public async Task ActivateAsync( Spirit self, GameState _ ) {

			var newElements = await self.SelectElements( count, ElementList.AllElements );
			foreach(var newEl in newElements)
				++self.Elements[newEl];
			
		}
		public Task ActivateAsync(SpiritGameStateCtx ctx) => ActivateAsync( ctx.Self, ctx.GameState);

		public bool IsActiveDuring( Speed _, CountDictionary<Element> _1 ) => true;

	}


}

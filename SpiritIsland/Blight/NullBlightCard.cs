using System.Threading.Tasks;

namespace SpiritIsland {

	public class NullBlightCard : IBlightCard {

		public bool IslandIsBlighted {get; set; }

		public string Name => "[null]";

		public void OnBlightDepleated( GameState gs ) {
			if(IslandIsBlighted) return;
			IslandIsBlighted = true;
			gs.blightOnCard += 4 * gs.Spirits.Length;
		}

		public void OnGameStart( GameState gs ) {
			gs.blightOnCard += 5 * gs.Spirits.Length;
		}

		public Task OnStartOfInvaders( GameState gs ) {return Task.CompletedTask;}

	}
}
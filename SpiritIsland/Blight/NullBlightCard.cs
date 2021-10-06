using System.Threading.Tasks;

namespace SpiritIsland {

	public class NullBlightCard : IBlightCard {

		public bool IslandIsBlighted {get; set; }

		public void OnBlightDepleated( GameState gs ) {
			if(IslandIsBlighted) GameOverException.Lose();
			IslandIsBlighted = true;
			gs.blightOnCard += 4 * gs.Spirits.Length;
		}

		public void OnGameStart( GameState gs ) {
			gs.blightOnCard += 5 * gs.Spirits.Length;
		}

		public Task OnStartOfInvaders( GameState gs ) {return Task.CompletedTask;}

	}
}
using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IBlightCard {
		void OnGameStart( GameState gs );
		void OnBlightDepleated( GameState gs );
		Task OnStartOfInvaders( GameState gs );
		bool IslandIsBlighted { get; set; } // set so we can update via Memento
	}

}
using System.Threading.Tasks;

namespace SpiritIsland.Core {
	public interface IBlightCard {
		void OnGameStart( GameState gs );
		void OnBlightDepleated( GameState gs );
		Task OnStartOfInvaders( GameState gs );
		bool IslandIsBlighted { get; }
	}
}
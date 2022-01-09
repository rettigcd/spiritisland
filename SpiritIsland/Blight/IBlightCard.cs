using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IBlightCard {
		string Name { get; }
		void OnGameStart( GameState gs );
		Task OnBlightDepleated( GameState gs );
		bool CardFlipped { get; set; } // set so we can update via Memento
		ActionOption<GameState> Immediately { get; }
	}

}
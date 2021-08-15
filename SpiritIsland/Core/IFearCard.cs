using System.Threading.Tasks;

namespace SpiritIsland {
	public interface IFearCard {
		Task Level1(GameState gs);
		Task Level2( GameState gs );
		Task Level3( GameState gs );
	}

}


using System.Threading.Tasks;

namespace SpiritIsland.Core {
	public interface IFearCard {
		Task Level1(GameState gs);
		Task Level2( GameState gs );
		Task Level3( GameState gs );
	}

}


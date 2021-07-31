using System.Threading.Tasks;

namespace SpiritIsland.Core {
	public interface IFearCard {
		void Level1(GameState gs);
		void Level2( GameState gs );
		void Level3( GameState gs );
	}

}


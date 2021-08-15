using System.Threading.Tasks;

namespace SpiritIsland {
	public interface IPowerCardDrawer {
		Task Draw( ActionEngine engine);
		Task DrawMajor( ActionEngine engine );
		Task DrawMinor( ActionEngine engine );
	}

}
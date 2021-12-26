using System.Threading.Tasks;

namespace SpiritIsland {
	public interface IFearOptions {
		string Name { get; }
		Task Level1( FearCtx ctx );
		Task Level2( FearCtx ctx );
		Task Level3( FearCtx ctx );
	}

}


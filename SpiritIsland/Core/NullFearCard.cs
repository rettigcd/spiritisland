using System.Threading.Tasks;

namespace SpiritIsland {
	public class NullFearCard : IFearCard {
	
		public const string Name = "Null Fear Card";
		public Task Level1( GameState gs ) { return Task.CompletedTask; }
		public Task Level2( GameState gs ) { return Task.CompletedTask; }
		public Task Level3( GameState gs ) { return Task.CompletedTask; }
	}


}


namespace SpiritIsland.PowerCards {

	[PowerCard(UncannyMelting.Name,0, Speed.Fast)]
	public class UncannyMelting : BaseAction {
		// Range: 1 from SS
		// If invaders are present, 1 fear
		// If target land is S/W, remove 1 blight

		public const string Name = "Uncanny Melting";
		public UncannyMelting(Spirit spirit,GameState gameState):base(gameState){

		}
	}
}

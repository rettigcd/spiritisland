using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class InvadersFindTheLandToTheirLiking : BlightCardBase {

		public InvadersFindTheLandToTheirLiking():base("Invaders Find the Land to Their Liking",2) {}

		protected override Task BlightAction( GameState gs ) {
			// (Still Healthy for now)
			// Immediately: If the Terror Level is 1/2/3, add 1/1.5/2 Fear Markers per player to the Fear Pool (round down at TL2)
			int pc = gs.Spirits.Length;
			int fearCount = gs.Fear.TerrorLevel switch {
				1 => pc*1, // 1
				2 => pc*3/2, // 1.5 round down
				3 => pc*2, // 2
				_ => throw new System.ArgumentOutOfRangeException(),
			};
			gs.Fear.AddDirect(new FearArgs { count = fearCount });

			return Task.CompletedTask;
		}

		protected override void Side2Depleted(  GameState gs ) {
			// If there is ever NO Blight here, draw a new Blight Card.
			gs.BlightCard = gs.BlightCards[0];
			gs.BlightCards.RemoveAt(0);
			// It comes into play already flipped
			gs.BlightCard.OnBlightDepleated(gs);
		}

	}

}

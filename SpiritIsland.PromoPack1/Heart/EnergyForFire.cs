using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class EnergyForFire : GrowthActionFactory {

		public override Task ActivateAsync( Spirit self, GameState gameState ) {
			var presence = self.Presence as HeartPresence;
			self.Energy += presence.FireShowing();
			return Task.CompletedTask;
		}
	}



}

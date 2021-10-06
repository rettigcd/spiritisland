using SpiritIsland;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DownwardSpiral : IBlightCard {

		public void OnBlightDepleated( GameState gs ) {
			if(IslandIsBlighted) GameOverException.Lose();
			IslandIsBlighted = true;
			// +5 blight
			gs.blightOnCard += 5 * gs.Spirits.Length;
		}

		public async Task OnStartOfInvaders( GameState gs ) {
			if(!IslandIsBlighted) return;
			// Spirit destorys a presence.
			foreach(var spirit in gs.Spirits) {
				var presence = await spirit.Action.Decision( new Decision.TargetSpace( "BLIGHT: Select presence to destroy.", spirit.Presence.Spaces.ToArray() ));
				spirit.Presence.Destroy( presence );
			}
		}

		public void OnGameStart( GameState gs ) {
			gs.blightOnCard = 2 * gs.Spirits.Length + 1; // +1 from Jan 2021 errata
		}
		public bool IslandIsBlighted { get; set; }
	}

}

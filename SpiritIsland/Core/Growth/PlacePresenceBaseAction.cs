using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class PlacePresenceBaseAction : BaseAction {

		public PlacePresenceBaseAction(Spirit spirit,GameState gs,Space[] destinationOptions)
			:base(spirit,gs)
		{
			_ = ActAsync(destinationOptions);
		}

		public async Task ActAsync(Space[] destinationOptions){
			// From
			var from = await engine.SelectTrack();

			// To
			var to = await engine.SelectSpace("Where would you like to place your presence?",destinationOptions);
			
			// from
			if(from == Track.Card)
				engine.Self.RevealedCardSpaces++;
			else if(from == Track.Energy)
				engine.Self.RevealedEnergySpaces++;

			// To
			engine.Self.Presence.Add(to);
		}

	}

}

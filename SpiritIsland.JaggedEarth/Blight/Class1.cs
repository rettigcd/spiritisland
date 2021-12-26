using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class B1 {

//		public B1():base("",5) {}

//		protected override Task BlightAction( GameState gs ) {
//			return Task.CompletedTask;
//		}

	}

	// All Things Weaken -
	// (3/player) -
	// Ongoing, starting next turn:
	// Invaders and Dahan have -1 Health (min, 1).
	// The land takes blight on 1 less Damage (normally 1),.
	// When you add blight, it Destroys all presence/beast in that land and 1 presence (total) in an adjacent land.

	// Invaders Find the Land to Their Liking
	// (Still Healthy for now)
	// (2/player) -
	// Immediately: If the Terror Level is 1/2/3, add 1/1.5/2 Fear Markers per player to the Fear Pool (round down at TL2)
	// If there is ever NO Blight here, draw a new Blight Card.  It comes into play already flipped

	// Power Corrodes the Spirit
	// 4/player
	// Each Invader Phase:  Each Spirit Destorys 1 of their presence if they have 3 or more Power Cards in play, or have a Power Card in play costing 4 or more (printed) Energy.

	// Strong Earth Shatters Slowly
	// (Still healthy for now)
	// 2 / player
	// Immediately: Each player adds 1 blight (from this card) to a land adjacent to blight.
	// If there is ever NO Blight here, draw a new Blight Card.  IT comes into play already flipped

	// Thriving Communitites
	// 4 / player
	// Immediately, on each board:  In 4 different lands with explorer/town, Replace 1 town with a city or Replace 1 explorer with 1 town

	// Unnatural Proliferation
	// 3 / player
	// Immediately:  Each spirit adds 1 presence to a land with their prescense.
	// On each board:  Add 1 dahan to a land with dahan, and 2 cities to the land with fewest town/city (min.1)

	// Untended Land Crumbles
	// 4 / player
	// Each Invader Phase:  On Each Board:  Add 1 blight to a land adjacent to blight.
	//		Spirits may prevent this on any/all boards;
	//		each board to be protected requires jointly paying 3 energy or destroying 1 presence from that board.


}

using System;

namespace SpiritIsland {

	class AddPresence {
		public AddPresence(Space to){this.to = to;}
		public void Apply(Spirit spirit) => spirit.Presence.Add(to);
		readonly Space to;
	}

}

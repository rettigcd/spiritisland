using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	class LingeringPestilence : SpiritPresence.DefaultDestroyBehavior {

		static public SpecialRule Rule => new SpecialRule(
			"Lingering Pestilence",
			"When your presence is destroyed by anything except a Spirit action, add 1 disease where each destroyed presence was."
		);

		public override Task DestroyPresenceApi( SpiritPresence presence, Space space, GameState gs, Cause cause ) {
			if(cause != Cause.Power)
				gs.Tokens[space].Disease.Add(1); // !!! trig Shifting Memories Prepare Element???
			return base.DestroyPresenceApi(presence,space,gs,cause);
		}
	}

}

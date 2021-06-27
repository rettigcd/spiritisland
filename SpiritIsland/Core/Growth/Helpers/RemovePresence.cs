namespace SpiritIsland.Core {

	class RemovePresence {
		public RemovePresence(Space from){this.from = from;}
		public void Apply(Spirit spirit) => spirit.Presence.Remove(from);
		readonly Space from;
	}

}

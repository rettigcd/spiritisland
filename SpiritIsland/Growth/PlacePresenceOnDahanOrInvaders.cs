namespace SpiritIsland {
	public class PlacePresenceOnDahanOrInvaders : PlacePresence {
		public PlacePresenceOnDahanOrInvaders(int range):base(range){}
		public override bool IsValid( Space bs, GameState gs ) {
			return gs.HasDahan(bs) || gs.HasInvaders(bs);
		}
	}
}

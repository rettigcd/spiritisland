namespace SpiritIsland {
	public class SelectInvaderToPushDecision : InvadersOnSpaceDecision {
		public SelectInvaderToPushDecision( Space space, int count, InvaderSpecific[] options, Present present )
			: base( $"Select invader to push ({count} remaining)", space, options, present ) { }
	}


}
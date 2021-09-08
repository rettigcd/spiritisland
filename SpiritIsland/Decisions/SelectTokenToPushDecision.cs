namespace SpiritIsland {

	public class SelectTokenToPushDecision : InvadersOnSpaceDecision {

		public SelectTokenToPushDecision( Space space, int count, Token[] options, Present present )
			: base( $"Select item to push ({count} remaining)", space, options, present ) { }
	}


}
namespace SpiritIsland {
	public class SelectInvaderToDowngrade : InvadersOnSpaceDecision {
		public SelectInvaderToDowngrade( Space space, Token[] options, Present present )
			: base( "Select invader to down-grade (C=>T or T=>E)", space, options, present ) { }
	}


}
namespace SpiritIsland {

	public class InvadersOnSpaceDecision : TypedDecision<InvaderSpecific> {
		protected InvadersOnSpaceDecision( string prompt, Space space, InvaderSpecific[] options, Present present )
			: base( prompt, options, present ) { 
			Space = space;
		}
		public Space Space { get; }
	}

	public class SelectInvaderToPushDecision : InvadersOnSpaceDecision {
		public SelectInvaderToPushDecision( Space space, int count, InvaderSpecific[] options, Present present )
			: base( $"Select invader to push ({count} remaining)", space, options, present ) { }
	}

	public class SelectInvaderToGatherDecision : InvadersOnSpaceDecision {
		public Space From { get; }
		public Space To { get; }
		public SelectInvaderToGatherDecision( Space from, Space to, InvaderSpecific[] options, Present present )
			: base( $"Invader to gather ({from.Label}=>{to.Label})", from, options, present ) {
			From = from;
			To = to;
		}
	}

	public class SelectInvaderToDowngrade : InvadersOnSpaceDecision {
		public SelectInvaderToDowngrade( Space space, InvaderSpecific[] options, Present present )
			: base( "Select invader to down-grade (C=>T or T=>E)", space, options, present ) { }
	}

	public class SelectInvaderToDamage : InvadersOnSpaceDecision {
		public SelectInvaderToDamage(int maxDamage, Space space, InvaderSpecific[] options, Present present )
			: base( $"Apply damage({maxDamage}) to:", space, options, present ) { }
	}


}
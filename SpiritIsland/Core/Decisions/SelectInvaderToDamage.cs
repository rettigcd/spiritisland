namespace SpiritIsland {
	public class SelectInvaderToDamage : InvadersOnSpaceDecision {
		public SelectInvaderToDamage(int maxDamage, Space space, InvaderSpecific[] options, Present present )
			: base( $"Apply damage({maxDamage}) to:", space, options, present ) { }
	}


}
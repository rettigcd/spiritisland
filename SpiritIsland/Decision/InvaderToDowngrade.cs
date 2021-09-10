using System.Collections.Generic;

namespace SpiritIsland.Decision {

	public class InvaderToDowngrade : TokenOnSpace {

		public InvaderToDowngrade( Space space, IEnumerable<Token> options, Present present )
			: base( "Select invader to down-grade (C=>T or T=>E)", space, options, present ) { }

	}


}
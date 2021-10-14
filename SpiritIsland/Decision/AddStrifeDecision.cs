
namespace SpiritIsland.Decision {
	public class AddStrifeDecision : Decision.TokenOnSpace {

		public AddStrifeDecision( TokenCountDictionary tokens, params TokenGroup[] groups )
			: base( "Add Strife", 
				  tokens.Space,
				  (groups!=null && groups.Length>0) ? tokens.OfAnyType(groups) : tokens.Invaders(), 
				  Present.Always 
			) { }
	}

}
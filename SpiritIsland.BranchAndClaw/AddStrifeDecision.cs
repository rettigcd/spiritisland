
namespace SpiritIsland.BranchAndClaw {
	public class AddStrifeDecision : Decision.TokenOnSpace {
		public AddStrifeDecision( TokenCountDictionary tokens )
			: base( "Add Strife", tokens.Space, tokens.Invaders(), Present.Always ) { }
	}

}
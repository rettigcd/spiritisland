using System.Threading.Tasks;

namespace SpiritIsland {

	public static class PowerCtxExtensions {
		
		static public GameState_BranchAndClaw BAC( this GameState gs ) => (GameState_BranchAndClaw)gs;

		/// <param name="groups">Option: if null/empty, no filtering</param>
		static public async Task SelectInvader_ToStrife( this Spirit spirit, TokenCountDictionary tokens, params TokenGroup[] groups ) {
			var invader = await spirit.Action.Decision( new Decision.AddStrifeDecision( tokens, groups ) );
			if(invader != null)
				tokens.AddStrifeTo( invader );
		}

	}

}

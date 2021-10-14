using System.Threading.Tasks;

namespace SpiritIsland {

	public static class PowerCtxExtensions {
		
		static public GameState_BranchAndClaw BAC( this GameState gs ) => (GameState_BranchAndClaw)gs;

		static public Task AddStrife( this TargetSpaceCtx ctx, params TokenGroup[] groups ) => ctx.Self.SelectInvader_ToStrife( ctx.Tokens, groups );

		static public async Task SelectInvader_ToStrife( this Spirit spirit, TokenCountDictionary tokens, params TokenGroup[] groups ) {
			var invader = await spirit.Action.Decision( new Decision.AddStrifeDecision( tokens, groups ) );
			if(invader != null)
				tokens.AddStrifeTo( invader );
		}

	}

}

using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public static class PowerCtxExtensions {
		
		static public GameState_BranchAndClaw BAC( this GameState gs ) => (GameState_BranchAndClaw)gs;

		static public Task AddStrife( this TargetSpaceCtx ctx ) => ctx.Self.SelectInvader_ToStrife( ctx.Tokens );

	}

}

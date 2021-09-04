
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public static class BacTokens {
		static readonly public Token Beast = new TokenGroup("Beast",1).Default;
		static readonly public Token Wilds = new TokenGroup("Wilds",1).Default;
		static readonly public Token Disease = new TokenGroup("Zizease",1).Default;
	}

	public class GameState_BranchAndClaw : GameState {

		public GameState_BranchAndClaw(Spirit spirit,Board board ) : base( spirit, board ) {
		}

		public override void Initialize() {
			base.Initialize();
			foreach(var board in Island.Boards) {
				Tokens[board[2]][BacTokens.Disease]++;
				var lowest = board.Spaces.Skip(1).First(s=>s.StartUpCounts.Empty);
				Tokens[lowest][BacTokens.Beast]++;
			}
		}

		protected override bool ExploresSpace( Space space ) {
			var counts = Tokens[space];
			if(counts.Has(BacTokens.Wilds)) {
				counts.Adjust( BacTokens.Wilds, -1 );
				return false;
			}
			return base.ExploresSpace( space );
		}

		protected override string Build( Space space, TokenCountDictionary group ) {
			var counts = Tokens[space];
			if(counts.Has(BacTokens.Disease)) {
				counts.Adjust( BacTokens.Disease, -1 );
				return space.Label +" build stopped by disease";
			}
			return base.Build( space, group );
		}

		protected override async Task<string> RavageSpace( InvaderGroup grp ) {
			var cfg = GetRavageConfiguration( grp.Space );
			var eng = new RavageEngineWithStrife( this, grp, cfg );
			await eng.Exec();
			return grp.Space.Label + ": " + eng.log.Join( "  " );
		}

	}

}

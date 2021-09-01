
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GameState_BranchAndClaw : GameState {

		public GameState_BranchAndClaw(Spirit spirit,Board board ) : base( spirit, board ) {
		Beasts = new TokenCounts(this);
		Wilds = new TokenCounts(this);
		Disease = new TokenCounts(this);
	}

	protected override bool ExploresSpace( Space space ) {
			if(Wilds.AreOn( space )) {
				Wilds.RemoveOneFrom( space );
				return false;
			}
			return base.ExploresSpace( space );
		}

		protected override string Build( Space space, IInvaderCounts group ) {
			if(Disease.AreOn( space )) {
				Disease.RemoveOneFrom( space );
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

		public TokenCounts Beasts { get; }
		public TokenCounts Wilds { get; }
		public TokenCounts Disease { get; }

	}

}

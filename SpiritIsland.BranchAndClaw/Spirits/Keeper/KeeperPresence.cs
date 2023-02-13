using System.Linq;

namespace SpiritIsland.BranchAndClaw;

public partial class Keeper {

	class KeeperPresence : SpiritPresence {
		public KeeperPresence()
			: base(
				new PresenceTrack( Track.Energy2, Track.SunEnergy, Track.Energy4, Track.Energy5, Track.PlantEnergy, Track.Energy7, Track.Energy8, Track.Energy9 ),
				new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, Track.Card5Reclaim1 )
			) { }

		public override void SetSpirit( Spirit spirit ) { 
			base.SetSpirit( spirit );
			Token = new KeeperToken( spirit );
		}

	}

}

public class KeeperToken : SpiritPresenceToken, IHandleTokenAdded {

	public KeeperToken(Spirit spirit):base(spirit) {}

	public async Task HandleTokenAdded( ITokenAddedArgs args ) {
		if(args.Token != this) return;

		int tokenCount = args.AddedTo[this];
		bool createdSacredSite = (tokenCount-args.Count) < 2 && 2<= tokenCount;

		if(createdSacredSite && args.AddedTo.Dahan.Any) {
			var selfCtx = ActionScope.Current.Category == ActionCategory.Spirit_Power
				? _spirit.BindMyPowers()
				: _spirit.BindSelf();
			await selfCtx.Target( args.AddedTo ).PushDahan( int.MaxValue );
		}
	}
}
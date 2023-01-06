namespace SpiritIsland.BranchAndClaw;

public partial class Keeper {

	class KeeperPresence : SpiritPresence {
		public KeeperPresence()
			: base(
				new PresenceTrack( Track.Energy2, Track.SunEnergy, Track.Energy4, Track.Energy5, Track.PlantEnergy, Track.Energy7, Track.Energy8, Track.Energy9 ),
				new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4, Track.Card5Reclaim1 )
			) { }

		public override async Task Place( IOption from, Space to, GameState gs, UnitOfWork actionScope ) {
			bool wasSacredSite = SacredSites( gs, gs.Island.Terrain ).Contains( to );
			await base.Place( from, to, gs, actionScope );
			bool createdSacredSite = !wasSacredSite && SacredSites( gs, gs.Island.Terrain ).Contains( to );

			if( createdSacredSite && gs.DahanOn( to ).Any) {
				var selfCtx = actionScope.Category == ActionCategory.Spirit_Power
					? Self.BindMyPowers( gs, actionScope )
					: Self.BindSelf( gs, actionScope );
				await selfCtx.Target( to ).PushDahan( int.MaxValue );
			}
		}
	}

}


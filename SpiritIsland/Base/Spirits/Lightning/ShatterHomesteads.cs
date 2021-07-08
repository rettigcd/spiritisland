using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(ShatterHomesteads.Name,2,Speed.Slow,Element.Fire,Element.Air)]
	public class ShatterHomesteads : TargetSpaceAction {
		public const string Name = "Shatter Homesteads";

		// range 2 from sacred site
		public ShatterHomesteads(Spirit spirit,GameState gameState)
			:base(spirit,gameState,2,From.SacredSite){ }

		// 1 fear.  Destroy 1 town
		protected override void SelectSpace( Space space ) {
			gameState.AddFear(1);
			var grp = gameState.InvadersOn(space);
			if(grp.HasTown){
				var town = grp[Invader.Town] > 0 ? Invader.Town : Invader.Town1;
				grp[town]--;
				gameState.UpdateFromGroup(grp);
			}
		}
	}


}

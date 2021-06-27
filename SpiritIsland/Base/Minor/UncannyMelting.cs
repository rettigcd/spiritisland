using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[PowerCard(UncannyMelting.Name,0, Speed.Fast)]
	public class UncannyMelting : TargetSpaceAction {

		public const string Name = "Uncanny Melting";
		public UncannyMelting(Spirit spirit,GameState gameState)
			:base(spirit,gameState,1,From.SacredSite){}

		protected override bool FilterSpace(Space space){
			return space.Terrain.IsIn(Terrain.Sand,Terrain.Wetland)
				&& (gameState.HasBlight(space) || gameState.HasInvaders(space));
		}

		protected override void SelectSpace(Space space){

			if(gameState.HasInvaders(space))
				gameState.AddFear(1);

			if(gameState.HasBlight(space))
				gameState.AddBlight(space,-1);
		}

	}

}

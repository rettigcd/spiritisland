
namespace SpiritIsland.PowerCards {

	[PowerCard(UncannyMelting.Name,0, Speed.Fast)]
	public class UncannyMelting : BaseAction {
		// Range: 1 from SS
		// If invaders are present, 1 fear
		// If target land is S/W, remove 1 blight

		public const string Name = "Uncanny Melting";
		public UncannyMelting(Spirit spirit,GameState gameState):base(gameState){
			engine.decisions.Push(new TargetSpaceRangeFromSacredSite(spirit,1
				,InvadersOrBlightOnSandOrWetland
				,SelectSpace
			));
		}

		bool InvadersOrBlightOnSandOrWetland(Space space){
			return HasRemoveableBlight(space)
				|| gameState.HasInvaders(space);
		}

		bool HasRemoveableBlight(Space space) 
			=> (space.Terrain == Terrain.Sand || space.Terrain == Terrain.Wetland)
			&& gameState.HasBlight(space);

		void SelectSpace(Space space){

			if(gameState.HasInvaders(space))
				gameState.AddFear(1);

			if(HasRemoveableBlight(space))
				gameState.AddBlight(space,-1);
		}

	}

	//public class RemoveBlight : IAtomicAction {
	//	readonly Space space;
	//	public RemoveBlight(Space space){
	//		this.space = space;
	//	}
	//	public void Apply( GameState gameState ) {
	//		gameState.AddBlight(space,-1);
	//	}
	//}
}

namespace SpiritIsland.Basegame {

	public class UncannyMelting {

		public const string Name = "Uncanny Melting";

		[MinorCard(UncannyMelting.Name,1, Speed.Slow,Element.Sun,Element.Moon,Element.Water)]
		[FromSacredSite(1,Target.Any)]
		static public void ActAsync(ActionEngine eng,Space target){
			var (_,gameState) = eng;

			// Invaders
			if(gameState.HasInvaders(target))
				gameState.AddFear(1);

			// !!! unit test - requires Sand / wetland
			if(gameState.HasBlight(target) && target.Terrain.IsIn(Terrain.Wetland,Terrain.Sand))
				gameState.RemoveBlight(target);

		}

	}

}

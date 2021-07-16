using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class SongOfSanctity {

		public const string Name = "Song of Sanctity";

		[MinorCard(SongOfSanctity.Name, 1, Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
		[FromPresence(1,Filter.JungleOrMountain)]
		static public async Task ActionAsync(ActionEngine engine, Space target){
			var (_,gameState) = engine;
			var group = gameState.InvadersOn(target);

			if( group[Invader.Explorer]>0 )
				await engine.PushUpToNInvaders(target,engine.GameState.InvadersOn(target)[Invader.Explorer],Invader.Explorer);
			else if(gameState.HasBlight(target))
				gameState.AddBlight(target,-1);

		}

	}

}

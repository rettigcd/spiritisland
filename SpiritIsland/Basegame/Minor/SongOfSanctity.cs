using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SongOfSanctity {

		public const string Name = "Song of Sanctity";

		[MinorCard(SongOfSanctity.Name, 1, Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
		[FromPresence(1,Target.JungleOrMountain)]
		static public async Task ActionAsync(TargetSpaceCtx ctx){
			var target = ctx.Target;
			var (_,gameState) = ctx;
			var group = gameState.InvadersOn(target);

			if( group[InvaderSpecific.Explorer]>0 )
				await ctx.PushUpToNInvaders(target,ctx.GameState.InvadersOn(target)[Invader.Explorer],Invader.Explorer);
			else if(gameState.HasBlight(target))
				gameState.AddBlight(target,-1);

		}

	}

}

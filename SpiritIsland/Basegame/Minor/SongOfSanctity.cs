using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class SongOfSanctity {

		public const string Name = "Song of Sanctity";

		[MinorCard(SongOfSanctity.Name, 1, Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
		[FromPresence(1,Target.JungleOrMountain)]
		static public async Task ActionAsync(TargetSpaceCtx ctx){
			int explorerCount = ctx.PowerInvaders[Invader.Explorer[1]];
			if( 0 < explorerCount )
				await ctx.PowerPushUpToNInvaders( explorerCount, Invader.Explorer);
			else if( ctx.HasBlight )
				ctx.RemoveBlight();
		}

	}

}

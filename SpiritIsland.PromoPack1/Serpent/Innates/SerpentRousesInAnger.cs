using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	[InnatePower( SerpentRousesInAnger.Name ), Slow, FromPresence(0)]
	public class SerpentRousesInAnger {

		public const string Name = "Serpent Rouses in Anger";

		[InnateOption( "1 fire,1 earth","For each fire earth you have, 1 Damage to 1 town / city." )]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			return Task.CompletedTask;
		}

		[InnateOption( "2 moon 2 earth", "For each 2 moon 2 earth you have, 2 fear and you may Push 1 town from target land." )]
		static public Task Option2Async( TargetSpaceCtx ctx ) {
			return Task.CompletedTask;
		}

		[InnateOption("5 moon,6 fire,6 earth", "-7 Energy.  In every land in the game: X Damage, where X is the number of presence you have in and adjacent to that land." )]
		static public Task Option3Async( TargetSpaceCtx ctx ) {
			return Task.CompletedTask;
		}

	}

}

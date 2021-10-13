using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	[InnatePower( SerpentWakesInPower.Name ), Slow, TargetSpirit] // !!! target self
	public class SerpentWakesInPower {

		public const string Name = "Serpent Wakes in Power";

		[InnateOption( "2 fire,1 water,1 plant","Gain 1 Energy. Other spirits with any Absorbed Presence also gain 1 Energy." )]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			return Task.CompletedTask;
		}

		[InnateOption( "2 water,3 earth,2 plant", "Add 1 presence to range-1.  Other spirits with 2 or more Absobred Presence may do likewise." )]
		static public Task Option2Async( TargetSpaceCtx ctx ) {
			return Task.CompletedTask;
		}

		[InnateOption("3 fire,3 water,3 earth,3 plant", "Gain a Major Power without Forgetting.  Other Spirits with 3 or more Absorbed Presence may do likewise." )]
		static public Task Option3Async( TargetSpaceCtx ctx ) {
			return Task.CompletedTask;
		}

	}

}

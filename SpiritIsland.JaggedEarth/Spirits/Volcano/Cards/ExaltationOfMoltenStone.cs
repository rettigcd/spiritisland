using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class ExaltationOfMoltenStone {

		[SpiritCard("Exaltation of Molten Stone",1, Element.Moon,Element.Fire,Element.Earth), Fast, AnotherSpirit]
		public static Task ActAsync(TargetSpiritCtx ctx ) {
			// Split 1 Energy per fire you have between yourself and target Spirit, as evenly as possible.
			int fireCount = ctx.Self.Elements[Element.Fire];
			int energyForSelf = fireCount / 2; // will round down
			int energyForOther = fireCount - energyForSelf;
			ctx.Self.Energy += energyForSelf;
			ctx.Other.Energy += energyForOther;

			// Target Spirit gains +1 range with their Powers that originate from a Mountain
			ExtendRangeFromMountains( ctx.OtherCtx );

			return Task.CompletedTask;
		}

		private static void ExtendRangeFromMountains( SpiritGameStateCtx x ) {
			TargetLandApi.ScheduleRestore( x );
			x.Self.TargetLandApi = new ExtendRange1FromMountain( x.Self.TargetLandApi );
		}

		class ExtendRange1FromMountain : TargetLandApi {

			readonly TargetLandApi originalApi;

			public ExtendRange1FromMountain( TargetLandApi originalApi ) {
				this.originalApi = originalApi;
			}

			public override IEnumerable<Space> GetTargetOptions( Spirit self, GameState gameState, From from, Terrain? sourceTerrain, int range, string filterEnum ) {
				// original options
				List<Space> spaces = GetTargetOptions( self, gameState, from, sourceTerrain, range, filterEnum ).ToList();;

				// Target Spirit gains +1 range with their Powers that originate from a Mountain
				if(sourceTerrain==null || sourceTerrain == Terrain.Mountain)
					spaces.AddRange(GetTargetOptions( self, gameState, from, Terrain.Mountain, range+1, filterEnum ));
				return spaces.Distinct();
			}

			//public override Task<Space> TargetsSpace( 
			//	Spirit self, 
			//	GameState gameState, 
			//	string prompt,
			//	From from, 
			//	Terrain? sourceTerrain, 
			//	int range, 
			//	string target
			//) {
			//	if(prompt == null) prompt = "Target Space.";

			//	// original options
			//	List<Space> spaces = GetTargetOptions( self, gameState, from, sourceTerrain, range, target ).ToList();;

			//	// Target Spirit gains +1 range with their Powers that originate from a Mountain
			//	if(sourceTerrain==null || sourceTerrain == Terrain.Mountain)
			//		spaces.AddRange(GetTargetOptions( self, gameState, from, Terrain.Mountain, range+1, target ));

			//	return self.Action.Decision( new Decision.TargetSpace( prompt, spaces.Distinct(), Present.Always ));

			//}

		}


	}


}

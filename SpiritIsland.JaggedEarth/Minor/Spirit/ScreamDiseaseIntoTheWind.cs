using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class ScreamDiseaseIntoTheWind{

		public const string Name = "Scream Disease Into the Wind";

		[MinorCard(Name,1,Element.Air,Element.Water,Element.Animal),Fast,AnotherSpirit]
		static public Task ActAsync(TargetSpiritCtx ctx){
			// Target Spirit gets +1 range with all their Powers.
			ctx.GameState.TimePasses_ThisRound.Push( new PowerApiRestorer( ctx.Other ).Restore );
			ctx.Other.RangeCalc = new TargetLandApi_ExtendRange( 1, ctx.Other.RangeCalc );

			// Once this turn, after target Spirit uses a Power targeting a land, they may add 1 disease to that land.
			bool used = false;
			ctx.Other.ActionTaken_ThisRound.Add( async ( gs, args ) => {
				if( !used
					&& args.Context is TargetSpaceCtx spaceCtx 
					&& await ctx.Other.UserSelectsFirstText(Name+" ("+spaceCtx.Space.Label+")","Yes, add 1 disease", "No thank you")
				){
					used = true;
					await spaceCtx.Disease.Add( 1 );
				}
			} );

			// (Hand them a disease toekn as a reminder.)
			return Task.CompletedTask;
		}
	}

}

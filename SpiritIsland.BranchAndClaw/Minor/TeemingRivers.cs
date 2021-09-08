using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TeemingRivers {

		[MinorCard( "Teeming Rivers", 1, Speed.Slow, Element.Sun, Element.Water, Element.Plant, Element.Animal )]
		[FromSacredSite( 2, Target.MountainOrWetland )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			int blightCount = ctx.Tokens.Blight;

			if( blightCount == 0 )
				ctx.Tokens.Beasts().Count++;

			if( blightCount == 1 )
				ctx.GameState.AddBlight(ctx.Target,-1);

			return Task.CompletedTask;
		}

	}

}

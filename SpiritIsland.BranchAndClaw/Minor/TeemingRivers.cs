using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TeemingRivers {

		[MinorCard( "Teeming Rivers", 1, Speed.Slow, Element.Sun, Element.Water, Element.Plant, Element.Animal )]
		[FromSacredSite( 2, Target.MountainOrWetland )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			int blightCount = ctx.Tokens.Blight;

			// if target land has no blight, add 1 beast
			if( blightCount == 0 )
				ctx.Tokens.Beasts().Count++;

			// if target land has exactly 1 blight, remove it
			if( blightCount == 1 )
				ctx.GameState.AddBlight(ctx.Space,-1);

			return Task.CompletedTask;
		}

	}

}

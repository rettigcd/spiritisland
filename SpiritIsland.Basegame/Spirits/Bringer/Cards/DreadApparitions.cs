using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	// When powers generate fear in target land, defend 1 per fear.
	// 1 fear  (fear from to Dream a Thousands Deaths counts.
	// Fear from destroying town/cities does not.)

	public class DreadApparitions {

		[SpiritCard("Dread Apparitions",2,Element.Moon,Element.Air)]
		[Fast]
		[FromPresence(1,Target.Invaders)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {

			// When powers generate fear in target land, defend 1 per fear.
			// Fear from destroying town/cities does not.)
			void FearAdded( GameState gs, FearArgs args ) {
				if(args.cause == Cause.Power && args.space==ctx.Space)
					gs.Tokens[args.space].Defend.Add( args.count );
			}

			ctx.GameState.Fear.FearAdded.ForRound.Add(FearAdded);

			return Task.CompletedTask;
		}


	}

}

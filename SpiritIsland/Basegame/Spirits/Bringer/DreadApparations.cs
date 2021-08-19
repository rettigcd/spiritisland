using System.Threading.Tasks;

namespace SpiritIsland.Basegame.Spirits.Bringer {

	// When powers generate fear in target land, defend 1 per fear.
	// 1 fear  (fear from to Dream a Thousands Deaths counts.
	// Fear from destroying town/cities does not.)

	class DreadApparations {

		[SpiritCard("Dread Apparitions",2,Speed.Fast,Element.Moon,Element.Air)]
		[FromPresence(1,Target.Invaders)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			// 1 fear
			ctx.AddFear(1);

			// When powers generate fear in target land, defend 1 per fear.
			// 1 fear  (fear from to Dream a Thousands Deaths counts.
			// Fear from destroying town/cities does not.)

			// !!!!!!!!!!!!

			return Task.CompletedTask;
		}

	}
}

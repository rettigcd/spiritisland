﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ParalyzingFright {

		public const string Name = "Paralyzing Fright";

		[MajorCard(ParalyzingFright.Name,4,Speed.Fast,Element.Air,Element.Earth)]
		[FromSacredSite(1)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			// 4 fear
			ctx.AddFear(4);

			// invaders skip all actions in target land this turn
			ctx.GameState.SkipAllInvaderActions( ctx.Space );

			// if you have 2 air 3 earth, +4 fear
			if(ctx.YouHave("2 air,3 earth"))
				ctx.AddFear(4);

			return Task.CompletedTask;
		}


	}
}

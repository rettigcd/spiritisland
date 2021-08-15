﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class ParalyzingFright {

		[MajorCard("Paralyzing Fright",4,Speed.Fast,Element.Air,Element.Earth)]
		[FromSacredSite(1)]
		static public Task ActAsync(ActionEngine eng,Space target ) {
			var (self,gs) = eng;
			// 4 fear
			gs.AddFear(4);

			// invaders skip all actions in target land this turn
			gs.SkipAllInvaderActions(target);

			// if you have 2 air 3 earth, +4 fear
			var el = self.Elements;
			if(2<=el[Element.Air] && 3<=el[Element.Earth])
				gs.AddFear(4);
			return Task.CompletedTask;
		}


	}
}
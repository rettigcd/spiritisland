using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class ParalyzingFright {

		[MajorCard("Paralyzing Fright",4,Speed.Fast,Element.Air,Element.Earth)]
		[FromSacredSite(1)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			var (self,gs) = ctx;
			// 4 fear
			ctx.AddFear(4);

			// invaders skip all actions in target land this turn
			gs.SkipAllInvaderActions( ctx.Target );

			// if you have 2 air 3 earth, +4 fear
			if(self.Elements.Contains("2 air,3 earth"))
				ctx.AddFear(4);
			return Task.CompletedTask;
		}


	}
}

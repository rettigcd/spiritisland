using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class GraspingTide {

		// Grasping Tide => 1 => fast, range 1, cotal => moon, water => 2 fear, defend 4
		[SpiritCard("Grasping Tide",1,Speed.Fast,Element.Moon,Element.Water)]
		[FromPresence(1,Target.Costal)]
		static public Task Act(TargetSpaceCtx ctx ) {
			ctx.AddFear(2);
			ctx.Defend(4);
			return Task.CompletedTask;
		}

	}
}

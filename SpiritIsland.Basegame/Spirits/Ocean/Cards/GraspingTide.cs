namespace SpiritIsland.Basegame;

public class GraspingTide {

	// Grasping Tide => 1 => fast, range 1, cotal => moon, water => 2 fear, defend 4
	[SpiritCard("Grasping Tide",1,Element.Moon,Element.Water)]
	[Fast]
	[FromPresence(1,Target.Coastal)]
	static public Task Act(TargetSpaceCtx ctx ) {
		ctx.AddFear(2);
		ctx.Defend(4);
		return Task.CompletedTask;
	}

}

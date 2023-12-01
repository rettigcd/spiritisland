namespace SpiritIsland.Basegame;

public class GraspingTide {

	[SpiritCard("Grasping Tide",1,Element.Moon,Element.Water),Fast,FromPresence(1,Filter.Coastal)]
	[Instructions( "2 Fear. Defend 4." ), Artist( Artists.JoshuaWright )]
	static public Task Act(TargetSpaceCtx ctx ) {
		ctx.AddFear(2);
		ctx.Defend(4);
		return Task.CompletedTask;
	}

}

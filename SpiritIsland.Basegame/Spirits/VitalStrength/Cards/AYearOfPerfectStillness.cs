namespace SpiritIsland.Basegame;

public class AYearOfPerfectStillness {

	[SpiritCard("A Year of Perfect Stillness",3,Element.Sun,Element.Earth)]
	[Fast]
	[FromPresence(1)]
	static public Task Act(TargetSpaceCtx ctx){
		ctx.SkipAllInvaderActions();
		return Task.CompletedTask;
	}

}
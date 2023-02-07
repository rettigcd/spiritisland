namespace SpiritIsland.Basegame;

public class AYearOfPerfectStillness {

	const string Name = "A Year of Perfect Stillness";

	[SpiritCard( Name,3,Element.Sun,Element.Earth)]
	[Fast]
	[FromPresence(1)]
	static public Task Act(TargetSpaceCtx ctx){
		ctx.Tokens.SkipAllInvaderActions(Name);
		return Task.CompletedTask;
	}

}
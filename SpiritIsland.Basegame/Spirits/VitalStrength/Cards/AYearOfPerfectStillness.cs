namespace SpiritIsland.Basegame;

public class AYearOfPerfectStillness {

	public const string Name = "A Year of Perfect Stillness";

	[SpiritCard( Name,3,Element.Sun,Element.Earth),Fast,FromPresence(1)]
	[Instructions( "Invaders skip all Actions in target land this turn." ), Artist( Artists.SydniKruger )]
	static public Task Act(TargetSpaceCtx ctx){
		ctx.Space.SkipAllInvaderActions(Name);
		return Task.CompletedTask;
	}

}
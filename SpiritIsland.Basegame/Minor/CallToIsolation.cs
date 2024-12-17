namespace SpiritIsland.Basegame;

public class CallToIsolation {

	[MinorCard("Call to Isolation",0,Element.Sun,Element.Air,Element.Animal),Fast,FromPresence(1,Filter.Dahan)]
	[Instructions( "Push 1 Explorer / Town per Dahan. -or- Push 1 Dahan." ), Artist( Artists.GrahamStermberg )]
	static public Task ActAsync(TargetSpaceCtx ctx){

		return ctx.SelectActionOption(
			Cmd.PushNDahan(1),
			Cmd.PushExplorersOrTowns( ctx.Dahan.CountAll )
		);

	}

}
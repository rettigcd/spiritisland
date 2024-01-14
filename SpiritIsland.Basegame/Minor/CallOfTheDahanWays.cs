namespace SpiritIsland.Basegame;

public class CallOfTheDahanWays {

	[MinorCard("Call of the Dahan Ways",1,Element.Moon,Element.Water,Element.Animal), Slow, FromPresence(1,Filter.Dahan)]
	[Instructions( "Replace 1 Explorer with 1 Dahan. -If you have- 2 Moon: You may instead replace 1 Town with 1 Dahan." ), Artist( Artists.LoicBelliau )]
	static public async Task Act(TargetSpaceCtx ctx){

		HumanToken oldInvader = null;

		// if you have 2 moon, you may instead replace 1 town with 1 dahan
		if(ctx.Tokens.Has(Human.Town) && await ctx.YouHave("2 moon" ))
			oldInvader = ctx.Tokens.HumanOfTag( Human.Town ).First();
		else if(ctx.Tokens.Has( Human.Explorer ))
			oldInvader = ctx.Tokens.HumanOfTag( Human.Explorer ).First();

		await ctx.Tokens.ReplaceHumanAsync( oldInvader, Human.Dahan );

	}

}
namespace SpiritIsland.Basegame;

public class CallOfTheDahanWays {

	[MinorCard("Call of the Dahan Ways",1,Element.Moon,Element.Water,Element.Animal), Slow, FromPresence(1,Filter.Dahan)]
	[Instructions( "Replace 1 Explorer with 1 Dahan. -If you have- 2 Moon: You may instead replace 1 Town with 1 Dahan." ), Artist( Artists.LoicBelliau )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		// if you have 2 moon, you may instead replace 1 town with 1 dahan
		HumanTokenClass[] tokenClasses = ctx.Space.Has(Human.Town) && await ctx.YouHave("2 moon")
			? [Human.Town] 
			: Human.Explorer_Town;

		HumanToken? oldInvader = ctx.Space.HumanOfAnyTag( tokenClasses ).FirstOrDefault();
		if(oldInvader is not null)
			await ctx.Space.ReplaceHumanAsync( oldInvader, Human.Dahan );
	}

}
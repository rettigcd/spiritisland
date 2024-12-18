namespace SpiritIsland.NatureIncarnate;

public class SwallowedByTheEndlessDark {

	const string Name = "Swallowed by the Endless Dark";

	[SpiritCard( Name, 1, Element.Moon, Element.Air, Element.Water ), Fast, FromPresence(0,Filter.Invaders)]
	[Instructions( "2 Fear. Abduct 1 Explorer." ), Artist( Artists.DavidMarkiwsky )]
	static async public Task ActAsync( TargetSpaceCtx ctx ) {

		// 2 Fear.
		await ctx.AddFear(2);

		// Abduct 1 Explorer.
		var options = ctx.Space.SpaceTokensOfTag(Human.Explorer);
		SpaceToken? explorer = await ctx.SelectAsync(new A.SpaceTokenDecision("Abduct Explorer",options,Present.Always));
		if(explorer is not null)
			await explorer.MoveTo(EndlessDark.Space.ScopeSpace);
	}

}

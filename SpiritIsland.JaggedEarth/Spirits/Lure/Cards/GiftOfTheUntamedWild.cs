namespace SpiritIsland.JaggedEarth;

public class GiftOfTheUntamedWild {

	[SpiritCard("Gift of the Untamed Wild",0,Element.Moon,Element.Fire,Element.Air,Element.Plant),Slow,AnySpirit]
	[Instructions( "Target Spirit chooses to either: add 1 Wilds to one of their lands. -or- Replace 1 of their Presence with 1 Disease." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync(TargetSpiritCtx ctx ) {

		// target spirit chooses to either: 
		return ctx.OtherCtx.SelectActionOption(
			// Add 1 wilds to one of their lands
			new SelfCmd("Add 1 wilds to one of your lands", Add1WildsToOneOfYourLands ),
			// Replace 1 of their presence with 1 disease.
			new SelfCmd("Replace 1 of your presence with 1 disease", Replace1PresenceWith1Disease)
		);
	}

	static async Task Add1WildsToOneOfYourLands( SelfCtx ctx ) {
		var spaceCtx = await ctx.SelectSpace("Add 1 Wilds",ctx.Self.Presence.Spaces );
		await spaceCtx.Wilds.Add(1);
	}

	static async Task Replace1PresenceWith1Disease( SelfCtx ctx ) {
		var space = await ctx.Self.SelectDeployed("Replace Presence with 1 disease");
		await ctx.Self.Presence.Token.RemoveFrom( space ); // !!! upgrade to handle Incarna presence
		await ctx.Target(space).Disease.Add(1);
	}

}
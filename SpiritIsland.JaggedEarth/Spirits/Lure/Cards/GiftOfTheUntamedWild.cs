namespace SpiritIsland.JaggedEarth;

public class GiftOfTheUntamedWild {

	[SpiritCard("Gift of the Untamed Wild",0,Element.Moon,Element.Fire,Element.Air,Element.Plant),Slow,AnySpirit]
	[Instructions( "Target Spirit chooses to either: add 1 Wilds to one of their lands. -or- Replace 1 of their Presence with 1 Disease." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpiritCtx ctx ) {

		// target spirit chooses to either: 
		await Cmd.Pick1(
			// Add 1 wilds to one of their lands
			new SpiritAction("Add 1 wilds to one of your lands", Add1WildsToOneOfYourLands ),
			// Replace 1 of their presence with 1 disease.
			new SpiritAction("Replace 1 of your presence with 1 disease", Replace1PresenceWith1Disease)
		).ActAsync(ctx.Other);
	}

	static async Task Add1WildsToOneOfYourLands( Spirit self ) {
		var space = (await self.SelectSpaceAsync("Add 1 Wilds",self.Presence.Lands,Present.Always ))!;
		await space.Wilds.AddAsync(1);
	}

	static async Task Replace1PresenceWith1Disease( Spirit self ) {
		var spaceToken = await self.SelectDeployed("Replace Presence with 1 disease");
		await spaceToken.RemoveAsync(); // !!! upgrade to handle Incarna presence
		await spaceToken.Space.Disease.AddAsync(1);
	}

}
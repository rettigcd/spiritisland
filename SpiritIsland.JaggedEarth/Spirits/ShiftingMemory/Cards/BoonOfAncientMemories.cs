namespace SpiritIsland.JaggedEarth;

public class BoonOfAncientMemories {

	public const string Name = "Boon of Ancient Memories";

	[SpiritCard(BoonOfAncientMemories.Name,1,Element.Moon,Element.Water,Element.Earth,Element.Plant), Slow, AnySpirit]
	[Instructions( "If you target yourself, gain a Minor Power. Otherwise: Target Spirit gains a Power Card. If it's a Major Power, they may pay 2 Energy instead of Forgetting a Power Card." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpiritCtx ctx ) { 

		// if you target yourself
		if(ctx.Other == ctx.Self) { 
			// Draw minor
			await ctx.Self.DrawMinor();
			return;
		}

		// Otherwise: Target Spirit gains a Power Card.
		await ctx.Other.Draw( async deck => {
			// If it's a Major Power, they may pay 2 Energy instead of Forgetting a Power Card.
			if(2 <= ctx.Other.Energy && await ctx.Other.UserSelectsFirstText( "Pay for Major Card", "2 energy", "forget a card" )) {
				ctx.Other.Energy -= 2;
				return false;
			}
			return true;
		} );

	}

}
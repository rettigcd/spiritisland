namespace SpiritIsland.JaggedEarth;

public class BoonOfAncientMemories {

	public const string Name = "Boon of Ancient Memories";

	[SpiritCard(BoonOfAncientMemories.Name,1,Element.Moon,Element.Water,Element.Earth,Element.Plant), Slow, AnySpirit]
	static public async Task ActAsync(TargetSpiritCtx ctx ) { 

		// if you target yourself
		if(ctx.Other == ctx.Self) { 
			// Draw minor
			await ctx.DrawMinor();
			return;
		}

		// Otherwise: Target Spirit gains a Power Card.
		var powerType = await DrawFromDeck.SelectPowerCardType( ctx.Self );

		if( powerType == PowerType.Minor )
			await ctx.OtherCtx.DrawMinor();
		else {
			await ctx.OtherCtx.DrawMajor(false, 4);
			// If it's a Major Power, they may pay 2 Energy instead of Forgetting a Power Card.
			if( 2 <= ctx.Other.Energy && await ctx.Other.UserSelectsFirstText( "Pay for Major Card", "2 energy", "forget a card" ))
				ctx.Other.Energy -= 2;
			else
				await ctx.Other.ForgetPowerCard_UserChoice();
		}
	}

}
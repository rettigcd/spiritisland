﻿using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.ShiftingMemory {
	public class BoonOfAncientMemories {

		[SpiritCard("Boon of Ancient Memories",1,Element.Moon,Element.Water,Element.Earth,Element.Plant), Slow, AnySpirit]
		static public async Task ActAsync(TargetSpiritCtx ctx ) { 

			// if you target yourself
			if(ctx.Other == ctx.Self) { 
				// Draw minor
				await ctx.Self.DrawMinor(ctx.GameState);
				return;
			}

			// Otherwise: Target Spirit gains a Power Card.
			if(await ctx.Self.UserSelectsFirstText( "Which type do you wish to draw", "minor", "major" ))
				await ctx.OtherCtx.DrawMinor();
			else {
				await ctx.OtherCtx.DrawMajor(4, false);
				// If it's a Major Power, they may pay 2 Energy instead of Forgetting a Power Card.
				if( ctx.Other.Energy>=2 && await ctx.Other.UserSelectsFirstText( "Pay for Major Card", "2 energy", "forget a card" ))
					ctx.Other.Energy -= 2;
				else
					await ctx.Other.ForgetPowerCard();
			}
		}

	}

}

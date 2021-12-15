﻿using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class BatsScoutForRaidsByDarkness{ 
		
		[MinorCard("Bats Scout for Raids by Darkness",1,Element.Moon,Element.Air,Element.Animal),Slow,FromPresence(2)]
		static public Task ActAsync(TargetSpaceCtx ctx){
			return ctx.SelectActionOption( 
				new ActionOption("For each dahan, 1 Damage to town/city.", EachDahanDamagesTownOrCiry ),
				new ActionOption("1 Fear. Gather up to 2 Dahan", OneFearAndGatherUpTo2Dahan )
			);
		}

		static Task EachDahanDamagesTownOrCiry(TargetSpaceCtx ctx ) => ctx.DamageInvaders(ctx.Dahan.Count,Invader.Town,Invader.City);

		static Task OneFearAndGatherUpTo2Dahan(TargetSpaceCtx ctx ) {
			ctx.AddFear(1); 
			return ctx.GatherUpToNDahan(2);
		}

	}

}

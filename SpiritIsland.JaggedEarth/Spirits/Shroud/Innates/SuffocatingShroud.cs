﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Suffocating Shroud"), Slow, FromPresence(0)]
	public class SuffocatingShroud {

		[InnateOption("1 moon,2 air,1 water","1 Damage.")]
		static public Task Option1(TargetSpaceCtx ctx){ 
			return ctx.DamageInvaders(1);
		}

		[InnateOption("2 moon,3 air,2 water","For each adjacent land with your presence, 1 Damage to a different Invader.")]
		static public Task Option2(TargetSpaceCtx ctx){ 
			int count = ctx.Space.Adjacent.Count(a=>ctx.Target(a).Presence.IsHere);
			return ctx.Apply1DamageToDifferentInvaders( count );
		}

		[InnateOption("4 moon,4 air,3 water","1 Damage")]
		static public Task Option3(TargetSpaceCtx ctx){
			return ctx.DamageInvaders(1);
		}

		[InnateOption("5 moon,6 air,4 water","1 Damage to each Invader")]
		static public Task Option4(TargetSpaceCtx ctx){
			return ctx.DamageEachInvader(1);
		}

	}

}

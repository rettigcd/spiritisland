using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class FlamesFury{

		[SpiritCard("Flame's Fury",0,Element.Sun,Element.Fire,Element.Plant),Fast,TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {

			// Target Spirit gains 1 energy.
			++ctx.Other.Energy;

			// Target Spirit does +1 damage for each damage-dealing power
			var oldDamageStrategy = ctx.Other.CustomDamageStrategy;
			ctx.Other.CustomDamageStrategy = new FlamesFuryDamage(ctx.Other);
			ctx.GameState.TimePasses_ThisRound.Push( ( gs ) => {
				ctx.Other.CustomDamageStrategy = oldDamageStrategy;
				return Task.CompletedTask;
			} );

			return Task.CompletedTask;
		}

	}

	class FlamesFuryDamage : IDamageApplier {
		Spirit spirit;
		public FlamesFuryDamage(Spirit spirit) {
			this.spirit = spirit;
		}

		HashSet<Guid> actionsThatUsedExtraDamage = new HashSet<Guid>();
		public Token ApplyDamage( TokenCountDictionary tokens, int availableDamage, Token invaderToken ) {
			if( availableDamage<invaderToken.Health
				&& !actionsThatUsedExtraDamage.Contains(spirit.CurrentActionId)
			) {
				// !!! this allows additional 1 point damage to single item, but it does not allow additional items to be hit
				// !!! To make this work correctly, we may need to pull damage methods out of InvaderGroup and override them all
				++availableDamage;  
				actionsThatUsedExtraDamage.Add(spirit.CurrentActionId);
			}


			var damagedInvader = invaderToken.ResultingDamagedInvader( availableDamage );
			tokens.Adjust( invaderToken, -1 );
			if(0 < damagedInvader.Health) // only track alive invaders
				tokens.Adjust( damagedInvader, 1 );
			return damagedInvader;
		}
	}
}

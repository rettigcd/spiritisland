using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	static public class SmartInvaderDamageExtensions {

		// When we swap this out for user choosing, Which user chooses when dahan are doing damage????
		static public async Task SmartRavageDamage( this InvaderGroup grp, int startingdamage, List<string> log = null ) {
			int damagetoinvaders = startingdamage;

			while(damagetoinvaders > 0 && grp.Tokens.HasInvaders()) {
				Token invadertodamage = grp.Tokens.PickSmartInvaderToDamage( damagetoinvaders );
				damagetoinvaders -= await grp.ApplyDamageTo1( damagetoinvaders, invadertodamage );
			}
			if(log != null) log.Add( $"{startingdamage} damage to invaders leaving {grp.Tokens.InvaderSummary}." );
		}

		static Token PickSmartInvaderToDamage( this TokenCountDictionary counts, int availableDamage, params TokenGroup[] invaderGeneric ) { // $$
			var candidates = counts.Invaders();
			if(invaderGeneric != null && invaderGeneric.Length > 0)
				candidates = candidates.Where( i => invaderGeneric.Contains( i.Generic ) );

			return PickItemToKill( candidates, availableDamage )
				?? PickItemToDamage( candidates );
		}

		static Token PickItemToKill(IEnumerable<Token> candidates, int availableDamage) {
			return candidates
				.Where( specific => specific.Health <= availableDamage ) // can be killed
				.OrderByDescending( k => k.FullHealth ) // pick items with most Full Health
				.ThenBy( k => k.Health ) // and most damaged.
				.FirstOrDefault();
		}

		static Token PickItemToDamage( IEnumerable<Token> candidates ) {
			return candidates
				.OrderBy(i=>i.Health) // closest to dead
				.ThenByDescending(i=>i.FullHealth) // biggest impact
				.First();
		}

	}

}
namespace SpiritIsland.NatureIncarnate;

public class ReachFromTheInfiniteDarkness {
	const string Name = "Reach from the Infinite Darkness";

	[SpiritCard( Name, 0, Element.Moon, Element.Air, Element.Animal ), Fast, Yourself]
	[Instructions( "Abduct up to 2 presence (of any Spirits, with permission) from any lands on the island, ignoring land type restrictions on moving presence. Each Spirit's presence in endless-dark grants them +1 with all their Powers (this turn)." ), Artist( Artists.DavidMarkiwsky )]
	static async public Task ActAsync( Spirit self ) {

		CountDictionary<Spirit> bonuses = [];

		// Abduct up to 2 presence( of any Spirits, w/ permission ) from any lands on the island, ignoring land type restrictions on moving presence.
		int remaining = 2;
		while(0 < remaining) {

			// select presence
			var presenceToAbduct = await self.SelectAsync( 
				new A.SpaceTokenDecision( $"Abduct Presence for +1 Range for all powers ({remaining} remaining)", 
				GameState.Current.Spirits.SelectMany(s=>s.Presence.Deployed),
				Present.Done 
			) );

			if(presenceToAbduct == null) break;

			// get consent
			Spirit? otherSpirit = presenceToAbduct.Token is SpiritPresenceToken spt ? spt.Self 
				: presenceToAbduct.Token is Incarna i ? i.Self
				: throw new InvalidOperationException("token was not a presence/incarna");
			if(!await SpiritConsentsToAbduction( self, presenceToAbduct, otherSpirit ))
				continue; // try again

			// move it
			await presenceToAbduct.MoveTo( EndlessDark.Space.ScopeSpace );

			// record the bonus
			bonuses[otherSpirit]++;

			// next
			--remaining;
		}

		// Each Spirit's presence in () grants them +1 with all their Powers (this turn).
		GrantRangeBonus( bonuses );
	}

	static void GrantRangeBonus( CountDictionary<Spirit> bonuses ) {
		foreach(Spirit spirit in bonuses.Keys) {
			RangeCalcRestorer.Save( spirit );
			RangeExtender.Extend( spirit, bonuses[spirit] );
		}
	}

	static async Task<bool> SpiritConsentsToAbduction( Spirit self, SpaceToken presenceToAbduct, Spirit other ) 
		=> other == self 
		|| await other.UserSelectsFirstText( $"Allow {presenceToAbduct} to be abducted for +1 Range on powers this turn", "Yes", "No" );

}

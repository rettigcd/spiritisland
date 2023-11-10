namespace SpiritIsland.NatureIncarnate;

public class ReachFromTheInfiniteDarkness {
	const string Name = "Reach from the Infinite Darkness";

	[SpiritCard( Name, 0, Element.Moon, Element.Air, Element.Animal ), Fast, Yourself]
	[Instructions( "Abduct up to 2 presence (of any Spirits, with permission) from any lands on the island, ignoring land type restrictions on moving presence. Each Spirit's presence in endless-dark grants them +1 with all their Powers (this turn)." ), Artist( Artists.DavidMarkiwsky )]
	static async public Task ActAsync( SelfCtx ctx ) {

		CountDictionary<Spirit> bonuses = new CountDictionary<Spirit>();

		// Abduct up to 2 presence( of any Spirits, w/ permission ) from any lands on the island, ignoring land type restrictions on moving presence.
		int remaining = 2;
		while(0 < remaining) {

			// select presence
			var (presenceToAbduct, other) = await SelectPresenceToAbduct( ctx, remaining );
			if(presenceToAbduct == null || other == null) break;

			// get consent
			if(!await SpiritConsentsToAbduction( ctx, presenceToAbduct, other ))
				continue; // try again

			// move it
			await presenceToAbduct.MoveTo( EndlessDark.Space );

			// record the bonus
			bonuses[other]++;

			// next
			--remaining;
		}

		// Each Spirit's presence in () grants them +1 with all their Powers (this turn).
		GrantRangeBonus( ctx, bonuses );
	}

	static void GrantRangeBonus( SelfCtx ctx, CountDictionary<Spirit> bonuses ) {
		foreach(var spirit in bonuses.Keys) {
			RangeCalcRestorer.Save( spirit );
			RangeExtender.Extend( spirit, bonuses[spirit] );
		}
	}

	static async Task<bool> SpiritConsentsToAbduction( SelfCtx ctx, SpaceToken presenceToAbduct, Spirit other ) 
		=> other == ctx.Self 
		|| await other.UserSelectsFirstText( $"Allow {presenceToAbduct} to be abducted for +1 Range on powers this turn", "Yes", "No" );

	static async Task<(SpaceToken?,Spirit?)> SelectPresenceToAbduct( SelfCtx ctx, int remaining ) {
		var spiritLookup = new Dictionary<SpaceToken, Spirit>(); // !! THis is stupid, there should be a link from the presence/incarna back to the spirit.

		foreach(Spirit spirit in GameState.Current.Spirits)
			foreach(SpaceToken st in spirit.Presence.Deployed)
				spiritLookup.Add( st, spirit );
		var presenceToAbduct = await ctx.SelectAsync( new A.SpaceToken( $"Abduct Presence for +1 Range for all powers ({remaining} remaining)", spiritLookup.Keys, Present.Done ) );
		var selectedSpirit = presenceToAbduct != null ? spiritLookup[presenceToAbduct] : null;
		return (presenceToAbduct, selectedSpirit);
	}
}

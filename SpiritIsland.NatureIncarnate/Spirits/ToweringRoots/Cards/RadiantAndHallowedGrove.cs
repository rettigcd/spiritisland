namespace SpiritIsland.NatureIncarnate;

public class RadiantAndHallowedGrove {
	const string Name = "Radiant and Hallowed Grove";

	[SpiritCard( Name, 2, Element.Sun, Element.Moon, Element.Fire, Element.Plant ), Fast, FromIncarna] 
	[Instructions( "2 Fear if Invaders are present or adjacent. In both target and one adjacent land, you may Remove an Invader with Health less than or equal to the Terror Level." ), Artist( Artists.AalaaYassin )]
	static async public Task ActAsync( TargetSpaceCtx ctx ) {

		// 2 Fear if Invaders are present or adjacent.
		var spaceState = ctx.Tokens;
		if(spaceState.InOrAdjacentTo.Any( x => x.HasInvaders() ))
			ctx.AddFear( 2 );

		// In both target and one adjacent land,
		await InBothTargetAnd1Adjacent( ctx.Tokens )
			// an Invader with Health less than or equal to the Terror Level.
			.AddGroup( 1 + 1, AnInvaderWithHealthLessThanOrEqualToTerrorLevel() )
			.RemoveUpToN( ctx.Self );
	}

	static SourceSelector InBothTargetAnd1Adjacent( SpaceState target ) {
		bool removedFromAdjacent = false;
		return new SourceSelector( target.InOrAdjacentTo )
			.Track( st => removedFromAdjacent = (st.Space.ScopeTokens != target) )
			.FilterSource( ss => !removedFromAdjacent || ss == target );
	}

	static HumanTokenClass[] AnInvaderWithHealthLessThanOrEqualToTerrorLevel() {
		return GameState.Current.Fear.TerrorLevel switch {
			1 => new HumanTokenClass[] { Human.Explorer },
			2 => Human.Explorer_Town,
			_ => Human.Invader
		};
	}

}

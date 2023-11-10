namespace SpiritIsland.NatureIncarnate;

public class RadiantAndHallowedGrove {
	const string Name = "Radiant and Hallowed Grove";

	[SpiritCard( Name, 2, Element.Sun, Element.Moon, Element.Fire, Element.Plant ), Fast, FromIncarna] 
	[Instructions( "2 Fear if Invaders are present or adjacent. In both target and one adjacent land, you may Remove an Invader with Health less than or equal to the Terror Level." ), Artist( Artists.AalaaYassin )]
	static async public Task ActAsync( TargetSpaceCtx ctx ) {

		// 2 Fear if Invaders are present or adjacent.
		var spaceState = ctx.Space.Tokens;
		if(spaceState.InOrAdjacentTo.Any(x=>x.HasInvaders()))
			ctx.AddFear(2);

		// In both target and one adjacent land, you may Remove an Invader with Health less than or equal to the Terror Level.
		var removableClasses = GameState.Current.Fear.TerrorLevel switch {
			1 => new HumanTokenClass[] { Human.Explorer },
			2 => Human.Explorer_Town,
			_ => Human.Invader
		};

		// Present all candidates in target or adjacent
		var removeableTokens = spaceState.InOrAdjacentTo.SelectMany(s=>s.SpaceTokensOfAnyClass( removableClasses )).ToArray();

		for(int i = 0; i < 2; ++i) {
			var sp = await ctx.SelectAsync( new A.SpaceToken( "Select invader to remove", removeableTokens, Present.Done ) );
			if(sp==null) break;
			await sp.Space.Tokens.Remove(sp.Token,1);
			removeableTokens = (sp.Space == ctx.Space) // if we selected in target
				? spaceState.Adjacent.SelectMany( s => s.SpaceTokensOfAnyClass( removableClasses ) ).ToArray() // use only adjacents
				: spaceState.SpaceTokensOfAnyClass( removableClasses ).ToArray(); // use only target
		}

	}

}

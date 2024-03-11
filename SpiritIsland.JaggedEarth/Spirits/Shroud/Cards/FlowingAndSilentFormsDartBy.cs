namespace SpiritIsland.JaggedEarth;

public class FlowingAndSilentFormsDartBy {

	[SpiritCard("Flowing and Silent Forms Dart By",0, Element.Moon,Element.Air,Element.Water), Fast, FromPresence(0) ]
	[Instructions( "2 Fear if Invaders are present. When Presence in target land would be Destroyed, its owner may, if possible, instead Push that Presence. You may Gather 1 Presence / Sacred Site of another Spirit (with their permission)." ), Artist( Artists.EmilyHancock )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// 2 fear if Invaders are present
		if(ctx.HasInvaders)
			ctx.AddFear( 2 );

		// When presence in target land would be Destroyed, its owner may, if possible instead Push that presence.
		// (do it for all spirits, not just the ones currently here)
		ctx.Tokens.Init(new PushPresenceInsteadOfDestroy(),1);

		// You may Gather 1 presence / Sacred site of another Spirit (with their permission).
		await GatherSomeonesPresence( ctx );

	}

	/// <summary>
	/// Allows presence to be pushed when it normally would be destroyed.
	/// </summary>
	class PushPresenceInsteadOfDestroy : IModifyRemovingTokenAsync, IEndWhenTimePasses {

		async Task IModifyRemovingTokenAsync.ModifyRemovingAsync( RemovingTokenArgs args ) {
			if( !args.Reason.IsDestroyingPresence()
				|| args.Token is not SpiritPresenceToken spt 
			) return;

			Spirit spirit = spt.Self;

			if( !args.From.Has(spirit.Presence) ) return;
			
			var dst = await spirit.SelectAsync( new A.Space( "Instead of destroying, push presence to:", args.From.Adjacent.Downgrade(), Present.Done ) );
			if(dst == null) return;

			while(0 < args.Count--)
				await args.Token.MoveAsync(args.From.Space,dst);
		}
	}

	static async Task GatherSomeonesPresence( TargetSpaceCtx ctx ) {
		var adj = ctx.Tokens.Adjacent;
		// Pick Spirit
		Spirit[] spirits = GameState.Current.Spirits;
		var nearbySpirits = spirits.Where( s => adj.Any( s.Presence.IsOn ) ).ToArray();
		Spirit other = spirits.Length == 1 ? ctx.Self
			: await ctx.SelectAsync( new A.Spirit( "Flowing and Silent Forms Dart By", nearbySpirits ) );
		// Pick spot
		var options = adj.Where(adj=>adj.Has(other.Presence));

		var source = await ctx.SelectAsync( A.SpaceToken.ToCollect( "Gather presence", other.Presence.Movable.WhereIsOn(adj), Present.Done, ctx.Space ) );//

		if(source == null) return;
		// # to move
		int numToMove = (1 < other.Presence.CountOn(source.Space.ScopeTokens) && await ctx.Self.UserSelectsFirstText("# of presence to gather", "2", "1"))
			? 2
			: 1;
		// Get permission
		if(other != ctx.Self && !await ctx.Self.UserSelectsFirstText($"Allow Shroud to Gather {numToMove} of your presence {source.Space.Label} => {ctx.Space.Label} ?", "Yes, please", "No, I don't want to move" ))
			return; // cancel

		// move
		while(numToMove-->0)
			await source.MoveTo(ctx.Tokens);
	}

}
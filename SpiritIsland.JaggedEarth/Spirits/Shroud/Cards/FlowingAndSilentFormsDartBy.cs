namespace SpiritIsland.JaggedEarth;

public class FlowingAndSilentFormsDartBy {

	[SpiritCard("Flowing and Silent Forms Dart By",0, Element.Moon,Element.Air,Element.Water), Fast, FromPresence(0) ]
	[Instructions( "2 Fear if Invaders are present. When Presence in target land would be Destroyed, its owner may, if possible, instead Push that Presence. You may Gather 1 Presence / Sacred Site of another Spirit (with their permission)." ), Artist( Artists.EmilyHancock )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// 2 fear if Invaders are present
		if(ctx.HasInvaders)
			await ctx.AddFear( 2 );

		// When presence in target land would be Destroyed, its owner may, if possible instead Push that presence.
		// (do it for all spirits, not just the ones currently here)
		ctx.Space.Init(new PushPresenceInsteadOfDestroy(),1);

		// You may Gather 1 presence / Sacred site of another Spirit (with their permission).
		await GatherSomeonesPresence( ctx );

	}

	/// <summary>
	/// Allows presence to be pushed when it normally would be destroyed.
	/// </summary>
	class PushPresenceInsteadOfDestroy : ISpaceEntity, IModifyRemovingToken, IEndWhenTimePasses {

		async Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
			if( !args.Reason.IsDestroyingPresence()
				|| args.Token is not SpiritPresenceToken spiritPresenceToken 
			) return;

			Spirit spirit = spiritPresenceToken.Self;

			if( !args.From.Has(spirit.Presence) ) return; // this should never happen.
			
			var dst = await spirit.SelectAsync( new A.SpaceDecision( "Instead of destroying, push presence to:", args.From.Adjacent, Present.Done ) );
			if(dst is null) return;

			while(0 < args.Count) {
				--args.Count; // must be inside while to avoid setting to -1
				await args.Token.MoveAsync(args.From,dst);
			}
		}
	}

	static async Task GatherSomeonesPresence( TargetSpaceCtx ctx ) {
		var adj = ctx.Space.Adjacent;

		// Pick Spirit
		Spirit[] spirits = GameState.Current.Spirits;
		var nearbySpirits = spirits.Where( s => adj.Any( s.Presence.IsOn ) ).ToArray();
		Spirit? other = spirits.Length == 1 ? ctx.Self
			: await ctx.SelectAsync( new A.Spirit( "Flowing and Silent Forms Dart By", nearbySpirits ) );
		if(other is null) return; // no spirit to gather

		// Pick spot
		var options = adj.Where(adj=>adj.Has(other.Presence));
		var source = await ctx.SelectAsync( A.SpaceTokenDecision.ToCollect( "Gather presence", other.Presence.Movable.WhereIsOn(adj), Present.Done, ctx.SpaceSpec ) );
		if(source is null) return; // in case they cancel / change their mind.

		// # to move
		int numToMove = (1 < other.Presence.CountOn(source.Space) && await ctx.Self.UserSelectsFirstText("# of presence to gather", "2", "1"))
			? 2
			: 1;
		// Get permission
		if(other != ctx.Self && !await ctx.Self.UserSelectsFirstText($"Allow Shroud to Gather {numToMove} of your presence {source.Space.SpaceSpec.Label} => {ctx.SpaceSpec.Label} ?", "Yes, please", "No, I don't want to move" ))
			return; // cancel

		// move
		while(numToMove-->0)
			await source.MoveTo(ctx.Space);
	}

}
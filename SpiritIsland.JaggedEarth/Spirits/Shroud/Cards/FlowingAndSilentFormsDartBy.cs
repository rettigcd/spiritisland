namespace SpiritIsland.JaggedEarth;

public class FlowingAndSilentFormsDartBy {

	[SpiritCard("Flowing and Silent Forms Dart By",0, Element.Moon,Element.Air,Element.Water), Fast, FromPresence(0) ]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		// 2 fear if Invaders are present
		if(ctx.HasInvaders)
			ctx.AddFear( 2 );

		// When presence in target land would be Destroyed, its owner may, if possible instead Push that presence.
		// (do it for all spirits, not just the ones currently here)
		ctx.Tokens.Init(new PushInsteadOfDestroy(),1);

		// You may Gather 1 presence / Sacred site of another Spirit (with their permission).
		await GatherSomeonesPresence( ctx );

	}


	class PushInsteadOfDestroy : IModifyRemovingTokenAsync, IEndWhenTimePasses {
		public IEntityClass Class => ActionModTokenClass.Mod;

		public async Task ModifyRemovingAsync( RemovingTokenArgs args ) {
			if( args.Mode == RemoveMode.Test
				|| !args.Reason.IsDestroyingPresence()
				|| args.Token is not SpiritPresenceToken spt 
				|| !spt.CanMove
			) return;

			GameState gs = GameState.Current;
			Spirit spirit = gs.Spirits.First( s => s.Token == args.Token );

			if( !spirit.Presence.HasMovableTokens( args.From ) ) return;
			
			var dst = await spirit.Gateway.Decision( new Select.ASpace( "Instead of destroying, push presence to:", args.From.Adjacent.Downgrade(), Present.Done ) );
			if(dst == null) return;

			while(0 < args.Count--)
				await args.From.MoveTo(args.Token, dst);
		}
	}

	static async Task GatherSomeonesPresence( TargetSpaceCtx ctx ) {
		var adj = ctx.Tokens.Adjacent;
		// Pick Spirit
		var nearbySpirits = ctx.GameState.Spirits.Where( s => adj.Any( s.Presence.IsOn ) ).ToArray();
		Spirit other = ctx.GameState.Spirits.Length == 1 ? ctx.Self
			: await ctx.Decision( new Select.ASpirit( "Flowing and Silent Forms Dart By", nearbySpirits ) );
		// Pick spot
		var options = adj.Where(other.Presence.HasMovableTokens);
		var source = (await ctx.Decision( Select.DeployedPresence.Gather("Gather presence", ctx.Space, options, other.Token ) ))?.Space;
		if(source == null) return;
		// # to move
		int numToMove = (1 < source.Tokens[other.Token] && await ctx.Self.UserSelectsFirstText("# of presence to gather", "2", "1"))
			? 2
			: 1;
		// Get permission
		if(other != ctx.Self && await ctx.Self.UserSelectsFirstText($"Allow Shroud to Gather {numToMove} of your presence {source.Label} => {ctx.Space.Label} ?", "No, I don't want to move", "Yes, please"))
			return; // cancel

		// move
		while(numToMove-->0)
			await other.Token.Move(source,ctx.Tokens);
	}

}
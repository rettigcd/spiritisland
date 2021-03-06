namespace SpiritIsland.BranchAndClaw;

[InnatePower( "Spreading Wilds" ),Slow]
[SpreadingWilds.FromPresence]
public class SpreadingWilds {

	[InnateOption( "2 sun", "Push 1 explorer from target land per 2sun you have.", 0 )]
	static public Task Option1( TargetSpaceCtx ctx ) {
		// push 1 explorer from target land per 2 sun you have
		return ctx.Push(ctx.Self.Elements[Element.Sun]/2,Invader.Explorer);
	}

	[InnateOption( "1 plant", "If target land has no explorer, add 1 wilds.", 1 )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		// if target land has no explorer, add 1 wilds
		if( !ctx.Tokens.HasAny(Invader.Explorer) )
			await ctx.Wilds.Add(1);
	}

	[DisplayOnly( "3 plant", "This Power has +1 range" )]
	static public Task NoOp1( TargetSpaceCtx _ ) { return Task.CompletedTask; }

	[DisplayOnly( "1 air", "This Power has +1 range" )]
	static public Task NoOp2( TargetSpaceCtx _ ) { return Task.CompletedTask; }


	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class FromPresenceAttribute : SpiritIsland.FromPresenceAttribute {
		public FromPresenceAttribute() : base(1, Target.NoBlight ) { }

		protected override async Task<int> CalcRange( SelfCtx ctx ) => range
			// 3 plant    this power has +1 range
			+ (await ctx.YouHave( "3 plant" ) ? 1 : 0)
			// 1 air      this power has +1 range
			+ (await ctx.YouHave( "1 air" ) ? 1 : 0);
	}

}
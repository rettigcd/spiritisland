namespace SpiritIsland.BranchAndClaw;

[InnatePower( "Spreading Wilds" ),Slow]
[SpreadingWilds.FromPresence]
public class SpreadingWilds {

	[InnateTier( "2 sun", "Push 1 explorer from target land per 2sun you have.", 0 )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		// push 1 explorer from target land per 2 sun you have
		await ctx.Push( await ctx.Self.Elements.CommitToCount(Element.Sun)/2,Human.Explorer);
	}

	[InnateTier( "1 plant", "If target land has no explorer, add 1 wilds.", 1 )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		// if target land has no explorer, add 1 wilds
		if( !ctx.Space.HasAny(Human.Explorer) )
			await ctx.Wilds.AddAsync(1);
	}

	[DisplayOnly( "3 plant", "This Power has +1 range" )]
	static public Task NoOp1( TargetSpaceCtx _ ) { return Task.CompletedTask; }

	[DisplayOnly( "1 air", "This Power has +1 range" )]
	static public Task NoOp2( TargetSpaceCtx _ ) { return Task.CompletedTask; }


	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public class FromPresenceAttribute : SpiritIsland.FromPresenceAttribute {
		public FromPresenceAttribute() : base(1, Filter.NoBlight ) { }

		protected override async Task<int> CalcRange( Spirit self ) => _range
			// 3 plant    this power has +1 range
			+ (await self.Elements.YouHave( "3 plant" ) ? 1 : 0)
			// 1 air      this power has +1 range
			+ (await self.Elements.YouHave( "1 air" ) ? 1 : 0);
	}

}
using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	[InnatePower( "Spreading Wilds", Speed.Slow )]
	[SpreadingWilds.FromPresence]
	public class SpreadingWilds {

		[InnateOption( "2 sun" )]
		static public Task Option1( TargetSpaceCtx ctx ) {
			// push 1 explorer from target land per 2 sun you have
			return ctx.Push(ctx.Self.Elements[Element.Sun]/2,Invader.Explorer);
		}

		[InnateOption( "1 plant" )]
		static public Task Option2( TargetSpaceCtx ctx ) {
			// if target land has no explorer, add 1 wilds
			if( !ctx.Tokens.HasAny(Invader.Explorer) )
				ctx.Tokens.Wilds().Count++;
			return Task.CompletedTask;
		}

		[InnateOption( "1 plant,2 sun" )]
		static public async Task Option1And2( TargetSpaceCtx ctx ) {
			await Option1(ctx);
			await Option2(ctx);
		}

		[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
		public class FromPresenceAttribute : TargetSpaceAttribute {
			public FromPresenceAttribute() : base( From.Presence, null, 1, Target.NoBlight ) { }
			protected override int CalcRange( SpiritGameStateCtx ctx ) => range
				// 3 plant    this power has +1 range
				+ (ctx.YouHave( "3 plant" ) ? 1 : 0)
				// 1 air      this power has +1 range
				+ (ctx.YouHave( "1 air" ) ? 1 : 0);
		}

	}

}

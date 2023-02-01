namespace SpiritIsland.BranchAndClaw;

public class FlowLikeWaterReachLikeAir {

	[MajorCard("Flow Like Water, Reach Like Air",2,Element.Air,Element.Water), Fast, AnySpirit]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {
		await TargetActions( ctx.OtherCtx, await ctx.YouHave( "2 air,2 water" ) );
	}

	static async Task TargetActions( SelfCtx ctx, bool bringCityAndBlight ) {

		// target spirit gets +2 range with all Powers.
		RangeCalcRestorer.Save(ctx.Self,ctx.GameState);
		RangeExtender.Extend( ctx.Self, 2 );



		// Target spirit may push 1 of their presence to an adjacent land
		var (src,destination) = await ctx.Presence.PushUpTo1();

		var mover = new TokenPusher_FixedDestination( ctx.Target( src ), destination );
		// bringing up to 2 explorers, 2 towns and 2 dahan along with it.
		mover.AddGroup( 2, Human.Explorer );
		mover.AddGroup( 2, Human.Town );
		mover.AddGroup( 2, Human.Dahan );

		// if you hvae 2 air, 2 water, the moved presence may also bring along up to 2 cities and up to 2 blight.
		if(bringCityAndBlight) {
			mover.AddGroup( 2, Human.City );
			mover.AddGroup( 2, Token.Blight );
		}

		await mover.MoveUpToN();
	}

}
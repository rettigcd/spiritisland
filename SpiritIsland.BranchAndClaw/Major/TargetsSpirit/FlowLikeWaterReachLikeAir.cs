namespace SpiritIsland.BranchAndClaw;

public class FlowLikeWaterReachLikeAir {

	[MajorCard("Flow Like Water, Reach Like Air",2,Element.Air,Element.Water), Fast, AnySpirit]
	[Instructions( "Target Spirit gets +2 Range with all Powers. Target Spirit may Push 1 of their Presence to an adjacent land, bringing up to 2 Explorer, 2 Town and 2 Dahan along with it. -If you have- 2 Air, 2 Water: The moved Presence may also bring along up to 2 City and up to 2 Blight." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpiritCtx ctx ) {
		await TargetActions( ctx.OtherCtx, await ctx.YouHave( "2 air,2 water" ) );
	}

	static async Task TargetActions( SelfCtx ctx, bool bringCityAndBlight ) {

		// target spirit gets +2 range with all Powers.
		RangeCalcRestorer.Save(ctx.Self,ctx.GameState);
		RangeExtender.Extend( ctx.Self, 2 );

		// Target spirit may push 1 of their presence to an adjacent land
		var (src,destination) = await ctx.Self.PushUpTo1Presence();
		if(destination is null) return;

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
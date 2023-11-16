namespace SpiritIsland.JaggedEarth;

[InnatePower("Powered by the Furnace of the Earth"), Slow, FromPresence(0)]
public class PoweredByTheFurnaceOfTheEarth {

	[InnateTier("3 earth","Add 1 of your destroyed presence.",0)]
	static public async Task Option1(TargetSpaceCtx ctx ) {
		await ctx.Presence.PlaceDestroyedHere( 1 );
	}

	[InnateTier("3 fire","-2 cost.  Gain a Power Card",1)]
	static public async Task Option2(TargetSpaceCtx ctx ) {
		if(2 <= ctx.Self.Energy 
			&& await ctx.Self.UserSelectsFirstText("Spend 2 energy to gain a Power Card", "Yes", "No thank you.")
		) {
			ctx.Self.Energy -= 2;
			await ctx.Draw();
		}
	}

	[InnateTier("4 earth,4 fire","Move up to 2 of your presence from other lands to target land.",2)]
	static public Task Option3(TargetSpaceCtx ctx ) {
		return ctx.Presence.MoveHereFromAnywhere(2);
	}

	[InnateTier("5 fire","Return up to 2 of your destroyed presence to your presence tracks.",3)]
	static public Task Option4( TargetSpaceCtx ctx ) => Cmd.ReturnUpToNDestroyedToTrack(2).ActAsync(ctx);



}
namespace SpiritIsland.JaggedEarth;

public class BirdsCryWarning {

	[MinorCard( "Birds Cry Warning", 1, Element.Sun,Element.Air,Element.Animal), Fast, FromPresence(3,Target.Dahan)]
	static public Task ActAsync( TargetSpaceCtx ctx ){
		return ctx.SelectActionOption(
			Cmd.NextTimeDestroy2FewerDahan,
			Cmd.PushUpToNDahan(3) // Push up to 3 dahan
		);
	}

}
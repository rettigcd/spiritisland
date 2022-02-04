namespace SpiritIsland.JaggedEarth;

public class BirdsCryWarning {

	[MinorCard( "Birds Cry Warning", 1, Element.Sun,Element.Air,Element.Animal), Fast, FromPresence(3,Target.Dahan)]
	static public Task ActAsync( TargetSpaceCtx ctx ){
		return ctx.SelectActionOption(
			Cmd.Destroy2FewerDahan, // !! The next time dahan would be destroyed in target land, Destroy 2 fewer dahan.
			Cmd.PushUpToNDahan(3) // Push up to 3 dahan
		);
	}

}
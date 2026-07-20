namespace SpiritIsland.BranchAndClaw;

public class TippingPoint : BlightCard {

	public TippingPoint():base("Tipping Point", 
		"Each Spirit: Destroy 3 presence.", 
		5 
	) { }

	public override IActOn<GameState> Immediately 
		=> Cmd.ForEachSpirit( 
			// destroys 3 presence
			Cmd.DestroyPresence(3)
		);

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> BlightCardRegistry.Register( nameof( TippingPoint ), ( json, ctx ) => new TippingPoint() );

}

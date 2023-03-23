namespace SpiritIsland.JaggedEarth;

public class EverMultiplyingSwarm {

	[SpiritCard("Ever-Multiplying Swarm",1,Element.Fire,Element.Earth,Element.Animal), Slow, FromPresence(0)]
	[Instructions( "Add 2 Beasts." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync(TargetSpaceCtx ctx ) {
		// Add 2 beast
		return ctx.Beasts.Add( 2 );
	}

}
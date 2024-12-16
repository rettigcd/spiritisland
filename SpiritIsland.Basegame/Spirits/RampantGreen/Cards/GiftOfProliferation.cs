namespace SpiritIsland.Basegame;

public class GiftOfProliferation {

	public const string Name = "Gift of Proliferation";

	[SpiritCard( Name, 1, Element.Moon, Element.Plant ),Fast,AnotherSpirit]
	[Instructions( "Target Spirit adds 1 Presence up to 1 Range from their Presence." ),Artist( Artists.JorgeRamos )]
	static public Task ActAsync( TargetSpiritCtx ctx ) {
		// target spirit adds 1 presense up to range 1 from their presesnse
		return Cmd.PlacePresenceWithin( 1 ).ActAsync(ctx.Other);
	}

}
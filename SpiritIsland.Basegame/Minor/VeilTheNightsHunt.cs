namespace SpiritIsland.Basegame;

public class VeilTheNightsHunt {

	[MinorCard( "Veil the Night's Hunt", 1, Element.Moon, Element.Air, Element.Animal),Fast,FromPresence( 2, Filter.Dahan )]
	[Instructions( "Each Dahan deals 1 Damage to a different Invader. -or- Push up to 3 Dahan." ), Artist( Artists.LoicBelliau )]
	static public Task Act( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceCmd( $"Each dahan deals 1 damage to a different invader", ctx => ctx.Apply1DamageToDifferentInvaders( ctx.Dahan.CountAll ) ),
			new SpaceCmd( "push up to 3 dahan", ctx => ctx.PushUpToNDahan( 3 ) )
		);

	}

}
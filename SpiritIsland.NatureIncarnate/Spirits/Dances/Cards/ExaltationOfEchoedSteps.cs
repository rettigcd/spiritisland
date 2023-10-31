namespace SpiritIsland.NatureIncarnate;

public class ExaltationOfEchoedSteps {
	const string Name = "Exaltation of Echoed Steps";

	[SpiritCard( Name, 1, Element.Moon, Element.Water, Element.Earth ), Slow, AnotherSpirit]
	[Instructions( "Target Spirit may Push 1 Presence, Bringing up to 1 Explorer/Town/Dahan/Beast. You may do likewise." ), Artist( Artists.EmilyHancock )]
	static async public Task ActAsync( TargetSpiritCtx ctx ) {
		// Target Spirit may Push 1 Presence, Bringing up to 1 Explorer/Town/Dahan/Beast.
		await PushPresenceAndBring(ctx.OtherCtx);

		// You may do likewise.
		await PushPresenceAndBring( ctx );
	}

	static async Task PushPresenceAndBring( SelfCtx ctx ) {

		// Target spirit may push 1 of their presence to an adjacent land
		await Cmd.PushUpTo1Presence( async ( from, to ) => {
			await new TokenPusher_FixedDestination( ctx.Target( from ), to )
				.AddGroup( 1, Human.Explorer, Human.Town, Human.Dahan, Token.Beast )
				.MoveUpToN();
			} )
			.Execute( ctx );

	}

}


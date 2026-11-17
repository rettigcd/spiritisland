#nullable enable
using SpiritIsland.Agent;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Tests.Solo;

/// <summary>
/// Whole games played at random with <see cref="Spirit.CombineActionsAndTargets"/> on, for the spirits whose targeting is unusual
/// (Shadows pays to reach Dahan and may ignore range, Shroud gathers presence while targeting, Keeper and Volcano change range or
/// ask about thresholds): choosing an action together with its target must never be asked for again, and no game may fail.
/// </summary>
[Collection( "BaseGame Spirits" )]
public class CombinedActions_Games_Tests {

	[Theory]
	[InlineData( "River Surges in Sunlight", true )]
	[InlineData( "Shadows Flicker Like Flame", true )]
	[InlineData( "Keeper of the Forbidden Wilds", true )]
	[InlineData( "Volcano Looming High", true )]
	[InlineData( "Lightning's Swift Strike", true )]
	[InlineData( "Relentless Gaze of the Sun", true )]
	[InlineData( "Bringer of Dreams and Nightmares", true )] // an innate power that targets a spirit
	[InlineData( "Vital Strength of the Earth", true )]
	[InlineData( "Shroud of Silent Mist", false )] // gathers presence while it targets, so its powers stay in two steps
	public void RandomGames_PlayToTheEnd_AndNeverAskForATargetThatWasAlreadyChosen( string spirit, bool offeredWithTargets ) {
		int combinedPicks = 0;

		foreach( int seed in new[] { 1, 2 } ) {
			var game = new SoloGame( GameSetup.Build( new NewGameRequest( spirit, "A", seed ) ) ) {
				EnablePreselects = false,
				EnableCombinedActions = true,
				LogExceptions = true,
			};
			var failures = new List<string>();
			game.GameState.NewLogEntry += e => { if( e is SpiritIsland.Log.ExceptionEntry ) failures.Add( e.ToString() ?? "exception" ); };
			game.Start();

			var rnd = new Random( seed );
			IDecisionPortal portal = game.UserPortal.DecisionPortal;
			string? mustNotBePrompted = null; // "<card>: Target Space" straight after choosing that card together with its target
			for( int step = 0; step < 4000 && !game.EngineTask!.IsCompleted; ++step ) {
				if( !portal.WaitForNext( 20 ) ) continue;
				IDecision decision = portal.Next;
				if( mustNotBePrompted is not null )
					decision.Prompt.ShouldNotBe( mustNotBePrompted, $"{spirit}: asked for the target again after choosing it with the power" );
				mustNotBePrompted = null;

				IOption pick = decision.Options[rnd.Next( decision.Options.Length )];
				if( pick is TargetedAction targeted ) {
					++combinedPicks;
					mustNotBePrompted = targeted.Action.Title + ": Target Space";
				}
				portal.Choose( decision, pick );
			}

			// a failure inside the engine ends the game early and is only logged
			failures.ShouldBeEmpty( $"{spirit}, seed {seed}" );
			game.EngineTask!.IsCompleted.ShouldBeTrue( $"{spirit}, seed {seed} did not finish" );
		}

		if( offeredWithTargets )
			combinedPicks.ShouldBeGreaterThan( 0, spirit + " was never offered an action together with a target" );
		else
			combinedPicks.ShouldBe( 0, spirit + " was offered an action together with a target, but cannot be handed one" );
	}

}

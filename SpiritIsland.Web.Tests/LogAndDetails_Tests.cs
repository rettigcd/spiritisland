using SpiritIsland.Web.View;

namespace SpiritIsland.Web.Tests;

/// <summary> What the log shows, and the prompt wording that is spelled out. The log's whole path is in OracleEndpoint_Tests. </summary>
public class LogAndDetails_Tests {

	[Fact]
	public void RoundsAndPhases_AreShown_AsPhaseLines() {
		LogFilter.From( new Log.Round( 2 ) ).ShouldBe( new LogLineView( "phase", "=== Round 2 ===" ) );
		LogFilter.From( new Log.Phase( Phase.Slow ) )!.Kind.ShouldBe( "phase" );
	}

	[Fact]
	public void DebugDetail_IsNotShown() {
		LogFilter.From( new Log.Debug( "each token moved" ) ).ShouldBeNull();
	}

	[Fact]
	public void WhatThePlayerChose_AndFailures_AreNotShown() {
		LogFilter.From( new Log.ExceptionEntry( new InvalidOperationException( "x" ) ) ).ShouldBeNull();
	}

	[Fact]
	public void TheGameEnding_IsShown_AsBad() {
		LogFilter.From( new GameOverLogEntry( GameOverResult.Defeat, "Time runs out" ) )!.Kind.ShouldBe( "bad" );
	}

	[Fact]
	public void ConsoleUnderlines_AreLeftOut() {
		// The engine's "island blighted" message underlines words with a second line of ^ marks.
		string text = "Blighted Island => Some Card => Do a thing\r\n  ^^^^^^^^ ^^^^^^\r\n";

		LogFilter.From( new Said( text ) )!.Text.ShouldBe( "Blighted Island => Some Card => Do a thing" );
	}

	[Fact]
	public void MessagesOfSeveralLines_KeepTheirLines() {
		LogFilter.From( new Said( "first\nsecond  \n" ) )!.Text.ShouldBe( "first\nsecond" );
	}

	sealed class Said( string text ) : Log.ILogEntry {
		public Log.LogLevel Level => Log.LogLevel.Info;
		public string Msg( Log.LogLevel _ ) => text;
	}

	[Fact]
	public void PlayPowerCard_PromptIsSpelledOut() {
		PromptDetails.For( "Play power card ($3 / 2)" ).ShouldBe( "Energy: 3. Card plays left: 2." );
	}

	[Theory]
	[InlineData( "Select Growth" )]
	[InlineData( "Play power card" )]
	[InlineData( "Play power card ($x / 2)" )]
	public void OtherPrompts_HaveNoDetails( string prompt ) => PromptDetails.For( prompt ).ShouldBeNull();
}

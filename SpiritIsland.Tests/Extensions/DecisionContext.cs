namespace SpiritIsland.Tests;

/// <summary> Binds to 1 Decision </summary>
public class DecisionContext {

	#region private

	readonly IUserPortal _gateway;
	readonly IDecision _current;

	#endregion

	#region constructor

	/// <summary> Binds to the *next* Decsion </summary>
	public DecisionContext( Spirit spirit ) {
		_gateway = spirit.Gateway;
		_current = _gateway.Next; // Waits for a decision, then binds to it
		// _current = _gateway.Current ?? throw new Exception( "no Decision presented." );
	}

	#endregion

	#region public Prompt assertions

	public DecisionContext HasPrompt( string prompt ) {
		AssertIsReady( prompt );
		_current.Prompt.ShouldBe( prompt, $"{prompt}:[any]:[first]", StringCompareShould.IgnoreCase );
		return this;
	}

	// Prompt-Prefix, .Text
	public DecisionContext HasPromptPrefix( string promptPrefix ) {
		if(!_current.Prompt.StartsWith( promptPrefix ))
			_current.Prompt.ShouldBe( promptPrefix );
		return this;
	}

	#endregion

	#region public Option assertions

	public DecisionContext HasOptions( string optionsString ) {
		string actualOptionsString = _current.FormatOptions();
		actualOptionsString
			.ShouldBe( optionsString, $"For decision '{_current.Prompt}', expected '{optionsString}' did not match actual '{actualOptionsString}'" );
		return this;
	}

	#endregion

	#region misc assertions

	public DecisionContext IsForSpace( Space space ) => IsForSpace( space.Text );

	public DecisionContext IsForSpace( string space ) {
		Select.TokenFromManySpaces tfs = _current as Select.TokenFromManySpaces;
		tfs.ShouldNotBeNull();
		((IDecision)tfs).Options.OfType<SpaceToken>().Select(x=>x.Space).Distinct().Single().Text.ShouldBe( space );
		return this;
	}

	#endregion

	public void ChooseFirst()
		=> ChooseAndWait( _current.Options.First() );

	public void ChooseFirst( string optionText, int skip = 0 )
		=> ChooseAndWait( _current.FindFirstChoice( optionText, skip ) );

	public void ChooseFirstThatStartsWith( string optionPrefix )
		=> ChooseAndWait( _current.FindFirstChoiceStartsWith( optionPrefix ) );

	public void Choose( string optionText )
		=> ChooseAndWait( _current.FindChoice( optionText ) );

	public void Choose( int index )
		=> ChooseAndWait( _current.Options[index] );

	public void Choose( IOption choice )
		=> ChooseAndWait( _current.FindChoice( choice ) );

	public string Format() => _current.FormatDecision();

	#region private helper methods

	void ChooseAndWait( IOption choice ) {
		_gateway.Choose( _current, choice );
		WaitForSignal();
	}

	void AssertIsReady( string prompt ) {
		// This line tests that are currently waiting at a decision
		// It breaks tests that get to a decision then wait for the engine to catch up.
		_gateway.IsResolved.ShouldBeFalse( $"Prompt [{prompt}] is not there." );
	}
	void WaitForSignal() {
		_gateway.WaitForNextDecision( 10 );
	}

	#endregion
}


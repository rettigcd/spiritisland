namespace SpiritIsland.Tests;

/// <summary> Binds to 1 Decision </summary>
public class DecisionContext {

	#region private

	readonly IUserPortal _portal;
	readonly IDecision _current;

	#endregion

	#region constructor

	/// <summary> Binds to the *next* Decsion </summary>
	public DecisionContext( Spirit spirit ) {
		_portal = spirit.Portal;
		_current = _portal.Next; // Waits for a decision, then binds to it
	}

	#endregion

	#region public Prompt assertions

	public DecisionContext HasPrompt( string expectedPrompt ) {
		AssertIsReady( expectedPrompt );
		return string.Compare( _current.Prompt, expectedPrompt, true )==0 ? this 
			: throw new ArgumentException($"Expected prompt '{expectedPrompt}' not found in '{Format(_current)}'");
	}
	static string Format(IDecision decision ) {
		return decision.Prompt+":"+decision.Options.Select(o=>o.Text).Join(",");
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
		A.SpaceToken tfs = _current as A.SpaceToken;
		tfs.ShouldNotBeNull();
		((IDecision)tfs).Options.OfType<SpaceToken>().Select(x=>x.Space).Distinct().Single().Text.ShouldBe( space );
		return this;
	}

	#endregion

	public void ChooseFirst()
		=> Choose( _current.Options.First() );

	public void ChooseFirst( string optionText, int skip = 0 )
		=> Choose( _current.FindFirstChoice( optionText, skip ) );

	public void ChooseFirstThatStartsWith( string optionPrefix )
		=> Choose( _current.FindFirstChoiceStartsWith( optionPrefix ) );

	public void Choose( string optionText )
		=> Choose( _current.FindChoice( optionText ) );

	public void Choose( int index )
		=> Choose( _current.Options[index] );

	public void Choose( IOption choice )
		=> _portal.Choose( _current, choice );

	public string Format() => _current.FormatDecision();

	#region private helper methods

	void AssertIsReady( string prompt ) {
		// This line tests that are currently waiting at a decision
		// It breaks tests that get to a decision then wait for the engine to catch up.
		_portal.IsResolved.ShouldBeFalse( $"Prompt [{prompt}] is not there." );
	}

	#endregion
}


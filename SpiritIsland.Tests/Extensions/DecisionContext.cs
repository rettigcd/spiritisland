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
		return string.Compare( _current.Prompt, expectedPrompt, true )==0 ? this 
			: throw new ArgumentException($"Expected prompt '{expectedPrompt}' not found in '{Format(_current)}'");
	}
	public DecisionContext HasTargetSpacePrompt( string cardName ) 
		=> HasPrompt( cardName+": Target Space" );


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

	public DecisionContext HasOptions( string expectedOptionsString ) {
		string actualOptionsString = _current.FormatOptions();
		if(actualOptionsString != expectedOptionsString )
			actualOptionsString = _current.FromatSpaceTokenOptions();

		actualOptionsString
			.ShouldBe( expectedOptionsString, $"For decision '{_current.Prompt}', expected options '{expectedOptionsString}' did not match actual options '{actualOptionsString}'" );
		return this;
	}

	/// <summary>
	/// Shortcut helper for .HasSourceOptions(...).ChooseSource(...)
	/// </summary>
	public DecisionContext MoveFrom( string optionText, string sourceOptions = null ) {
		if(sourceOptions is not null)
			HasSourceOptions(sourceOptions);

		// Handle 'Done'
		if(optionText == TextOption.Done.Text ) {
			Choose( _current.FindChoice( optionText ) );
			return this;
		}
		var source = _current.FindSourceChoice(optionText);
		// If only 1 destination, auto-select it.
		Move[] moveOptionsForSource = _current.Options.OfType<Move>().Where(m=>m.Source == source).ToArray();
		switch(moveOptionsForSource.Length) {
			case 1:
				Choose( moveOptionsForSource[0] ); break; // auto-select it
            case 0:
				throw new Exception($"found no moves that have source {source}."); // should never happen
            default:
				// multiple destinations, this is fine.
				_moveOptions = moveOptionsForSource;
				break;
		}
		return this;
	}

	public void MoveTo( string optionText, string destinationOptions = null ) {
		if(destinationOptions is not null)
			HasDestinationOptions(destinationOptions);

		var dst = _moveOptions.Single(m=>((IOption)m.Destination).Text == optionText);
		Choose( dst );
	}

	Move[] _moveOptions; // for selected source

	// For use with Move
	public DecisionContext HasSourceOptions( string optionsString ) {
		var moves = _current.Options.OfType<Move>().ToArray();
		bool fromSingle = moves.Select(m=>m.Source.Space.SpaceSpec).Distinct().Count() == 1;
		string actualOptionsString = _current.Options
			.Select( x => x is Move move ? (fromSingle ? move.Source.Token.Text : move.Source.ToString()) : x.Text )
			.Distinct()
			.Join( "," );
		actualOptionsString
			.ShouldBe( optionsString, $"For decision '{_current.Prompt}', expected '{optionsString}' did not match actual '{actualOptionsString}'" );
		return this;
	}

	public DecisionContext HasDestinationOptions( string optionsString ) {
		string actualOptionsString = _moveOptions.Select( move => ((IOption)move.Destination).Text ).OrderBy(x=>x).Join( "," );
		actualOptionsString
			.ShouldBe( optionsString, $"For decision '{_current.Prompt}', expected '{optionsString}' did not match actual '{actualOptionsString}'" );
		return this;
	}

	#endregion

	#region misc assertions

	public DecisionContext IsForSpace( SpaceSpec space ) => IsForSpace( space.Label );

	public DecisionContext IsForSpace( string space ) {
		A.SpaceTokenDecision tfs = _current as A.SpaceTokenDecision;
		tfs.ShouldNotBeNull();
		((IDecision)tfs).Options.OfType<SpaceToken>().Select(x=>x.Space.SpaceSpec).Distinct().Single().Label.ShouldBe( space );
		return this;
	}

	#endregion

	public void ChooseFirst()
		=> Choose( _current.Options.First() );

	// !!! Deprecate this
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

}


#nullable enable
namespace SpiritIsland.A;

public class TypedDecision<T> : IDecisionPlus where T:class {

	public bool AllowAutoSelect { get; }

	public string Prompt { get; }

	public IOption[] Options => _allOptions;
	protected readonly IOption[] _allOptions;

	/// <summary>
	/// Presents user we a "Done" option for Present.Done
	/// </summary>
	public TypedDecision(
		string prompt,
		IEnumerable<IOption> options,
		Present present
	){

		var optionList = options.ToList();
		if(optionList.Count != 0 && present == Present.Done )
			optionList.Add(TextOption.Done);

		Prompt = prompt;
		_allOptions = [..optionList];
		AllowAutoSelect = present == Present.AutoSelectSingle;

	}

	/// <summary>
	/// Allows caller to select "Done" text (if desired)
	/// </summary>
	/// <param name="cancelPrompt">null => does not allow cancelling.</param>
	public TypedDecision(
		string prompt,
		IEnumerable<IOption> options,
		string? cancelPrompt = null
	){

		var optionList = options.ToList();
		if(optionList.Count != 0 && cancelPrompt != null )
			optionList.Add(new TextOption(cancelPrompt));

		Prompt = prompt;
		_allOptions = [..optionList];
		AllowAutoSelect = false;

	}

	public bool TryGetResultFromOption( IOption option, out T? t ) {
		if( TextOption.Done.Matches(option)) {
			t = null;
			return true;
		}
		if( Options.Contains(option)) {
			t = ConvertOptionToResult(option);
			return true;
		}
		t = null;
		return false;
	}

	public virtual T ConvertOptionToResult( IOption option ) => (T)option;
}

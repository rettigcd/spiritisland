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
		if(optionList.Count != 0 && cancelPrompt != null ) {
			_cancelOption = new TextOption(cancelPrompt);
			optionList.Add(_cancelOption);
		}

		Prompt = prompt;
		_allOptions = [..optionList];
		AllowAutoSelect = false;

	}

	/// <summary> The custom-text cancel option (when built with a cancelPrompt). Choosing it means "no result", same as Done. </summary>
	readonly TextOption? _cancelOption;

	public bool TryGetResultFromOption( IOption option, out T? t ) {
		if( TextOption.Done.Matches(option) || _cancelOption?.Matches(option) == true ) {
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

	public T ConvertOptionToResult( IOption option ) => (T)option;
}

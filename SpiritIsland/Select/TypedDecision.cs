namespace SpiritIsland.Select;

public class TypedDecision<T> : IDecisionPlus where T:class,IOption {

	public bool AllowAutoSelect { get; }

	public string Prompt { get; }

	public IOption[] Options { get; }

	public TypedDecision(
		string prompt,
		IEnumerable<T> options,
		Present present
	){

		var optionList = options.Cast<IOption>().ToList();
		if(optionList.Count != 0 && present == Present.Done )
			optionList.Add(TextOption.Done);

		Prompt = prompt;
		Options = optionList.ToArray();
		AllowAutoSelect = present == Present.AutoSelectSingle;

	}

	public TypedDecision(
		string prompt,
		IEnumerable<T> options,
		string cancelPrompt = null
	){

		var optionList = options.Cast<IOption>().ToList();
		if(optionList.Count != 0 && cancelPrompt != null )
			optionList.Add(new TextOption(cancelPrompt));

		Prompt = prompt;
		Options = optionList.ToArray();
		AllowAutoSelect = false;

	}

	public void Select( IOption option ) => throw new NotImplementedException();
}
namespace SpiritIsland.Maui;

public class OptionViewManager {

	#region constructor

	public OptionViewManager() {}

	void OnViewClicked(IOption option, bool submit) {
		OptionSelected?.Invoke(option, submit);
	}

	public IOption? SelectedOption {
		set {
			// un-select the old
			foreach (var sv in _selectedViews)
				sv.State = OptionState.IsOption;
			_selectedViews.Clear();

			// select the current
			if( value is not null && OptionLookup.TryGetValue(value, out List<IOptionModel>? ovl)) {
				_selectedViews.AddRange(ovl);
				foreach (var sv in _selectedViews)
					sv.State = OptionState.Selected;
			}
		}
	}

	public event Action<IOption,bool>? OptionSelected;

	#endregion

	public void AddRange(IEnumerable<IOptionModel> optionModels, object? owner=null) {
		foreach(IOptionModel ov in optionModels)
			Add(ov,owner);
	}

	/// <summary> Register an OptionView so it get selected/option state changes /// </summary>
	public void Add(IOptionModel optionView, object? owner=null){
		// Add
		IOption option = optionView.Option;
		if(OptionLookup.TryGetValue(option,out List<IOptionModel>? ovl))
			ovl.Add(optionView);
		else
			OptionLookup.Add(optionView.Option, [optionView]);

		// owner
		if(owner is not null)
			if( _byOwner.TryGetValue(owner,out List<IOptionModel>? optionViews) )
				optionViews.Add(optionView);
			else
				_byOwner.Add(owner, [optionView]);

		// Enable
		if(_options.Contains(option))
			EnableView(optionView);
	}

	public void RemoveByOwner(object owner) {
		if( !_byOwner.TryGetValue(owner, out List<IOptionModel>? optionViews)) return;
		RemoveRange(optionViews);
		_byOwner.Remove(owner);
	}

	public void RemoveRange(IEnumerable<IOptionModel> optionViews) {
		foreach (IOptionModel ov in optionViews)
			Remove(ov);
	}

	public void Remove(IOptionModel ov) {
		IOption option = ov.Option;
		List<IOptionModel> ovl = OptionLookup[option];
		ovl.Remove(ov);
		if(ovl.Count == 0)
			OptionLookup.Remove(option);
	}

	/// <summary> Updates OptionViews .State property based on current decision. </summary>
	public void SyncOptionStatesToDecision( IDecision decision ) {

		// Clear old
		foreach (IOptionModel optionView in _enabledViews) {
			optionView.State = OptionState.Default;
			optionView.SelectOptionCallback = null;
		}
		_enabledViews.Clear();

		// Add new
		_options = decision.Options;
		foreach( var option in _options )
			if( OptionLookup.TryGetValue(option, out List<IOptionModel>? ovl) )
				foreach(var ov in ovl)
					EnableView(ov);
	}

	void EnableView(IOptionModel ov) {
		ov.State = OptionState.IsOption;
		_enabledViews.Add(ov);
		ov.SelectOptionCallback = OnViewClicked;
	}

	#region privatefields

	IOption[] _options = []; // the options from the current decision - so when adding controls, can enable them for the current decision
	readonly List<IOptionModel> _selectedViews = []; // currently selected control
	readonly List<IOptionModel> _enabledViews = []; // holds enabled controls so we can easily disable them when decision is made

	readonly Dictionary<IOption, List<IOptionModel>> OptionLookup = []; // find controls
	readonly Dictionary<object,List<IOptionModel>> _byOwner = [];
	#endregion privatefields

}
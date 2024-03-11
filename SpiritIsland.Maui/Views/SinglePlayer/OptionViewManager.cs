namespace SpiritIsland.Maui;

public class OptionViewManager {

	public DecisionModel DecisionModel => _decisionModel;

	#region constructor

	public OptionViewManager( DecisionModel decisionModel ) {
		_decisionModel = decisionModel;
		_decisionModel.PropertyChanged += Value_PropertyChanged;
	}

	#endregion

	public void AddRange(IEnumerable<OptionView> optionViews, object? owner=null) {
		foreach(OptionView ov in optionViews)
			Add(ov,owner);
	}

	/// <summary> Register an OptionView so it get selected/option state changes /// </summary>
	public void Add(OptionView optionView, object? owner=null){
		// Add
		IOption option = optionView.Option;
		if(OptionLookup.TryGetValue(option,out List<OptionView>? ovl))
			ovl.Add(optionView);
		else
			OptionLookup.Add(optionView.Option, [optionView]);

		// owner
		if(owner is not null)
			if( _byOwner.TryGetValue(owner,out List<OptionView>? optionViews) )
				optionViews.Add(optionView);
			else
				_byOwner.Add(owner, [optionView]);

		// Enable
		if(_options.Contains(option))
			EnableView(optionView);
	}

	public void RemoveByOwner(object owner) {
		if( !_byOwner.TryGetValue(owner, out List<OptionView>? optionViews)) return;
		RemoveRange(optionViews);
		_byOwner.Remove(owner);
	}

	public void RemoveRange(IEnumerable<OptionView> optionViews) {
		foreach (OptionView ov in optionViews)
			Remove(ov);
	}

	public void Remove(OptionView ov) {
		IOption option = ov.Option;
		List<OptionView> ovl = OptionLookup[option];
		ovl.Remove(ov);
		if(ovl.Count == 0)
			OptionLookup.Remove(option);
	}

	/// <summary> Updates OptionViews .State property based on current decision. </summary>
	public void SyncOptionStatesToDecision( IDecision decision ) {
		// Clear old
		foreach (OptionView optionView in _enabledViews) {
			optionView.State = OptionState.Default;
			optionView.SelectOptionCallback = null;
		}
		_enabledViews.Clear();

		// Add new
		_options = decision.Options;
		foreach (var option in _options)
			if (OptionLookup.TryGetValue(option, out List<OptionView>? ovl))
				foreach(var ov in ovl)
					EnableView(ov);
	}

	void EnableView(OptionView ov) {
		ov.State = OptionState.IsOption;
		_enabledViews.Add(ov);
		ov.SelectOptionCallback = OnViewClicked;
	}

	#region private methods

	void OnViewClicked(IOption option,bool submit) { 
		_decisionModel.SelectedOption = option;
		if(submit)
			_decisionModel.Submit();
	}

	void Value_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
		ArgumentNullException.ThrowIfNull(sender);
		DecisionModel decision = (DecisionModel)sender;
		if (e.PropertyName == nameof(DecisionModel.SelectedOption) ) {
			// un-select the old
			foreach(var sv in _selectedViews)
				sv.State = OptionState.IsOption;
			_selectedViews.Clear();

			// select the current
			if (decision.SelectedOption is not null && OptionLookup.TryGetValue(decision.SelectedOption, out List<OptionView>? ovl)) {
				_selectedViews.AddRange(ovl);
				foreach(var sv in _selectedViews)
					sv.State = OptionState.Selected;
			}
		}
	}

	#endregion private methods

	#region privatefields

	IOption[] _options = []; // the options from the current decision - so when adding controls, can enable them for the current decision
	readonly List<OptionView> _selectedViews = []; // currently selected control
	readonly List<OptionView> _enabledViews = []; // holds enabled controls so we can easily disable them when decision is made

	readonly Dictionary<IOption, List<OptionView>> OptionLookup = []; // find controls
	readonly DecisionModel _decisionModel; // sets selected option when user clicks on control
	readonly Dictionary<object,List<OptionView>> _byOwner = [];
	#endregion privatefields

}
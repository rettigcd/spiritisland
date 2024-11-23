namespace SpiritIsland.Maui;

class AdversaryFlagModel : ObservableModel {

	public string Name { get; }

	public bool IsActive {
		get => _isActive;
		set { 
			SetProp(ref _isActive, value);
			ShadowColor = value ? SelectedColor : NonSelectedColor;
		}
	}

	// Helper for displaying what is active
	public Color ShadowColor { get => _shadowColor; private set => SetProp(ref _shadowColor, value); }

	/// <summary> View selects this Adversary </summary>
	public Command Select { get; }
	/// <summary> Tell the Parent model who was selected so it can set the correct flag as active. </summary>
	public event Action<AdversaryFlagModel>? RequestSelected;

	public AdversaryFlagModel(string name) {
		Name = name;
		Select = new Command(RequestSelect);
	}

	#region private
	void RequestSelect() { RequestSelected?.Invoke(this); }

	// backing fields
	bool _isActive;
	Color _shadowColor = NonSelectedColor;

	// Consts
	static readonly Color SelectedColor = Colors.Blue;
	static readonly Color NonSelectedColor = Colors.LightGray;
	#endregion private backing fields
}
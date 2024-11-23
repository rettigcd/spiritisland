namespace SpiritIsland.Maui;

public class AdversaryLevelModel : ObservableModel {

	public int Level => _src.Level;

	public string LevelText => _src.Level == 0 ? "Escalation" : $"Level {_src.Level}";

	public string Title => _src.Title;

	public string Description => _src.Description;

	public int Difficulty => _src.Difficulty;

	public Command Select { get; }
	public event Action<AdversaryLevelModel>? SelectRequested;

	public bool IsActive {
		get => _isActive;
		set {
			SetProp(ref _isActive, value);
			ShadowColor = value ? SelectedColor : NonSelectedColor;
		}
	}

	// Helper for displaying what is active
	public Color ShadowColor { get => _shadowColor; private set => SetProp(ref _shadowColor, value); }

	#region constructor

	public AdversaryLevelModel(AdversaryLevel src) {
		_src = src;
		Select = new Command(()=>SelectRequested?.Invoke(this));
	}

	#endregion constructor

	#region private

	bool _isActive; 
	Color _shadowColor = NonSelectedColor;

	readonly AdversaryLevel _src;

	static readonly Color SelectedColor = Colors.LightSteelBlue;
	static readonly Color NonSelectedColor = Colors.LightGray;

	#endregion private

}

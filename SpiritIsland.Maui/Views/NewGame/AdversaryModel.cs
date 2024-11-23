namespace SpiritIsland.Maui;

public class AdversaryModel : ObservableModel {

	public string Name { get; }
	public string? LossCondition { get; }
	public bool HasLossCondition => !string.IsNullOrEmpty(LossCondition);
	public AdversaryLevelModel[] Levels { get; }

	/// <summary> Top ACTIVE Level </summary>
	public AdversaryLevelModel TopLevel => Levels[_level];

	public int Level { 
		get => _level;
		set {
			SetProp(ref _level, value);
			foreach( var level in Levels )
				level.IsActive = level.Level <= value;
		}
	}
	int _level;

	#region constructor

	public AdversaryModel(IAdversaryBuilder adversary, int level=0) {
		Name = adversary.Name;
		LossCondition = adversary.LossCondition?.Description;
		Levels = adversary.Levels
			.Select(BuildLevelModel)
			.ToArray();
		Level = level; 
	}

	AdversaryLevelModel BuildLevelModel(AdversaryLevel level) {
		var model = new AdversaryLevelModel(level);
		model.SelectRequested += Level_SelectRequested;
		return model;
	}

	void Level_SelectRequested(AdversaryLevelModel selectedLevel) {
		Level=selectedLevel.Level;
	}

	#endregion constructor

	public AdversaryConfig ToConfig() => new AdversaryConfig(Name, Level);
}

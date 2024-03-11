using System.ComponentModel;
namespace SpiritIsland.Maui;

public class GrowthTrackModel : ObservableModel {

	#region observalbe properties

	public GrowthGroupModel[] Groups { get => _groups; set => SetProp(ref _groups,value); }
	GrowthGroupModel[] _groups = [];

	public string Description { get => _descritpion; set => SetProp(ref _descritpion, value); }
	string _descritpion = "";

	#endregion observalbe properties

	#region constructor

	public GrowthTrackModel( GrowthTrack track ) {
		_groups = track.Groups
			.Select(a => new GrowthGroupModel(a))
			.ToArray();

		// Watch each Growth and see if it test selected
		foreach(var growth in _groups.SelectMany(grp=>grp.Actions))
			growth.PropertyChanged += Growth_PropertyChanged;
	}

	#endregion

	#region private methods

	void Growth_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (e.PropertyName == nameof(GrowthActionModel.State)
			&& sender is GrowthActionModel gam
			&& gam.State == OptionState.Selected
		)
			Description = gam.Description;
	}

	#endregion

}
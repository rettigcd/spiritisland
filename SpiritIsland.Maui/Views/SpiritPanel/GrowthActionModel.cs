namespace SpiritIsland.Maui;

public class GrowthActionModel : ObservableModel, IOptionModel {

	#region observalbe properties
	public ImageSource ImageSource { 
		get => _imageSource; 
		set => SetProp(ref _imageSource,value); 
	}  ImageSource _imageSource;
	public string Description      { get => _description; set => SetProp(ref _description, value); } string _description;
	#endregion observalbe properties

	#region interface OptionView Imp

	public OptionState State {
		get => _state;
		set => SetProp(ref _state,value);
	}
	OptionState _state = OptionState.Default;

	// Maybe this should be part of the OptionView?
	public void Select(bool submit) => SelectOptionCallback?.Invoke(Option,submit);

	public IOption Option { get; }

	public Action<IOption,bool>? SelectOptionCallback { get; set; }

	#endregion interface OptionView Imp

	#region constructor

	public GrowthActionModel(GrowthAction action) {
		Option = action;
		_description = action.Cmd.Description;
		_imageSource = ImageCache.FromFile(_description.ToResourceName(".png") );
	}

	#endregion constructor

}

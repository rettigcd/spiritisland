namespace SpiritIsland.Maui;

public interface OptionView {
	OptionState State { get; set; }
	/// <summary> The user option this view represents </summary>
	IOption Option { get; }
	/// <summary> Enables / Disables the Button to enact clicks </summary>
	Action<IOption,bool>? SelectOptionCallback { set; }
}

public enum OptionState { Default, IsOption, Selected }
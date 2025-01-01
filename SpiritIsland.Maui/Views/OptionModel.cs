//using Android.OS;
namespace SpiritIsland.Maui;

/// <summary> Wrapper to make Text visibile for explicitly defined .Text interface property </summary>
public class OptionModel(IOption option) {
	public IOption Option { get; } = option; 
	public string Text => Option.Text;
}

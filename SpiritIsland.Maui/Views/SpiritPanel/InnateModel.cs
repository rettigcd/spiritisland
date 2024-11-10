
namespace SpiritIsland.Maui;

public class InnateModel(InnatePower power, Spirit spirit) : ObservableModel {

	public string Title { get; } = power.Title;
	public string Instructions { get; } = power.GeneralInstructions;
	public bool HasInstructions { get; } = !string.IsNullOrWhiteSpace(power.GeneralInstructions);
	public ImageSource Phase       => ImageCache.FromFile( power.DisplaySpeed.ToString().ToLower()+".png" );
	public ImageSource SourceRange => ImageCache.FromFile("attr_" + power.RangeText.ToResourceName(".png"));
	public ImageSource Target      => ImageCache.FromFile("attr_" + power.TargetFilter.ToResourceName(".png"));

	public InnateTierModel[] Tiers { get; } = power.DrawableOptions.Select(t => new InnateTierModel(t,spirit)).ToArray();

	internal void Update() {
		foreach(var tier in Tiers)
			tier.Update();
	}
}
namespace SpiritIsland;

/// <summary>
/// Repeats an Innate Power if the elemental thresholds are met.
/// </summary>
public class RepeatIfAttribute(string elementThreshold, params string[] additionalThresholds) : RepeatAttribute {

	public override IDrawableInnateTier[] ThresholdTiers { get; } = [
			new DrawableRepeatOption(elementThreshold, "Repeat this Power."),
			.. additionalThresholds.Select( t => new DrawableRepeatOption(t,"Repeat this Power again.") )
		];

	public override IPowerRepeater GetRepeater() => new Repeater( ThresholdTiers );

	class Repeater( IDrawableInnateTier[] thresholds ) : IPowerRepeater {

		readonly List<IDrawableInnateTier> _thresholds = [.. thresholds];

		public async Task<bool> ShouldRepeat( Spirit spirit ) {
			foreach(var threshold in _thresholds) {
				if( await spirit.Elements.MeetThreshold(threshold.Elements, "Repeating") ) {
					_thresholds.Remove(threshold);
					return true;
				}
			}
			return false;
		}
	}

}

public class DrawableRepeatOption( string thresholds, string description ) : IDrawableInnateTier {
	public CountDictionary<Element> Elements { get; } = ElementStrings.Parse( thresholds );

	public string Description { get; } = description;

	string IDrawableInnateTier.Text => ThresholdString;

	public string ThresholdString => Elements.BuildElementString();
	public bool IsActive( ElementMgr activatedElements ) => activatedElements.ContainsRaw( Elements );
}

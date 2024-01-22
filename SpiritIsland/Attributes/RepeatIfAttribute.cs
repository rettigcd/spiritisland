namespace SpiritIsland;

/// <summary>
/// Repeats an Innate Power if the elemental thresholds are met.
/// </summary>
public class RepeatIfAttribute : RepeatAttribute {

	public override IDrawableInnateTier[] Thresholds { get; }

	public RepeatIfAttribute(string elementThreshold, params string[] additionalThresholds) {
		var repeats = new List<IDrawableInnateTier> {
			new DrawableRepeatOption( elementThreshold, "Repeat this Power." )
		};
		if(additionalThresholds != null && additionalThresholds.Length>0)
			repeats.AddRange( additionalThresholds.Select( t => new DrawableRepeatOption(t,"Repeat this Power again.") ) );
		Thresholds = [.. repeats];
	}

	public override IPowerRepeater GetRepeater() => new Repeater( Thresholds );

	class Repeater : IPowerRepeater {

		readonly List<IDrawableInnateTier> _thresholds;

		public Repeater( IDrawableInnateTier[] thresholds ) {
			_thresholds = [.. thresholds];
		}

		public async Task<bool> ShouldRepeat( Spirit spirit ) {
			foreach(var threshold in _thresholds) {
				if( await spirit.HasElement( threshold.Elements, "Repeating" ) ) {
					_thresholds.Remove(threshold);
					return true;
				}
			}
			return false;
		}
	}

}

public class DrawableRepeatOption : IDrawableInnateTier {
	public DrawableRepeatOption( string thresholds, string description ) {
		Elements = ElementStrings.Parse(thresholds);
		Description = description;
	}
	public CountDictionary<Element> Elements { get; }

	public string Description { get; }

	string IOption.Text => ThresholdString;

	public string ThresholdString => Elements.BuildElementString();
	public bool IsActive( ElementMgr activatedElements ) => activatedElements.Contains( Elements );
}

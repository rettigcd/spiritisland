namespace SpiritIsland;

/// <summary>
/// Repeats an Innate Power if the elemental thresholds are met.
/// </summary>
public class RepeatIfAttribute : RepeatAttribute {

	public override IDrawableInnateOption[] Thresholds { get; }

	public RepeatIfAttribute(string elementThreshold, params string[] additionalThresholds) {
		var repeats = new List<IDrawableInnateOption> {
			new DrawableInnateOption( elementThreshold, "Repeat this Power." )
		};
		if(additionalThresholds != null && additionalThresholds.Length>0)
			repeats.AddRange( additionalThresholds.Select( t => new DrawableInnateOption(t,"Repeat this Power again.") ) );
		this.Thresholds = repeats.ToArray();
	}

	public override IPowerRepeater GetRepeater() => new Repeater( Thresholds );

	class Repeater : IPowerRepeater {

		readonly List<IDrawableInnateOption> thresholds;

		public Repeater( IDrawableInnateOption[] thresholds ) {
			this.thresholds = thresholds.ToList();
		}

		public async Task<bool> ShouldRepeat( Spirit spirit ) {
			foreach(var threshold in thresholds) {
				if( await spirit.HasElements(threshold.Elements) ) {
					thresholds.Remove(threshold);
					return true;
				}
			}
			return false;
		}
	}

}

public class DrawableInnateOption : IDrawableInnateOption {
	public DrawableInnateOption( string thresholds, string description ) {
		Elements = ElementCounts.Parse(thresholds);
		Description = description;
	}
	public ElementCounts Elements { get; }

	public string Description { get; }

	string IOption.Text => ThresholdString;

	public string ThresholdString => Elements.BuildElementString();
	public bool IsActive( ElementCounts activatedElements ) => activatedElements.Contains( Elements );
}

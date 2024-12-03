namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Prevents Disease from being removed when on Spirit's lands.
/// </summary>
public class TerrorOfASlowlyUnfoldingPlague( Spirit _spirit ) : BaseModEntity, IModifyRemovingToken {

	static public SpecialRule Rule => new SpecialRule(
		"The Terror of a Slowly Unfolding Plague",
		"When disease would prevent a Build on a board with your presence, you may let the Build happen (removing no disease).  If you do, 1 fear."
	);

	async Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {

		bool getFear = IsDiseaseStoppingBuild( args )
			&& _spirit.Presence.IsOn( args.From )
			&& await _spirit.UserSelectsFirstText(
				$"Allow pending Build on {args.From.SpaceSpec.Label}?",
				"Yes, Keep Disease, Gain +1 Fear", "No! Are you kidding me? Stop build." );

		if(getFear) {
			args.Count = 0; // not removing
			args.From.AddFear( 1 );
		}
	}

	static bool IsDiseaseStoppingBuild( RemovingTokenArgs args ) 
		=> args.Token.HasTag( Token.Disease )
		&& args.Reason == RemoveReason.UsedUp
		&& args.Count == 1;
}
namespace SpiritIsland.JaggedEarth;

public class TerrorOfASlowlyUnfoldingPlague : DiseaseToken {

	static public SpecialRule Rule => new SpecialRule(
		"The Terror of a Slowly Unfolding Plague",
		"When disease would prevent a Build on a board with your presence, you may let the Build happen (removing no disease).  If you do, 1 fear."
	);


	readonly Spirit _spirit;

	public TerrorOfASlowlyUnfoldingPlague( Spirit spirit ) : base() {
		_spirit = spirit;
	}

	public override async Task<bool> Skip( SpaceState tokens, TokenClass buildClass ) {

		bool allowBuild = _spirit.Presence.IsOn( tokens )
			&& await _spirit.UserSelectsFirstText( $"Allow pending {buildClass.Label} build on {tokens.Space.Label}?", "Yes, Keep Disease +1 Fear", "No, stop build" );

		if(allowBuild) {
			GameState.Current.Fear.AddDirect( new FearArgs( 1 ) { space = tokens.Space } );
			return false;
		}

		return await base.Skip( tokens, buildClass );
	}

}
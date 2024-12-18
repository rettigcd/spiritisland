namespace SpiritIsland.NatureIncarnate;

class SenslessRoamingTokens( Spirit spirit, Space ss ) : Space(ss) {
	readonly Spirit _spirit = spirit;

	public override async Task<SpaceToken> Add1StrifeToAsync( HumanToken invader ) {
		SpaceToken strifed = await base.Add1StrifeToAsync( invader );
		if( strifed.Token.HasAny(Human.Explorer_Town)) {
			TokenMovedArgs? moved = await strifed.PushAsync( _spirit, d=>d.ConfigAsOptional() );
			if(moved is not null)
				strifed = moved.Added.On( (Space)moved.To );
		}
		return strifed;
	}
}

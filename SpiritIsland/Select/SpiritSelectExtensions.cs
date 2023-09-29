using SpiritIsland.Select;

namespace SpiritIsland; 

static public class SpiritSelectExtensions {

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	/// <returns>Track or SpaceToken</returns>
	static public async Task<IOption> SelectMovablePresence2( this Spirit self, string actionPhrase = "move" ) {
		string prompt = $"Select Presence to {actionPhrase}";
		return (IOption)await self.Gateway.Decision( Select.TrackSlot.ToReveal( prompt, self ) )
			?? await self.Gateway.Decision( new ASpaceToken(prompt, self.Presence.Movable, Present.Always ) );
	}

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	/// <returns>Track or SpaceToken</returns>
	static public async Task<IOption> SelectSourcePresence( this Spirit self, string actionPhrase = "place" ) {
		string prompt = $"Select Presence to {actionPhrase}";
		return (IOption)await self.Gateway.Decision( Select.TrackSlot.ToReveal( prompt, self ) )
			?? (IOption)await self.Gateway.Decision( new ASpaceToken( prompt, self.Presence.Deployed, Present.Always ) );
	}

	static public async Task<Space> SelectDeployed( this Spirit self, string prompt )
		=> (await self.Gateway.Decision( new Select.ASpaceToken( prompt, self.Presence.Deployed, Present.Always ) )).Space;

	static public Task<SpaceToken> SelectDeployedMovable( this Spirit self, string prompt )
		=> self.Gateway.Decision( new ASpaceToken( prompt, self.Presence.Movable, Present.Always ) );

	static public async Task<SpaceToken> PickPresenceToDestroy( this Spirit spirit, string prompt= "Select Presence to Destroy" ) {
		var spaceToken = await spirit.Gateway.Decision( new ASpaceToken( prompt, spirit.Presence.Deployed, Present.Always ) );
		await spaceToken.Destroy();
		return spaceToken;
	}

	static public async Task<(Space, Space)> PushUpTo1Presence(this Spirit self) {
		// Select source
		var source = await self.Gateway.Decision( new ASpaceToken( "Select Presence to push.", self.Presence.Movable, Present.Done ) );
		if(source == null) return (null, null);

		// Select destination
		Space destination = await self.Gateway.Decision( Select.ASpace.PushPresence( source.Space, source.Space.Tokens.Adjacent, Present.Always, source.Token ) );
		await source.MoveTo( destination.Tokens );
		return (source.Space, destination);
	}

	static public Task<TokenMovedArgs> Move( this IToken token, SpaceState from, SpaceState to ) {
		return from.MoveTo( token, to.Space );
	}

	static public async Task ReturnUpToNDestroyedToTrack( this Spirit self, int count ) {
		count = Math.Max( count, self.Presence.Destroyed );
		while(count > 0) {
			var dst = await self.Gateway.Decision( Select.TrackSlot.ToCover( self ) );
			if(dst == null) break;
			await self.Presence.ReturnDestroyedToTrack( dst );
			--count;
		}
	}

	/// <summary> Selects: Source then Destination(predetermined) for placing presence.</summary>
	/// <returns>Place in Ocean, Growth through sacrifice</returns>
	static public async Task PlacePresenceOn1( this Spirit self, params SpaceState[] destinationOptions ) {
		IOption from = await self.SelectSourcePresence();
		IToken token = from is SpaceToken sp ? sp.Token : self.Presence.Token; // We could expose this as the Default Token
		Space to = await self.Gateway.Decision( Select.ASpace.ToPlacePresence( destinationOptions, Present.Always, token ) );
		await self.Presence.Place( from, to );
	}

	static public async Task<(IOption, Space)> PlacePresenceWithin( this Spirit self, TargetCriteria targetCriteria, bool forPower ) {
		IOption from = await self.SelectSourcePresence();
		IToken token = from is SpaceToken sp ? sp.Token : self.Presence.Token; // We could expose this as the Default Token
		var toOptions = self.FindSpacesWithinRange( targetCriteria, forPower )
			.Where( self.Presence.CanBePlacedOn );
		Space to = await self.Gateway.Decision( Select.ASpace.ToPlacePresence( toOptions, Present.Always, token ) );
		await self.Presence.Place( from, to );
		return (from, to);
	}

}

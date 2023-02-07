namespace SpiritIsland; 

static public class SpiritSelectExtensions {

	static public Task<SpiritIsland.Space> SelectSacredSite( this Spirit self, string prompt )
		=> self.Gateway.Decision( Select.DeployedPresence.SacredSites( prompt, self.Presence, Present.Always ) );

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	static public async Task<IOption> SelectMovablePresence( this SpiritIsland.Spirit self, string actionPhrase = "move" ) {
		string prompt = $"Select Presence to {actionPhrase}";
		return (IOption)await self.Gateway.Decision( Select.TrackSlot.ToReveal( prompt, self ) )
			?? await self.Gateway.Decision( Select.DeployedPresence.Movable( prompt, self, Present.Always ) );
	}

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	static public async Task<IOption> SelectSourcePresence( this Spirit self, string actionPhrase = "place" ) {
		string prompt = $"Select Presence to {actionPhrase}";
		return (IOption)await self.Gateway.Decision( Select.TrackSlot.ToReveal( prompt, self ) )
			?? (IOption)await self.Gateway.Decision( Select.DeployedPresence.All( prompt, self.Presence, Present.Always ) );
	}

	/// <summary>Selects a Space within a range of spirits Presence</summary>
	/// <param name="targetingPowerType">
	/// None => standard ranging 
	/// Innate/PowerCard => power ranging  + Passes to PowerRanging
	/// </param>
	static public async Task<Space> SelectDestinationWithinRange( this Spirit self, TargetCriteria targetCriteria, bool forPower ) {
		var options = self.FindSpacesWithinRange( targetCriteria, forPower )
			.Where( self.Presence.CanBePlacedOn )
			.ToArray();
		return await self.Gateway.Decision( Select.Space.ToPlacePresence( options, Present.Always, self.Token ) );
	}

	static public Task<Space> SelectDeployed( this Spirit self, string prompt )
		=> self.Gateway.Decision( Select.DeployedPresence.All( prompt, self.Presence, Present.Always ) );

	static public Task<Space> SelectDeployedMovable( this Spirit self, string prompt )
		=> self.Gateway.Decision( Select.DeployedPresence.Movable( prompt, self, Present.Always ) );


	static public async Task PickPresenceToDestroy( this Spirit spirit, string prompt ) {
		var space = await spirit.Gateway.Decision( Select.DeployedPresence.ToDestroy( prompt, spirit.Presence ) );
		await space.Tokens.Destroy(spirit.Token,1);
	}

	static public async Task<(Space, Space)> PushUpTo1Presence(this Spirit self) {
		// Select source
		var source = await self.Gateway.Decision( Select.DeployedPresence.ToPush( self.Presence ) );
		if(source == null) return (null, null);

		// Select destination
		Space destination = await self.Gateway.Decision( Select.Space.PushPresence( source, source.Tokens.Adjacent, Present.Always, self.Token ) );
		await source.Tokens.MoveTo( self.Token, destination );
		return (source, destination);
	}

	static public Task Move( this IVisibleToken token, SpaceState from, SpaceState to ) {
		return from.MoveTo( token, to.Space );
	}

	static public Task AddTo( this IVisibleToken token, SpaceState spaceState ) => spaceState.Add( token, 1 );

	static public Task RemoveFrom( this IVisibleToken token, SpaceState spaceState ) => spaceState.Remove( token, 1 );

	static public async Task<(IOption, Space)> PlacePresenceWithin( this Spirit self, TargetCriteria targetCriteria, bool forPower ) {
		IOption from = await self.SelectSourcePresence();
		Space to = await self.SelectDestinationWithinRange( targetCriteria, forPower );
		await self.Presence.Place( from, to );
		return (from, to);
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
		Space to = await self.Gateway.Decision( Select.Space.ToPlacePresence( destinationOptions, Present.Always, self.Token ) );
		await self.Presence.Place( from, to );
	}

	static public async Task DestroyOnePresenceFromAnywhere( this Spirit self, Func<SpaceState, bool> filter = null ) {
		var space = filter == null
			? await self.Gateway.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", self.Presence ) )
			: await self.Gateway.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", self.Presence, filter ) );
		await space.Tokens.Destroy( self.Token, 1 );
	}


}

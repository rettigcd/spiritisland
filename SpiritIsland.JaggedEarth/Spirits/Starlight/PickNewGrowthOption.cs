namespace SpiritIsland.JaggedEarth;
class PickNewGrowthOption : GrowthActionFactory, ITrackActionFactory {

	readonly GrowthOption[] options;
	readonly bool add1Energy;
	bool used = false;
	Track trackToReceiveElementAssignment;

	public PickNewGrowthOption(bool add1Energy, params GrowthOption[] options ) {
		this.options = options;
		this.add1Energy = add1Energy;
	}

	public Track AssignElementFor(Track track ) {
		this.trackToReceiveElementAssignment = track;
		track.Action = this;
		return track;
	}

	public bool RunAfterGrowthResult => 
		false;

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(used) 
			return;

		if(add1Energy)
			ctx.Self.Energy++;

		if(trackToReceiveElementAssignment != null)
			await AssignElement.AssignNewElementToTrack(ctx, trackToReceiveElementAssignment );

		GrowthOption option = (GrowthOption)await ctx.Self.Select( "Select New Growth Option", options, Present.Always );
		ctx.Self.Growth.Groups.Single().Add( option );

		used = true;
	}

}
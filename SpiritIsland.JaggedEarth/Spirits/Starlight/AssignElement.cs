namespace SpiritIsland.JaggedEarth;

class AssignElement : GrowthActionFactory, IActionFactory {
	readonly Track track;
	public AssignElement( Track track ) { this.track = track; }

	public override async Task ActivateAsync( SelfCtx ctx ) {
		await AssignNewElementToTrack( ctx, track );
		track.Action = null;
	}

	public static async Task AssignNewElementToTrack( SelfCtx ctx, Track track ) {
		var el = await ctx.Self.SelectElementEx( "Select permanent element for this slot.", ElementList.AllElements );
		track.Elements = new Element[] { el };
		ctx.Self.Elements[el]++;
		track.Icon.ContentImg = el.GetTokenImg();
	}
}
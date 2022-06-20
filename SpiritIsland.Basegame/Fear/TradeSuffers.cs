namespace SpiritIsland.Basegame;

public class TradeSuffers : IFearOptions {

	public const string Name = "Trade Suffers";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Invaders do not Build in lands with City." )]
	public Task Level1( FearCtx ctx ) {
		ctx.GameState.PreBuilding.ForRound.Add( ( args ) => {
			foreach(var space in args.SpacesWithBuildTokens) {
				if(0 < args.GameState.Tokens[space].Sum(Invader.City))
					args.GameState.AdjustTempToken( space, BuildStopper.StopAll( Name ) );
			}
		} );
		return Task.CompletedTask;
	}

	[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
	public async Task Level2( FearCtx ctx ) {
		var gs = ctx.GameState;
		var actionId = Guid.NewGuid();
		foreach(var spirit in gs.Spirits) {
			var options = gs.Island.AllSpaces.Where( s => s.IsCoastal && gs.Tokens[s].Has( Invader.Town ) ).ToArray();
			if(options.Length == 0) return;
			var target = await spirit.Action.Decision( new Select.Space( "Replace town with explorer", options, Present.Always ) );
			await ReplaceInvader.Downgrade( spirit.Bind( gs, actionId ).Target( target), Present.Done, Invader.Town );
		}
	}

	[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
	public async Task Level3( FearCtx ctx ) {
		var gs = ctx.GameState;
		var actionId = Guid.NewGuid();
		foreach(var spirit in gs.Spirits) {
			var options = gs.Island.AllSpaces.Where( s => s.IsCoastal && gs.Tokens[ s ].HasAny(Invader.Town,Invader.City) ).ToArray();
			if(options.Length == 0) return;
			var target = await spirit.Action.Decision( new Select.Space( "Replace town with explorer or city with town", options, Present.Always ));
			await ReplaceInvader.Downgrade( spirit.Bind( gs, actionId ).Target( target ), Present.Done, Invader.Town, Invader.City );
		}
	}

}
namespace SpiritIsland.Basegame;

public class TradeSuffers : IFearOptions {

	public const string Name = "Trade Suffers";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Invaders do not Build in lands with City." )]
	public Task Level1( GameCtx ctx ) {
		ctx.GameState.PreBuilding.ForRound.Add( ( args ) => {
			foreach(var space in args.SpacesWithBuildTokens) {
				if(0 < space.Sum(Invader.City))
					args.GameState.AdjustTempToken( space.Space, BuildStopper.StopAll( Name ) );
			}
		} );
		return Task.CompletedTask;
	}

	[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
	public async Task Level2( GameCtx ctx ) {
		var gs = ctx.GameState;
		var tm = gs.Island.Terrain_ForFear;
		foreach(var spirit in gs.Spirits) {
			var options = gs.AllActiveSpaces.Where( s => tm.IsInPlay(s) && tm.IsCoastal(s.Space) && s.Has( Invader.Town ) ).ToArray();
			if(options.Length == 0) return;
			var target = await spirit.Gateway.Decision( new Select.Space( "Replace town with explorer", options, Present.Always ) );
			await ReplaceInvader.Downgrade( spirit.Bind( gs, ctx.UnitOfWork ).Target( target), Present.Done, Invader.Town );
		}
	}

	[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
	public async Task Level3( GameCtx ctx ) {
		var gs = ctx.GameState;
		var tm = gs.Island.Terrain_ForFear;
		foreach(var spirit in gs.Spirits) {
			var options = gs.AllActiveSpaces.Where( s => tm.IsInPlay( s ) && tm.IsCoastal( s.Space ) && s.HasAny(Invader.Town,Invader.City) ).Select(s=>s.Space).ToArray();
			if(options.Length == 0) return;
			var target = await spirit.Gateway.Decision( new Select.Space( "Replace town with explorer or city with town", options, Present.Always ));
			await ReplaceInvader.Downgrade( spirit.Bind( gs, ctx.UnitOfWork ).Target( target ), Present.Done, Invader.Town, Invader.City );
		}
	}

}
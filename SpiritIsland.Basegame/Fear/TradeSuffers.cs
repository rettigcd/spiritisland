namespace SpiritIsland.Basegame;

public class TradeSuffers : FearCardBase, IFearCard {

	public const string Name = "Trade Suffers";
	public string Text => Name;

	[FearLevel( 1, "Invaders do not Build in lands with City." )]
	public Task Level1( GameCtx ctx ) {
		ctx.GameState.AddToAllActiveSpaces( new SkipBuild_Custom( Name, true, (_,space,_1)=> space.HasAny( Invader.City ) ) );
		return Task.CompletedTask;
	}

	[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
	public async Task Level2( GameCtx ctx ) {
		var gs = ctx.GameState;
		var tm = gs.Island.Terrain_ForFear;
		foreach(var spirit in gs.Spirits) {
			var options = gs.AllActiveSpaces.Where( s => tm.IsInPlay(s) && tm.IsCoastal(s) && s.Has( Invader.Town ) ).ToArray();
			if(options.Length == 0) return;
			var target = await spirit.Gateway.Decision( new Select.Space( "Replace town with explorer", options, Present.Always ) );
			await ReplaceInvader.Downgrade( spirit.BindSelf( gs, ctx.ActionScope ).Target( target), Present.Done, Invader.Town );
		}
	}

	[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
	public async Task Level3( GameCtx ctx ) {
		var gs = ctx.GameState;
		var tm = gs.Island.Terrain_ForFear;
		foreach(var spirit in gs.Spirits) {
			var options = gs.AllActiveSpaces.Where( s => tm.IsInPlay( s ) && tm.IsCoastal( s ) && s.HasAny(Invader.Town,Invader.City) ).Select(s=>s.Space).ToArray();
			if(options.Length == 0) return;
			var target = await spirit.Gateway.Decision( new Select.Space( "Replace town with explorer or city with town", options, Present.Always ));
			await ReplaceInvader.Downgrade( spirit.BindSelf( gs, ctx.ActionScope ).Target( target ), Present.Done, Invader.Town, Invader.City );
		}
	}

}
namespace SpiritIsland.Basegame;

public class GatherPresenceIntoOcean : SpiritAction {

	public GatherPresenceIntoOcean():base( "Gather 1 Presence into EACH Ocean" ) { }
	public override async Task ActAsync( Spirit self ) {

		Dictionary<Space, SpaceToken[]> oceans2 = GameState.Current.Island.Boards.Select(b=>b.Ocean.ScopeSpace)
			.Select(o=> new { 
				dst=o, 
				src = self.Presence.Token.On( o.Adjacent.Where(self.Presence.IsOn) ).ToArray() 
			})
			.Where(x=>0<x.src.Length)
			.ToDictionary(x=>x.dst,x=>x.src);

		foreach(var pair in oceans2){
			var source = await self.SelectAlwaysAsync( new A.SpaceTokenDecision(
				$"Select source of Presence to Gather into {pair.Key.SpaceSpec}"
				, pair.Value
				, Present.Always
			));
			await source.MoveTo( pair.Key );
		}
	}

}
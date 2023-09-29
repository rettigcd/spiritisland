namespace SpiritIsland.NatureIncarnate;

public class RadiantAndHallowedGrove {
	const string Name = "Radiant and Hallowed Grove";

	[SpiritCard( Name, 2, Element.Sun, Element.Moon, Element.Fire, Element.Plant ), Fast, FromSacredSite(0)] // !!! Incara
	[Instructions( "2 Fear if Invaders are present or adjacent. In both target and one adjacent land, you may Remove an Invader with Health less than or equal to the Terror Level." ), Artist( Artists.AalaaYassin )]
	static async public Task ActAsync( SelfCtx ctx ) {
		await Task.Run( () => { } );
	}

}

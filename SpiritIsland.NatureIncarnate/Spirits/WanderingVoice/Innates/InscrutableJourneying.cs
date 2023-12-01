namespace SpiritIsland.NatureIncarnate;

[InnatePower("Inscrutable Journeying")]
[Slow, Yourself]
public class InscrutableJourneying {

	[InnateTier("1 air","You may Push Incarna",0)]
	static public Task Option1( Spirit self ) => PushIncarna(1,self);

	[InnateTier("3 air","You may Push Incarna",0)]
	static public Task Option2( Spirit self ) => PushIncarna(2,self);

	[InnateTier("5 air","You may Push Incarna",0)]
	static public Task Option3( Spirit self )  => PushIncarna(3,self);

	[InnateTier("2 moon,1 fire,4 air,1 plant","Empower Incarna",1)]
	static public Task Option4( Spirit self ) => new EmpowerIncarna().ActAsync( self );

	static async Task PushIncarna( int count, Spirit self ) {
		 var cmd = new PushIncarna();
		while(0 < count--) await cmd.ActAsync( self );
	}

}
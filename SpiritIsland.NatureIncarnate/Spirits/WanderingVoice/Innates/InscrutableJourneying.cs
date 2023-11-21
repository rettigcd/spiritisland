namespace SpiritIsland.NatureIncarnate;

[InnatePower("Inscrutable Journeying")]
[Slow, Yourself]
public class InscrutableJourneying {

	[InnateTier("1 air","You may Push Incarna",0)]
	static public Task Option1( SelfCtx ctx ) => PushIncarna(1,ctx);

	[InnateTier("3 air","You may Push Incarna",0)]
	static public Task Option2( SelfCtx ctx ) => PushIncarna(2,ctx);

	[InnateTier("5 air","You may Push Incarna",0)]
	static public Task Option3( SelfCtx ctx )  => PushIncarna(3,ctx);

	[InnateTier("2 moon,1 fire,4 air,1 plant","Empower Incarna",1)]
	static public Task Option4( SelfCtx ctx ) => new EmpowerIncarna().ActAsync( ctx );

	static async Task PushIncarna( int count, SelfCtx ctx ) {
		 var cmd = new PushIncarna();
		while(0 < count--) await cmd.ActAsync( ctx );
	}

}
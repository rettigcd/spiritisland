#nullable enable
namespace SpiritIsland;

static public class SkipInvaderAction_Extensions {

	/// <summary>
	/// Skips 1 invader action - which one is picked later.
	/// </summary>
	static public void Skip1InvaderAction( this Space ss, string label, Spirit actionPicker, Func<Space,Task>? alternateAction = null ) { 
		ss.Adjust( new SkipAnyInvaderAction(label,actionPicker,alternateAction), 1 );
	}

	static public void SkipAllInvaderActions( this Space ss, string label ) {
		ss.SkipRavage( label, UsageDuration.SkipAllThisTurn );
		ss.SkipAllBuilds( label );
		ss.Adjust( new SkipExploreTo(skipAll:true), 1 );
	}

	static public void SkipRavage( this Space ss, string label, UsageDuration duration = UsageDuration.SkipOneThisTurn ) 
		=> ss.Adjust( new SkipRavage(label, duration), 1 );

	static public void Skip1Build( this Space ss, string label ) 
		=> ss.Adjust( SkipBuild.Default( label ), 1 );

	static public void SkipAllBuilds( this Space ss, string label, params ITokenClass[] stoppedClasses ) 
		=> ss.Adjust( new SkipBuild( label, UsageDuration.SkipAllThisTurn, stoppedClasses ), 1 );

	static public void Skip1Explore( this Space ss, string _ ) 
		=> ss.Adjust( new SkipExploreTo(), 1 );

}

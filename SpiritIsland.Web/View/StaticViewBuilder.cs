using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SpiritIsland.Web.View;

/// <summary> Projects the parts of the game that rarely change. See <see cref="StaticView"/>. Plain engine reads, no web types. </summary>
public static class StaticViewBuilder {

	public static StaticView Build( GameState gs ) {
		ActionScope.Initialize( gs.RootScope ); // engine state is read through the AsyncLocal scope
		var screen = ScreenCoordinates.For( gs ); // the engine's y points up, a screen's down
		var content = new StaticView(
			StaticVersion: "",
			new SetupView( [.. gs.Island.Boards.Select( b => b.Name )], AdversaryOf( gs ), gs.ShuffleNumber ),
			Bounds( screen.World ),
			[.. gs.Spaces_Unfiltered.Select( s => GeometryOf( s, screen ) )],
			[.. gs.Spirits.Select( SpiritOf )] );
		return content with { StaticVersion = Version( content ) };
	}

	/// <summary> The version is a hash of everything else, so it changes exactly when something here does. </summary>
	static string Version( StaticView content )
		=> Convert.ToHexString( SHA256.HashData( Encoding.UTF8.GetBytes( JsonSerializer.Serialize( content ) ) ) )[..8].ToLowerInvariant();

	static AdversaryView? AdversaryOf( GameState gs )
		=> gs.Adversary is { Level: var level } adversary && adversary.Builder is not NullAdversaryBuilder
			? new AdversaryView( adversary.Name, level, adversary.Describe() ) : null;

	static SpaceGeometry GeometryOf( Space space, ScreenCoordinates screen ) {
		SpaceLayout layout = space.SpaceSpec.Layout;
		ManageInternalPoints points = new( layout );
		return new SpaceGeometry(
			space.SpaceSpec.Label,
			string.Join( "/", Enum.GetValues<Terrain>().Where( t => t != Terrain.None && space.SpaceSpec.Is( t ) ) ),
			[.. layout.Corners.Select( screen.Point )],
			screen.Point( points.NameLocation ),
			points.TokenSize,
			[.. space.Adjacent_Existing.Select( a => a.SpaceSpec.Label )] );
	}

	static SpiritStatic SpiritOf( Spirit s ) => new(
		s.SpiritName, s.SpiritName,
		[.. s.Presence.Energy.Slots.Select( SlotOf )], [.. s.Presence.CardPlays.Slots.Select( SlotOf )],
		[.. s.GrowthTrack.Groups.Select( g => new GrowthGroupView( g.ToString(), [.. g.GrowthActionFactories.Select( f => ((IOption)f).Text )], g.GainEnergy ) )],
		s.GrowthTrack.PickGroups.Sum( p => p.MustPickCount ), PickNote( s.GrowthTrack ),
		[.. s.InnatePowers.Select( p => new InnateStatic( p.Title, p.Speed.ToString(), p.TargetFilter, p.RangeText, p.GeneralInstructions,
			[.. p.DrawableOptions.Select( t => new TierView( t.ThresholdString, t.Description ) )] ) )],
		[.. s.SpecialRules.Select( r => new RuleView( r.Title, r.Description ) )] );

	/// <summary> Where the picks come from, when it is not just "any of them": the groups are named G1, G2... in order, as the client labels them. </summary>
	static string PickNote( GrowthTrack track ) {
		if( track.PickGroups.Count == 1 ) return "";
		int first = 1;
		List<string> parts = [];
		foreach( PickGroups pick in track.PickGroups ) {
			string names = string.Join( ", ", Enumerable.Range( first, pick.Groups.Length ).Select( n => $"G{n}" ) );
			parts.Add( pick.Groups.Length == 1 ? names : $"{pick.MustPickCount} of {names}" );
			first += pick.Groups.Length;
		}
		return string.Join( "; ", parts );
	}

	static SlotView SlotOf( Track slot )
		=> new( slot.Code, ((IOption)slot).Text, slot.Energy, slot.CardPlay, [.. slot.Elements.Select( e => e.ToString() )] );

	static BoundsView Bounds( Bounds b ) => new( b.X, b.Y, b.Width, b.Height );
}

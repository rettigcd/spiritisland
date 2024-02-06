namespace SpiritIsland;

/// <summary>
/// Draws: Speed, Range, Target head attributes on a given Graphics object.
/// </summary>
public static class PowerHeaderDrawer {

	static public RowRect AttributeValuesRow( IFlexibleSpeedActionFactory power ) {

		return new RowRect(
			Col1_Speed( power ).FloatSelf(), // remove Width so they distribute evenly
			Col2_SourceRange( power ),
			Col3_Target( power.TargetFilter )
		){ 
			Between = Pens.SaddleBrown, 
			Background = Brushes.BlanchedAlmond,
//			WidthRatio = 12f
		};
	}

	static public IPaintableRect Col1_Speed( IFlexibleSpeedActionFactory power ) {
		var imgRect = new ImgRect( power.DisplaySpeed == Phase.Slow ? Img.Icon_Slow : Img.Icon_Fast );
		return new PoolRect{ WidthRatio = imgRect.WidthRatio }
			.Float(imgRect,.1f,.1f,.8f,.8f);
	}

	#region Range (Center)

	static public IPaintableRect Col2_SourceRange( IFlexibleSpeedActionFactory power ) {
		string sourceRangeText = power.RangeText;

		if(sourceRangeText == "-")
			// draw dash
			return new ImgRect( Img.NoRange ).FloatSelf(20,0,60,100); // Blazing Renewal


		string[] parts = sourceRangeText.Split( ':' );
		IPaintableRect range = Col2_Range( parts[0] );
		string sourceCriteria = 1<parts.Length ? parts[1] : string.Empty;

		// Range only (from presence-impledi)
		if(parts.Length == 1) return range;

		// From: Sacred Site
		if(sourceCriteria == "ss")
			return new ImgRect(Img.Icon_Sacredsite)
				.FloatSelf( x:10, w:40, y:10, h:80 )
				.Float( range, x:.4f, w:.6f, y:0f, h:1f ); // range

		// From: Presence (with filter)
		var presenceRect = new ImgRect(Img.Icon_Presence);
		Img[] imgs = sourceCriteria.Split( ',' ).Select( x=>TargetToImg(x).img ).ToArray();

		// Black Icon
		if( Col2_IsBlackIcon(imgs[0])) // "Wrack with Pain and Grief"  "Coordinated Raid"  "Call to Vigilance"
			return new ImgRect( imgs[0] ).FloatSelf(x:5, w:25 ,y:0,h:100)
				.Float( presenceRect, x:30, w:25, y:0, h:100 )
				.Float( range, x:50, w:60, y:0, h:100 ); // range

		// Single Terrain
		if( imgs.Length == 1 ) // "Tigers Hunting", "Cleansing Flood"
			return new ImgRect(imgs[0]).FloatSelf(2,0,50,100)
				.Float( presenceRect, x:20, w:40, y:45, h:70)
				.Float( range, x:45, w:60, y:0, h:100 ); // range

		// Multiple Terrain
		return new PoolRect()  // "Bombard with Boulders and Stinging Seeds"
			.Float( new ImgRect(imgs[0]), x:0, w:80, y:0, h:80 )
			.Float( new ImgRect(imgs[1]), x:40, w:80, y:0, h:80 )
			.Float( presenceRect, x:30, w:70, y:40, h:80 )
			.FloatSelf( x:10, w:40, y:10, h:80)
			.Float( range, x:40, w:60, y:0, h:100 ); // range

	}
	
	static bool Col2_IsBlackIcon(Img img) => img switch {
		Img.Icon_Blight or Img.Icon_Dahan => true, 
		Img.Icon_Jungle or Img.Icon_Ocean or Img.Icon_Wetland or Img.Icon_Sands or Img.Icon_Mountain => false,
		_ => throw new ArgumentException($"{img} b/w status not specified.")
	};

	static PoolRect Col2_Range( string text ) {
		return new PoolRect()
			.Float(new TextRect(text), 0,.1f,1,.8f)
			.Float(new ImgRect(Img.RangeArrow), 0,.7f,1,.2f);
	}

	#endregion

	static public IPaintableRect Col3_Target( string text ) 
		=> Col3_Target_Inner(text).FloatSelf(x:0,w:100, y:10,h:80); // add 10% top and bottom margins

	static IPaintableRect Col3_Target_Inner( string text ) {
		const string CoastOrWetlands = "Coast_Wetlands";
		text = text switch { "coastal/Wetland" => CoastOrWetlands, _ => text };

		string[] parts = Col3_SplitTargetIntoParts(text);

		// 2 parts
		if(parts.Length == 2)
			// Test Cases: A Dreadful Tide of Scurrying Flesh, Bargain of Cousing Paths, Death falls gentely, Fleshrot Fever, Melt Earth into Quicksand
			return new RowRect( 
				new NullRect{WidthRatio=null},
				Col3_Target_Inner( parts[0] ),
				new NullRect{WidthRatio=.1f},
				Col3_Target_Inner( parts[1] ),
				new NullRect{WidthRatio=null}
			);

		// Image
		(Img img,Img overlay) = TargetToImg( text );
		if(img != Img.None) {
			IPaintableRect rect = new ImgRect( img );
			if(overlay != Img.None)
				rect = rect.FloatSelf()
					.Float(new ImgRect( overlay ));
			return rect;
		}

		const string narrow6 = "Arial Narrow;.6;bold";
		const string narrow8 = "Arial Narrow;.8;bold";
		const string arial8 = "Arial;.8;bold";

		// Special Cases
		return text switch {
			Filter.BlightAndInvaders                => new ImgRect(Img.Blight).FloatSelf(2,0,25,100)
														.Float( new TextRect("+INVADERS"){ Font=narrow6 }, x:15,w:100, y:0,h:100 ), // "Threating Flames"

			// spirits
			AnySpiritAttribute.TargetFilterText     => new TextRect("ANY"){ Font=arial8, Horizontal = StringAlignment.Near }.FloatSelf(x:15,w:50,y:15,h:100)
														.Float( new ImgRect(Img.Icon_Spirit), x:49,w:50,y:0,h:100), // "Blazing Renwal", "Gift of Power"
			YourselfAttribute.TargetFilterText      => new TextRect( "YOURSELF" ){ Font=narrow8 }.FloatSelf(0,13,100,100), // "Reach from the Infinite Darkness"
			AnotherSpiritAttribute.TargetFilterText => new RowRect( new TextRect("ANOTHER"){ Font=narrow6 }, new ImgRect(Img.Icon_Spirit), new NullRect{WidthRatio=.2f} ), // "Absorb Essence"

			// Coastal
			Filter.Coastal                          => new TextRect("COASTAL"){ Font=narrow8 }.FloatSelf(0,15,100,100), // "Call of the Deeps"
			CoastOrWetlands                         => new TextRect("COASTAL /"){ Font=narrow6, Horizontal=StringAlignment.Near }.FloatSelf().Float( new ImgRect(Img.Icon_Wetland),70,0,30,100 ), // "Sea Monsters"
			Filter.CoastalCity                      => new TextRect("COASTAL"  ){ Font=narrow6, Horizontal=StringAlignment.Near  }.FloatSelf(3,0,100,100)
															.Float( new ImgRect(Img.Icon_City), 65,0,30,100 ), // "Plague Ships Sail to Distant Ports"

			// text
			_                                       => new TextRect( text.ToUpper() ){ Font=arial8 }.FloatSelf(0,20,100,100), // "Inland: Softly Beckon..." , Any // ANY:"Asphyxiating Smoke"
		};
	}

	static string[] Col3_SplitTargetIntoParts( string text ){
		if(text[0] == '2') {
			int index = text[1] == ' ' ? 2 : 1;
			string sub = text[index..];
			return [sub,sub];
		}

		int splitIndex = text.IndexOf( '/' );
		//		if(splitIndex == -1 )
		//			splitIndex = text.IndexOf( '+' );
		return splitIndex != -1 
			? ([ text[..splitIndex], text[(splitIndex + 1)..] ]) 
			: ([text]);
	}

	static (Img img,Img overlay) TargetToImg( string text ){
		bool isNot = text.StartsWith( "No" );
		Img overlay = isNot ? Img.NoX : Img.None;
		if(isNot) text = text.Split( ' ' )[1]; // just 2nd word

		Img img = text switch {
			"Spirit"           => Img.Icon_Spirit,
			Filter.Jungle      => Img.Icon_Jungle,
			Filter.Sands       => Img.Icon_Sands,
			Filter.Mountain    => Img.Icon_Mountain,
			Filter.Wetland     => Img.Icon_Wetland,
			Filter.Ocean       => Img.Icon_Ocean,
			Filter.Blight      => Img.Icon_Blight,
			Filter.Dahan       => Img.Icon_Dahan,
			Filter.City        => Img.Icon_City,
			Filter.Town        => Img.Icon_Town,
			Filter.Disease     => Img.Icon_Disease,
			Filter.Wilds       => Img.Icon_Wilds,
			Filter.Beast       => Img.Icon_Beast,
			Filter.Presence    => Img.Icon_Presence,
			Filter.EndlessDark => Img.Icon_EndlessDark,
			Filter.Quake       => Img.Icon_Quake,
			Filter.Invaders    => Img.Icon_Invaders,
			Filter.Strife      => Img.Icon_Strife,
			"Incarna"          => Img.Icon_Incarna,
			_                  => Img.None
		};
		return (img,overlay);
	}

}

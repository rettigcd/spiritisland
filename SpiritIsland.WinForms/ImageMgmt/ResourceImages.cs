using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SpiritIsland.WinForms;

// $$$
public class ResourceImages {

	static public readonly ResourceImages Singleton = new ResourceImages();

	ResourceImages() {
		assembly = System.Reflection.Assembly.GetExecutingAssembly();
		Fonts = new PrivateFontCollection();
		LoadFont( "leaguegothic-regular-webfont.ttf" );
		LoadFont( "playsir-regular.otf" );
	}

	#region Fonts

	readonly PrivateFontCollection Fonts;

	public Font UseGameFont( float fontHeight ) => new Font( Fonts.Families[0], fontHeight, GraphicsUnit.Pixel );
	public Font UseInvaderFont( float fontHeight ) => new Font( Fonts.Families[1], fontHeight, GraphicsUnit.Pixel );

	void LoadFont(string file) {
		string resource = "SpiritIsland.WinForms." + file;
		using Stream fontStream = assembly.GetManifestResourceStream( resource );               
		
		// read the fond data into a buffer
		byte[] fontdata = new byte[fontStream.Length];
		fontStream.Read( fontdata, 0, (int)fontStream.Length );

		// copy the bytes to the unsafe memory block
		IntPtr data = Marshal.AllocCoTaskMem( (int)fontStream.Length );
		Marshal.Copy( fontdata, 0, data, (int)fontStream.Length );

		// pass the font to the font collection
		Fonts.AddMemoryFont( data, (int)fontStream.Length );
		// free up the unsafe memory
		Marshal.FreeCoTaskMem( data );
	}

	#endregion

	public Bitmap GetPresenceImage( string img )    => GetResourceImage( $"presence.{img}.png" );
	public Bitmap GetAdversaryFlag( string adv )    => GetResourceImage($"adversaries.{adv}.png" );
	public Bitmap GetImage( Img img )               => GetResourceImage( ToResource( img ) );
	public Bitmap Strife()                          => GetResourceImage("tokens.strife.png");
	public Bitmap Fear()                            => GetResourceImage("tokens.fear.png");
	public Bitmap FearGray()                        => GetResourceImage("tokens.fear_gray.png");
	public Bitmap FearCard()                        => GetResourceImage("tokens.fearcard.png");
	public Bitmap RedX()                            => GetResourceImage("icons.red-x.png");
	public Bitmap Hourglass()                       => GetResourceImage("icons.hourglass.png");
	public Bitmap TerrorLevel( int terrorLevel )    => GetResourceImage($"icons.TerrorLevel{terrorLevel}.png" );
	public Bitmap GetInvaderCard(string text)       => GetResourceImage($"invaders.{text}.jpg");
	
	public Bitmap GetInvaderCardBack()              => GetInvaderCard("back");

	public Image GetInvaderCard( InvaderCard card ) {

		string key = $"invaders.{card.Text}.png";
		if( _cache.Contains( key ) ) return _cache.Get(key);

		// -- Build and Save to Cache --
		Bitmap image = new Bitmap( 200, 320 );
		using Graphics graphics = Graphics.FromImage( image );

		var perimeter = new PointF[] {
			// Top Half
			new PointF( 20    , 160 ),
			new PointF( 20-10 , 90 ),
			new PointF( 20    , 20 ), // top left
			new PointF( 100   , 25 ),
			new PointF( 180   , 20 ), // top right
			new PointF( 180+10, 90 ),
			new PointF( 180   , 160 ),
			// Bottom Half
			new PointF( 180   , 160 ),
			new PointF( 180+10, 230 ),
			new PointF( 180   , 290 ), // bottom right
			new PointF( 100   , 285 ), 
			new PointF( 20    , 290 ), // bottom left
			new PointF( 20-10 , 230 ),
			new PointF( 20    , 160 ),
		};

		// Background
		var backgroundBrush = Brushes.Bisque;//  Brushes.SaddleBrown; // Brushes.BurlyWood; // or maybe Bisque
		graphics.FillRoundedRectangle( backgroundBrush, new Rectangle(0,0,200,320), 20);

		using var perimeterPen = new Pen(Color.Black, 3f );

		// Draw perimeter and inner texture
		if( card.Filter is SingleTerrainFilter singleTerrain ) {
			RectangleF topRect = new RectangleF( 30, 45, 200 - 2 * 30, 160 - 60 );
			RectangleF botRect = new RectangleF( 30, 160 + 15, 200 - 2 * 30, 160 - 60 );

			// texture
			using Brush terrainBrush = UseTerrainBrush( singleTerrain.Terrain );
			float tension = .15f;
			graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

			// Abreviation Text in the middle
			using Font bigFont = UseInvaderFont( 60f );
			graphics.DrawStringCenter( singleTerrain.Terrain.ToString()[..1], bigFont, backgroundBrush, topRect );

			// Escalation
			if( card.HasEscalation ) {
				var ellipseRect = botRect.InflateBy( -45, -20 );
				graphics.FillEllipse( backgroundBrush, ellipseRect );
				using Bitmap escalation = GetResourceImage( $"invaders.escalation.png" );
				graphics.DrawImageFitHeight( escalation, ellipseRect.InflateBy( -5 ) );
			}

		} else if( card.Filter is DoubleTerrainFilter doubleTerrain ){
			RectangleF topRect = new RectangleF( 30, 30 + 15, 200 - 2 * 30, 160 - 60 );
			RectangleF botRect = new RectangleF( 30, 160 + 25, 200 - 2 * 30, 160 - 60 );

			float tension = .15f;
			int countPerSide = perimeter.Length/2;

			// texture - 1
			using Brush brush1 = UseTerrainBrush( doubleTerrain.Terrain1 );
			graphics.FillClosedCurve( brush1, perimeter.Take(countPerSide).ToArray(), FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );
			// texture - 2
			using Brush brush2 = UseTerrainBrush( doubleTerrain.Terrain2 );
			graphics.FillClosedCurve( brush2, perimeter.Skip(countPerSide).Take(countPerSide).ToArray(), FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

			using Font bigFont = UseInvaderFont( 60f );
			// Text
			graphics.DrawStringCenter( doubleTerrain.Terrain1.ToString()[..1], bigFont, backgroundBrush, topRect );
			graphics.DrawStringCenter( doubleTerrain.Terrain2.ToString()[..1], bigFont, backgroundBrush, botRect );

		} else {

			// must be coastal
			RectangleF topRect = new RectangleF( 30, 30+30, 200 - 2 * 30, 160 - 60 );
			RectangleF botRect = new RectangleF( 30, 160+20 + 15, 200 - 2 * 30, 160 - 60 );

			// texture
			using Brush terrainBrush = UseTerrainBrush( Terrain.Ocean );
			float tension = .15f;
			graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

			// Abreviation Text in the middle
			using Font bigFont = UseInvaderFont( 25f );
			graphics.DrawStringCenter( "Coastal", bigFont, backgroundBrush, topRect );
			graphics.DrawStringCenter( "Lands", bigFont, backgroundBrush, botRect );
		}

		// Stage at bottom
		using Font bottomFont = UseInvaderFont(20f);
		graphics.DrawStringCenter( 
			card.InvaderStage switch { 1=>"I",2=>"II",3=>"III",_=>"" }, 
			bottomFont, Brushes.Brown,
			new RectangleF(0,285,200,30)
		);

		_cache.Add(key, image );
		return image;
	}

	public Size CalcImageSize( Img img, int maxDimension ) {
		if(!iconSizes.ContainsKey( img )) {
			using Image image = GetImage( img );
			iconSizes.Add( img, image.Size );
		}
		var sz = iconSizes[img];

		return sz.Width < sz.Height
			? new Size( maxDimension * sz.Width / sz.Height, maxDimension )
			: new Size( maxDimension, maxDimension * sz.Height / sz.Width );
	}
	readonly Dictionary<Img, Size> iconSizes = new Dictionary<Img, Size>();

	public Brush UseSpaceBrush( Space space ) {
		Terrain terrain = space.IsWetland ? Terrain.Wetland
			: space.IsJungle ? Terrain.Jungle
			: space.IsMountain ? Terrain.Mountain
			: space.IsSand ? Terrain.Sand
			: space.IsOcean ? Terrain.Ocean
			: throw new ArgumentException( $"No terrain found for {space.Text}", nameof( space ) );
		return UseTerrainBrush( terrain );
	}

	public Brush UseTerrainBrush( Terrain terrain ) {
		using Image image = UseTerrainImage( terrain );
		return new TextureBrush( image );
	}

	public Image UseTerrainImage( Terrain terrain ) {
		string terrainName = terrain switch {
			Terrain.Wetland => "wetlands",
			Terrain.Jungle => "jungle",
			Terrain.Mountain => "mountains",
			Terrain.Sand => "sand",
			Terrain.Ocean => "ocean",
			_ => throw new ArgumentException($"{terrain} not mapped"),
		};

		HSL terrainColor = terrain switch {
			Terrain.Wetland => new HSL( 184, 40, 45 ),
			Terrain.Jungle => new HSL( 144, 60, 40 ),
			Terrain.Mountain => new HSL( 45, 10, 33 ),
			Terrain.Sand => new HSL( 38, 50, 40 ),
			_ => null
		};

		// No HSL, use original
		static string SrcPath( string terrainName ) => $".\\images\\{terrainName}.jpg";
		if( terrainColor == null ) return Image.FromFile( SrcPath(terrainName) );

		// Check Cache
		string key = $"{terrainName} {terrainColor}.jpg";
		if(_cache.Contains(key)) return _cache.Get(key);

		// Build it, & cache it
		Bitmap image = (Bitmap)Image.FromFile( SrcPath( terrainName ) );
		new PixelAdjustment( new HslColorAdjuster( terrainColor ).GetNewColor ).Adjust( (Bitmap)image );
		_cache.Add(key, image);
		return image;
	}

	readonly ImageCache _cache = new ImageCache();

	#region private

	Bitmap GetResourceImage( string filename ) {
		var imgStream = assembly.GetManifestResourceStream( "SpiritIsland.WinForms.images."+filename );
		return new Bitmap( imgStream );
	}

	static string ToResource( Img img ) => img switch {
		Img.Starlight_AssignElement => "icons.AssignElement.png",
		Img.CardPlay => "icons.cardplay.png",
		Img.Reclaim1 => "icons.reclaim 1.png",
		Img.ReclaimAll => "icons.ReclaimAll.png",
		Img.ReclaimHalf => "icons.Reclaim_Half.png",
		Img.CardPlayPlusN => "icons.Cardplayplus.png",
		Img.PlusOneRange => "icons.PlusOneRange.png",
		Img.Push1dahan => "icons.Push1dahan.png",
		Img.GainCard => "icons.GainCard.png",
		Img.MovePresence => "icons.MovePresence.png",

		Img.Pushfromocean => "icons.Pushfromocean.png",
		Img.GatherToOcean => "icons.Gathertoocean.png",
		Img.Damage_2 => "icons.Damage_2.png",
		Img.Oneenergyfire => "icons.Oneenergyfire.png",
		Img.Land_Gather_Beasts => "icons.Land_Gather_Beasts.png",
		Img.Land_Push_Town_City => "icons.Land_Push_Town-City.png",
		Img.GainEnergyEqualToCardPlays => "icons.GainEnergyEqualToCardPlays.png",

		Img.RangeArrow => "icons.Range.png",
		Img.MoveArrow => "icons.Moveicon.png",

		Img.Stone_Minor => "icons.minor.png",
		Img.ShiftingMemory_PrepareEl => "icons.PrepareElement.png",
		Img.ShiftingMemory_Discard2Prep => "icons.DiscardElementsForCardPlay.png",
		Img.Discard2 => "icons.Discard2Cards.png",
		Img.Starlight_GrowthOption1 => "icons.so1.png",
		Img.Starlight_GrowthOption2 => "icons.so2.png",
		Img.Starlight_GrowthOption3 => "icons.so3.png",
		Img.Starlight_GrowthOption4 => "icons.so4.png",
		Img.FracturedDays_Gain2Time => "icons.Gain2time.png",
		Img.FracturedDays_Gain1Timex2 => "icons.Gain1timex2.png",
		Img.FracturedDays_Gain1Timex3 => "icons.Gain1timex3.png",
		Img.FracturedDays_DrawDtnw => "icons.Daysthatneverweregrowthicon.png",


		Img.Coin         => "tokens.coin.png",
		Img.Token_Sun    => "tokens.Simple_sun.png",
		Img.Token_Moon   => "tokens.Simple_moon.png",
		Img.Token_Fire   => "tokens.Simple_fire.png",
		Img.Token_Air    => "tokens.Simple_air.png",
		Img.Token_Water  => "tokens.Simple_water.png",
		Img.Token_Plant  => "tokens.Simple_plant.png",
		Img.Token_Earth  => "tokens.Simple_earth.png",
		Img.Token_Animal => "tokens.Simple_animal.png",
		Img.Token_Any    => "tokens.Simple_any.png",

		Img.City     => "tokens.city.png",
		Img.Town     => "tokens.town.png",
		Img.Explorer => "tokens.explorer.png",
		Img.Dahan    => "tokens.dahan.png",
		Img.Blight   => "tokens.blight.png",
		Img.Beast    => "tokens.beast.png",
		Img.Wilds    => "tokens.wilds.png",
		Img.Disease  => "tokens.disease.png",
		Img.Badlands => "tokens.badlands.png",
		Img.Defend   => "tokens.defend1orange.png",
		Img.Isolate  => "tokens.isolateorange.png",

		Img.Icon_Sun    => "icons.Simple_sun.png",
		Img.Icon_Moon   => "icons.Simple_moon.png",
		Img.Icon_Fire   => "icons.Simple_fire.png",
		Img.Icon_Air    => "icons.Simple_air.png",
		Img.Icon_Water  => "icons.Simple_water.png",
		Img.Icon_Plant  => "icons.Simple_plant.png",
		Img.Icon_Earth  => "icons.Simple_earth.png",
		Img.Icon_Animal => "icons.Simple_animal.png",

		Img.Icon_Dahan              => "icons.Dahanicon.png",
		Img.Icon_JungleOrWetland    => "icons.Junglewetland.png",
		Img.Icon_DahanOrInvaders    => "icons.DahanOrInvaders.png",
		Img.Icon_Coastal            => "icons.Coastal.png",
		Img.Icon_PresenceOrWilds    => "icons.wildsorpresence.png",
		Img.Icon_NoBlight           => "icons.Noblight.png",
		Img.Icon_BeastOrJungle      => "icons.JungleOrBeast.png",
		Img.Icon_Ocean              => "icons.Ocean.png",
		Img.Icon_MountainOrPresence => "icons.mountainorpresence.png",
		Img.Icon_TownCityOrBlight   => "icons.TownCityOrBlight.png",
		Img.Icon_Blight             => "icons.Blighticon.png",
		Img.Icon_Beast              => "icons.Beasticon.png",
		Img.Icon_Fear               => "icons.Fearicon.png",
		Img.OpenTheWays             => "icons.open-the-ways.png",
		Img.Icon_Wilds              => "icons.Wildsicon.png",
		Img.Icon_Fast               => "icons.Fasticon.png",
		Img.Icon_Presence           => "icons.Presenceicon.png",
		Img.Icon_Slow               => "icons.Slowicon.png",
		Img.Icon_Disease            => "icons.Diseaseicon.png",
		Img.Icon_Strife             => "icons.Strifeicon.png",
		Img.Icon_Badlands           => "icons.Badlands.png",
		Img.Icon_DestroyedPresence  => "icons.Destroyedpresence.png",
		Img.Icon_City               => "icons.Cityicon.png",
		Img.Icon_Town               => "icons.Townicon.png",
		Img.Icon_Explorer           => "icons.Explorericon.png",
		Img.Icon_Checkmark          => "icons.Checkmark.png",
		Img.Icon_Play               => "icons.Play.png",


		Img.Deck_Hand               => "hand.png",
		Img.Deck_Played => "inplay.png",
		Img.Deck_Discarded => "discard.png",
		Img.Deck_DaysThatNeverWere_Major => "major_inverted.png",
		Img.Deck_DaysThatNeverWere_Minor => "minor_inverted.png",

		Img.None => null,
		_ => throw new System.ArgumentOutOfRangeException( nameof( img ), img.ToString() ),
	};

	readonly Assembly assembly;

	#endregion

}

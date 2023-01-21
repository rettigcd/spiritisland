using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
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
	public Bitmap FearCard()                        => GetResourceImage("tokens.fearcard.png");
	public Bitmap RedX()                            => GetResourceImage("icons.red-x.png");
	public Bitmap Hourglass()                       => GetResourceImage("icons.hourglass.png");
	public Bitmap TerrorLevel( int terrorLevel )    => GetResourceImage($"icons.TerrorLevel{terrorLevel}.png" );
	public Bitmap GetInvaderCard(string text)       => GetResourceImage($"invaders.{text}");
	
	public Image GetInvaderCard( InvaderCard card ) {
		if( !card.Flipped )
			return GetInvaderCardBack( card.InvaderStage );

		string key = $"invaders.{card.Text}.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap image = InvaderCardBuilder.BuildInvaderCard( card );
		_cache.Add( key, image );
		return image;
	}

	Image GetInvaderCardBack( int stage ) {
		string key = $"invaders.back_{stage}.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap image = InvaderCardBuilder.BuildInvaderCardBack( stage );
		_cache.Add( key, image );
		return image;
	}


	public Image GetBlightCard( IBlightCard card ) {
		string key = "bi_" + card.Name + ".png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap bitmap = BlightCardBuilder.BuildBlighted( card );

		// save
		_cache.Add( key, bitmap );
		return bitmap;
	}

	public Image GetHealthBlightCard() {
		string key = "bi_healthy.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap bitmap = BlightCardBuilder.BuildHealthy();

		// save
		_cache.Add( key, bitmap );
		return bitmap;
	}

	public Image GetGhostImage( Img img ) {

		string key = $"ghost {img}.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap image = GetImage( img );
		new PixelAdjustment( MakePartiallyTransparent ).Adjust( image );

		_cache.Add( key, image );
		return image;
	}

	public Image FearGray() { 
		string key = "fear_gray.png";
		if(_cache.Contains( key )) return _cache.Get( key );
		Bitmap image = Fear();
		new PixelAdjustment( LowerContrast ).Adjust( image );
		_cache.Add( key, image );
		return image;
	}
	static Color LowerContrast( Color x ) => Color.FromArgb( x.A, x.R / 4 + 96, x.G / 4 + 96, x.B / 4 + 96 );
	static Color MakePartiallyTransparent( Color x ) => Color.FromArgb( Math.Min( (byte)92, x.A ), x );

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

	public Bitmap GetResourceImage( string filename ) {
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

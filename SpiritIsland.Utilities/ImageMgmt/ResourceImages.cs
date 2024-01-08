using SpiritIsland.Tests.Core;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SpiritIsland;

/// <summary>
/// Used by IPaintableImageRect to get the images it needs to paint.
/// </summary>
public interface ImgSource {
	Bitmap GetImg(Img img);
}

public class ResourceImages 
	: ImgSource
	, PowerCardResources
	, InvaderCardResources
	, FearCardResources
	, IconResources
{

	static public readonly ResourceImages Singleton = new ResourceImages();

	readonly Assembly _assembly;

	ResourceImages() {
		_assembly = typeof(ResourceImages).Assembly; // System.Reflection.Assembly.GetExecutingAssembly();
		Fonts = new PrivateFontCollection();
		LoadFont( "leaguegothic-regular-webfont.ttf" );
		LoadFont( "playsir-regular.otf" );
	}

	#region Fonts

	readonly PrivateFontCollection Fonts;

	public Font UseGameFont( float fontHeight ) => new Font( Fonts.Families[0], Math.Max(4,fontHeight), GraphicsUnit.Pixel );

	void LoadFont(string file) {
		string resource = "SpiritIsland.Utilities." + file;
		using Stream? fontStream = _assembly.GetManifestResourceStream( resource ) 
			?? throw new ArgumentException($"No font stream found for {file}");

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

#pragma warning disable CA1822 // Mark members as static
	public Bitmap LoadSpiritImage( string spiritText ) {
		string filename = spiritText.Replace( ' ', '_' );
		return (Bitmap)Image.FromFile( $".\\images\\spirits\\{filename}.jpg" );
	}
#pragma warning restore CA1822 // Mark members as static

	public Bitmap AdversaryFlag( string advName ) => GetResourceImage( $"adversaries.{advName}.png" );
	public Bitmap FearCardBack()                    => GetResourceImage( "tokens.fearcard.png" );
	public Bitmap TerrorLevel( int terrorLevel )    => GetResourceImage( $"icons.TerrorLevel{terrorLevel}.png" );

	public Bitmap GetImg( Img img ) => GetResourceImage( ToResource( img ) );

	/// <summary> Backgrounds for Fear Cards </summary>
	public Bitmap CardTexture( string texture )		=> GetResourceImage( $"textures.{texture}" );

	#region Caching

	public Bitmap GetInvaderCard( InvaderCard card ) {
		return card.Flipped
			? Front( card )
			: Back( card.InvaderStage );

		Bitmap Front( InvaderCard card ) {
			string key = $"invaders\\{card.Text}.png";
			if(_cache.Contains( key )) return _cache.Get( key );

			Bitmap image = InvaderCardBuilder.BuildInvaderCard( card, this );
			_cache.Add( key, image );
			return image;
		}

		Bitmap Back( int stage ) {
			string key = $"invaders\\back_{stage}.png";
			if(_cache.Contains( key )) return _cache.Get( key );

			Bitmap image = InvaderCardBuilder.BuildInvaderCardBack( stage, this );
			_cache.Add( key, image );
			return image;
		}

	}

	public Bitmap GetFearCard( IFearCard card ) {

		const bool saveSpace = false;
		string key = $"fear\\{card.Text}." + (saveSpace ? "jpg" : "png");
		if(_cache.Contains( key )) 
			return _cache.Get( key );

		Bitmap img = FearCardImageBuilder.Build( card, this );
		_cache.Add( key, img );
		return img;
	}

	public Bitmap GetBlightCard( IBlightCard card ) {
		string key = "blight\\" + card.Name + ".png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap bitmap = BlightCardBuilder.BuildBlighted( card );

		// save
		_cache.Add( key, bitmap );
		return bitmap;
	}

	public Bitmap GetMiscAction( string name ) {
		string key = "misc_action\\" + name + ".png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap bitmap = new Bitmap(200,300); // !!! add a builder here

		// save
		_cache.Add( key, bitmap );
		return bitmap;
	}


	public Bitmap GetHealthBlightCard() {
		string key = "blight\\healthy.png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap bitmap = BlightCardBuilder.BuildHealthy();

		// save
		_cache.Add( key, bitmap );
		return bitmap;
	}

	public Bitmap GetTrackSlot( IconDescriptor icon ) {

		static string GetCacheKey( IconDescriptor icon ) {
			var items = new List<string>();
			if(!string.IsNullOrEmpty( icon.Text )) items.Add( icon.Text );
			if(icon.BackgroundImg != default) items.Add( icon.BackgroundImg.ToString() );

			if(icon.ContentImg != default) items.Add( "1-" + icon.ContentImg.ToString() );
			if(icon.ContentImg2 != default) items.Add( "2-" + icon.ContentImg2.ToString() );
			if(icon.Super != default) items.Add( "Sup(" + GetCacheKey( icon.Super ) + ")" );
			if(icon.Sub != default) items.Add( "Sub(" + GetCacheKey( icon.Sub ) + ")" );
			if(icon.BigSub != default) items.Add( "(" + GetCacheKey( icon.BigSub ) + ")" );
			return string.Join( " ", items );
		}

		string key = "track\\" + GetCacheKey( icon ) + ".png";
		if(_cache.Contains( key )) return _cache.Get( key );

		Bitmap bitmap = IconDrawer.BuildTrackSlot( icon, this );

		_cache.Add( key, bitmap );
		return bitmap;
	}

	public Bitmap GetInnateOption( IDrawableInnateTier innateOption, float emSize, Size rowSize ) {
		string key = "innateOptions\\" + innateOption.Text.Replace( ' ', '_' ).Replace( '/', '_' ).Replace( '.', '_' ) + ".png";
		if(_cache.Contains( key )) {
			var image = _cache.Get( key );
			// To get max resolution, Image should be as wide as rowSize - caller can scale it down
			if(rowSize.Width < image.Width) return image;
			// Image is narrower than ro
			image.Dispose(); // too small - regenerate
		}

		Bitmap bitmap = BuildInnateOption( innateOption, emSize, rowSize );

		_cache.Add( key, bitmap );
		return bitmap;
	}

	public Bitmap GetGeneralInstructions( string description, float textEmSize, Size rowSize ) {
		// Cropping General Instructions to 50.  why?
		if(50 < description.Length) description = description[..50];
		string key = "innateOptions\\gi_" + description.Replace( ' ', '_' ) + ".png";
		if(_cache.Contains( key )) {
			var image = _cache.Get( key );
			if(rowSize.Width < image.Width) return image;
			image.Dispose(); // cached image is too small - regenerate larger
		}
		Bitmap bitmap = BuildGeneralInstructions( description, textEmSize, rowSize );
		_cache.Add( key, bitmap );
		return bitmap;
	}

	public async Task<Image> GetPowerCard( PowerCard card ) {
		try {
			ImageDiskCache _cache = new ImageDiskCache();
			string key = $"PowerCard\\{card.Name}.png";
			if(_cache.Contains( key )) return _cache.Get( key );

			Bitmap image = (Bitmap)await PowerCardImageBuilder.Build( card, this ); // don't dispose, we are returning it
			_cache.Add( key, image );
			return image;
		}
		catch(Exception ex) {
			_ = ex.ToString();
			throw;
		}
	}

	public Image GetSpiritMarker( Spirit spirit, Img img ) {
		ImageDiskCache _cache = new ImageDiskCache();
		string key = $"SpiritMarkers\\{spirit.Text}-{img}.png";
		if(_cache.Contains( key )) return _cache.Get( key );
		Bitmap image = SpiritMarkerBuilder.BuildSpiritMarker( spirit, img, this );
		_cache.Add( key, image );
		return image;
	}

	#endregion Caching

	#region interface PowerCardResources

	async Task<Image> PowerCardResources.GetPowerCardImage( PowerCard card ) {
		string key = $"power_card_pic\\{card.Name}.png";
		if(_cache.Contains( key )) return _cache.Get( key );
		Bitmap bitmap = await GetPowerCardImage_Internal( card );
		_cache.Add( key, bitmap );
		return bitmap;
	}
	static async Task<Bitmap> GetPowerCardImage_Internal( PowerCard card ) {
		try {
			return await CardDownloader.GetImage( card.Name );
		}
		catch {
			return new Bitmap( 24, 24, System.Drawing.Imaging.PixelFormat.Format32bppPArgb );
		}
	}

	Bitmap PowerCardResources.GetPhaseCost( Phase phase ) => GetResourceImage( $"tokens.Cost{phase}.png" );

	#endregion interface PowerCardResources

	#region interface InvaderCardResources

	/// <summary> Images for drawing Invader cards. </summary>
	public Bitmap InvaderCardImage( string backOrEscalation ) => GetResourceImage( $"invaders.{backOrEscalation}" );

	public Font UseInvaderFont( float fontHeight ) => new Font( Fonts.Families[1], fontHeight, GraphicsUnit.Pixel );

	#endregion interface InvaderCardResources

	public Brush UseSpaceBrush( Space space ) {
		Terrain terrain
			= space.IsWetland ? Terrain.Wetland
			: space.IsJungle ? Terrain.Jungle
			: space.IsMountain ? Terrain.Mountain
			: space.IsSand ? Terrain.Sands
			: (space.IsOcean || space.IsDestroyed) ? Terrain.Ocean
			: Terrain.None; // throw new ArgumentException( $"No terrain found for {space.Text}", nameof( space ) );
		return UseTerrainBrush( terrain );
	}

	public Brush UseTerrainBrush( Terrain terrain ) {
		string terrainName = terrain switch {
			Terrain.Wetland  => "wetlands",
			Terrain.Jungle   => "jungle",
			Terrain.Mountain => "mountains",
			Terrain.Sands    => "sand",
			Terrain.Ocean    => "ocean",
			Terrain.None     => "none",
			_ => throw new ArgumentException( $"{terrain} not mapped" ),
		};
		using Image image = (Bitmap)Image.FromFile( $".\\images\\terrain\\{terrainName}.jpg" );
		return new TextureBrush( image );
	}

	public Image GetTokenImage( IToken token ) {
		return token is HumanToken ht ? HumanTokenBuilder.Build( ht )
			: token.GetType().Name == "ManyMindsBeast" ? GetManyMindsBeast( "many-minds-beast.png", token.Img, 60, 40 )
			: token.GetType().Name == "MarkedBeast" ? GetManyMindsBeast( "marked-beast.png", token.Img, 240, 20 )
			: GetImg( token.Img );
	}

	static Bitmap BuildGeneralInstructions( string description, float textEmSize, Size rowSize ) {
		Bitmap bitmap;
		{
			using var tempBitmap = new Bitmap( rowSize.Width, rowSize.Width ); // Height is wrong
			using Graphics graphics = Graphics.FromImage( tempBitmap );
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

			var config = new ConfigWrappingLayout {
				EmSize = textEmSize,
				ElementDimension = (int)(textEmSize * 2.4f),
				IconDimension = (int)(textEmSize * 1.8f),
				HorizontalAlignment = Align.Near
			};

			var layout = new WrappingLayout( config, rowSize, graphics );
			layout.Append( description, FontStyle.Regular );
			layout.FinalizeBounds();
			layout.Paint( graphics );

			bitmap = tempBitmap.Clone( new Rectangle( new Point( 0, 0 ), layout.Size ), tempBitmap.PixelFormat );

		}

		return bitmap;
	}

	static Bitmap BuildInnateOption( IDrawableInnateTier innateOption, float emSize, Size rowSize ) {
		using Bitmap tempBitmap = new Bitmap( rowSize.Width, rowSize.Width * 2 ); // Height is wrong
		using Graphics graphics = Graphics.FromImage( tempBitmap );
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

		var config = new ConfigWrappingLayout {
			EmSize = emSize,
			ElementDimension = (int)(emSize * 2.4f),
			IconDimension = (int)(emSize * 1.8f),
			HorizontalAlignment = Align.Near,
			Indent = rowSize.Height / 2,
		};
		// Elements Thresholds
		var layout = new WrappingLayout( config, rowSize, graphics );

		// top overhand margin
		layout.AddIconRowOverflowHeight(); // create top buffer for over hanging enlarged tokens

		// Elements in bold
		layout.Append( innateOption.ThresholdString, FontStyle.Bold );

		// Tab
		layout.Tab( 2, FontStyle.Bold );

		// Text
		layout.Append( innateOption.Description, FontStyle.Regular );

		layout.AddIconRowOverflowHeight(); // create bottom buffer for over hanging enlarged tokens

		layout.FinalizeBounds();

		layout.Paint( graphics );

		return tempBitmap.Clone( new Rectangle( new Point( 0, 0 ), layout.Size ), tempBitmap.PixelFormat );

	}


	#region private

	Bitmap GetManyMindsBeast(string key,Img baseImg, int hue, int saturation) {
		if(_cache.Contains(key)) return _cache.Get(key);

		Bitmap img = GetImg(baseImg);
		using Graphics graphics = Graphics.FromImage( img );

		new PixelAdjustment(new HslColorAdjuster(new HSL( hue, saturation,50)).GetNewColor).Adjust(img);

		_cache.Add(key,img);
		return img;
	}

	Bitmap GetResourceImage( string? filename ) {
		if(filename is null) return new Bitmap( 1, 1 );
		Stream imgStream = _assembly.GetManifestResourceStream( "SpiritIsland.Utilities.images." + filename )
			?? throw new ArgumentException( $"No resource image found for {filename}" );
		return new Bitmap( imgStream );
	}

	static string? ToResource( Img image ) => image switch {

		Img.Starlight_AssignElement => "icons.AssignElement.png",

		Img.CardPlay      => "icons.Card_Play.png",
		Img.CardPlayPlusN => "icons.Card_BonusPlay.png",
		Img.GainCard      => "icons.Card_Gain1.png",
		Img.ImpendingCard => "icons.Card_Impending.png",
		Img.Discard1      => "icons.Card_Discard1.png",
		Img.Discard2      => "icons.Card_Discard2.png",


		Img.Reclaim1 => "icons.reclaim 1.png",
		Img.ReclaimAll => "icons.ReclaimAll.png",
		Img.ReclaimHalf => "icons.Reclaim_Half.png",
		Img.PlusOneRange => "icons.PlusOneRange.png",
		Img.Icon_Major => "icons.major.png",
		Img.MovePresence => "icons.MovePresence.png",

		Img.Pushfromocean => "icons.Pushfromocean.png",
		Img.GatherToOcean => "icons.Gathertoocean.png",
		Img.Damage_2 => "icons.Damage_2.png",
		Img.Oneenergyfire => "icons.Oneenergyfire.png",

		Img.Land_Gather_Beasts  => "icons.Land_Gather_Beasts.png",
		Img.Land_Gather_Blight  => "icons.Land_Gather_Blight.png",
		Img.Land_Gather_Dahan   => "icons.Land_Gather_Dahan.png",
		Img.Land_Push_Town_City => "icons.Land_Push_Town-City.png",
		Img.Land_Push_Incarna   => "icons.Land_Push_Incarna.png",
		Img.Land_Push_Dahan     => "icons.Land_Push_Dahan.png",

		Img.GainEnergyEqualToCardPlays => "icons.GainEnergyEqualToCardPlays.png",
		Img.RangeArrow => "icons.Range.png",
		Img.MoveArrow => "icons.Moveicon.png",

		Img.Stone_Minor => "icons.minor.png",
		Img.ShiftingMemory_PrepareEl => "icons.PrepareElement.png",
		Img.ShiftingMemory_Discard2Prep => "icons.DiscardElementsForCardPlay.png",
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
		Img.Strife   => "tokens.strife.png",
		Img.Badlands => "tokens.badlands.png",
		Img.Vitality => "tokens.vitality.png",
		Img.Quake    => "tokens.quake.png",
		Img.Defend   => "tokens.defend1orange.png",
		Img.Isolate  => "tokens.isolateorange.png",

		Img.Token_Presence => "presence.red.png",

		Img.Icon_Sun    => "icons.Elements.sun.png",
		Img.Icon_Moon   => "icons.Elements.moon.png",
		Img.Icon_Fire   => "icons.Elements.fire.png",
		Img.Icon_Air    => "icons.Elements.air.png",
		Img.Icon_Water  => "icons.Elements.water.png",
		Img.Icon_Plant  => "icons.Elements.plant.png",
		Img.Icon_Earth  => "icons.Elements.earth.png",
		Img.Icon_Animal => "icons.Elements.animal.png",

		Img.Icon_Dahan              => "icons.Dahanicon.png",
		Img.Icon_Invaders           => "icons.Invaders.png",
		Img.Icon_Coastal            => "icons.Coastal.png",
		Img.Icon_PresenceOrWilds    => "icons.wildsorpresence.png",
		Img.Icon_NoBlight           => "icons.No_Blight.png",
		Img.Icon_TownCityOrBlight   => "icons.TownCityOrBlight.png",
		Img.Icon_Blight             => "icons.Blighticon.png",
		Img.Icon_Beast              => "icons.Beasticon.png",
		Img.Icon_Vitality           => "icons.Vitality.png",
		Img.Icon_Quake              => "icons.Quake.png",
		Img.Icon_Fear               => "icons.Fearicon.png",
		Img.OpenTheWays             => "icons.open-the-ways.png",
		Img.Icon_Wilds              => "icons.Wildsicon.png",
		Img.Icon_Fast               => "icons.Fasticon.png",
		Img.Icon_Presence           => "icons.Presenceicon.png",
		Img.Icon_Sacredsite         => "icons.Sacredsite.png",
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

		Img.Icon_Sands				=> "icons.Terrain.Sands.png",
		Img.Icon_Mountain			=> "icons.Terrain.Mountain.png",
		Img.Icon_Jungle				=> "icons.Terrain.Jungle.png",
		Img.Icon_Wetland			=> "icons.Terrain.Wetlandland.png",
		Img.Icon_Ocean              => "icons.Terrain.Ocean.png",

		Img.DestroyedX              => "icons.DestroyedX.png",

		Img.Icon_Spirit             => "icons.Spiriticon.png",

		Img.OrCurlyBefore => "icons.OR_Curly.png",
		Img.OrCurlyAfter => "icons.OR_Curly_180.png",

		Img.Icon_Incarna             => "icons.Incarna.png",
		Img.BoDDYS_Incarna           => "incarna.BoDDYS.png",
		Img.BoDDYS_Incarna_Empowered => "incarna.BoDDYS+.png",
		Img.EEB_Incarna              => "incarna.EEB.png",
		Img.EEB_Incarna_Empowered    => "incarna.EEB+.png",
		Img.L_Incarna                => "incarna.L.png",
		Img.S_Incarna                => "incarna.S.png",
		Img.S_Incarna_Empowered      => "incarna.S+.png",
		Img.T_Incarna                => "incarna.T.png",
		Img.TRotJ_Incarna            => "incarna.TRotJ.png",
		Img.TRotJ_Incarna_Empowered  => "incarna.TRotJ+.png",
		Img.WVKD_Incarna             => "incarna.WVKD.png",
		Img.WVKD_Incarna_Empowered   => "incarna.WVKD+.png",

		Img.Fear                     => "tokens.fear.png",

		Img.Hourglass				=> "icons.hourglass.png",
		Img.ArtistPalette			=> "icons.artist-palette.png",
		Img.NoRange					=> "icons.No_Range.png",
		Img.NoX						=> "icons.No_X.png",


	Img.Icon_EndlessDark => "icons.EndlessDark.png",

		Img.None => null,
		_ => throw new System.ArgumentOutOfRangeException( nameof( image ), image.ToString() ),
	};

	readonly ImageDiskCache _cache = new ImageDiskCache();

	#endregion

}

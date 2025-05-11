#r "../lib/UndertaleModLib.dll"
#r "../lib/Underanalyzer.dll"
#load "./_setup_rich_presence.csx"

using UndertaleModLib.Util;
using UndertaleModLib.Models;
using UndertaleModLib;
using System.Text.Json.Serialization;
using System.Text.Json;

var datafile = new FileInfo("input/data.win");
UndertaleData gameData;

try
{
    using FileStream fs = datafile.OpenRead();
    UndertaleData gmData = UndertaleIO.Read(fs);
    gameData = gmData;
}
catch (FileNotFoundException e)
{
    throw new FileNotFoundException($"Data file '{e.FileName}' does not exist");
}


Dictionary<string, UndertaleEmbeddedTexture> textures = [];

foreach (string texturePath in Directory.EnumerateFiles("mod_files/textures", "*.png", SearchOption.TopDirectoryOnly))
{
    var textureBytes = File.ReadAllBytes(texturePath);

    UndertaleEmbeddedTexture texture = new();
    string textureName = Path.GetFileNameWithoutExtension(texturePath);
    texture.Name = new UndertaleString(textureName);
    texture.TextureData.Image = GMImage.FromPng(textureBytes);
    gameData.EmbeddedTextures.Add(texture);
    textures.Add(textureName, texture);
}

foreach (string spritePath in Directory.EnumerateFiles("mod_files/sprites", "*.json", SearchOption.TopDirectoryOnly))
{
    var spriteText = File.ReadAllText(spritePath);

    string spriteName = Path.GetFileNameWithoutExtension(spritePath);
    Sprite sprite = JsonSerializer.Deserialize<Sprite>(spriteText);


    UndertaleSimpleList<UndertaleSprite.TextureEntry> textureEntries = new();

    foreach (TexturePage texturePage in sprite.TexturePages) {
        UndertaleTexturePageItem newPage = new();

        newPage.SourceX = texturePage.SourcePosition[0];
        newPage.SourceY = texturePage.SourcePosition[1];
        newPage.SourceWidth = texturePage.SourceSize[0];
        newPage.SourceHeight = texturePage.SourceSize[1];
        newPage.TargetX = texturePage.TargetPosition[0];
        newPage.TargetY = texturePage.TargetPosition[1];
        newPage.TargetWidth = texturePage.TargetSize[0];
        newPage.TargetHeight = texturePage.TargetSize[1];
        newPage.BoundingWidth = texturePage.BoundingBoxSize[0];
        newPage.BoundingHeight = texturePage.BoundingBoxSize[1];
        newPage.TexturePage = textures[texturePage.EmbeddedTextureName];

        gameData.TexturePageItems.Add(newPage);

        UndertaleSprite.TextureEntry textureEntry = new();
        textureEntry.Texture = newPage;
        textureEntries.Add(textureEntry);
    }

    UndertaleSprite newSprite = new();

    var spriteString = gameData.Strings.MakeString(spriteName);

    newSprite.Name = spriteString;
    newSprite.Width = sprite.Size[0];
    newSprite.Height = sprite.Size[1];
    newSprite.MarginLeft = sprite.Margin[0];
    newSprite.MarginRight = sprite.Margin[1];
    newSprite.MarginBottom = sprite.Margin[2];
    newSprite.MarginTop = sprite.Margin[3];
    newSprite.OriginX = sprite.Origin[0];
    newSprite.OriginY = sprite.Origin[1];
    newSprite.BBoxMode = sprite.BoundingBoxMode;
    newSprite.IsSpecialType = true;
    newSprite.SVersion = sprite.Special.Version;
    newSprite.SSpriteType = sprite.Special.SpriteType;
    newSprite.GMS2PlaybackSpeedType = sprite.Special.PlaybackSpeedType;
    newSprite.GMS2PlaybackSpeed = sprite.Special.PlaybackSpeed;
    newSprite.Textures = textureEntries;

    gameData.Sprites.Add(newSprite);
}

SetupRichPresence(gameData);

var importGroup = new UndertaleModLib.Compiler.CodeImportGroup(gameData);

foreach (string scriptPath in Directory.EnumerateFiles("ufo50_modded_scripts", "*.gml", SearchOption.AllDirectories))
{
    var scriptText = File.ReadAllText(scriptPath);

    string scriptName = Path.GetFileNameWithoutExtension(scriptPath);

    importGroup.QueueReplace(scriptName, scriptText);
}

importGroup.Import();


using (var stream = new FileStream("/tmp/data.win", FileMode.Create, FileAccess.Write))
{
    UndertaleIO.Write(stream, gameData);
}

File.Copy("/tmp/data.win", "output/data.win", true);


public class Sprite
{
    [JsonPropertyName("size")]
    public List<ushort> Size { get; set; }

    [JsonPropertyName("margin")]
    public List<ushort> Margin { get; set; }

    [JsonPropertyName("origin")]
    public List<ushort> Origin { get; set; }

    [JsonPropertyName("bounding_box_mode")]
    public ushort BoundingBoxMode { get; set; }

    [JsonPropertyName("special")]
    public SpriteSpecialData Special { get; set; }

    [JsonPropertyName("texture_pages")]
    public List<TexturePage> TexturePages { get; set; }
}

public class SpriteSpecialData 
{
    [JsonPropertyName("version")]
    public ushort Version { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<UndertaleSprite.SpriteType>))]
    public UndertaleSprite.SpriteType SpriteType { get; set; }

    [JsonPropertyName("playback_speed")]
    public ushort PlaybackSpeed { get; set; }

    [JsonPropertyName("playback_speed_type")]
    [JsonConverter(typeof(JsonStringEnumConverter<AnimSpeedType>))]
    public AnimSpeedType PlaybackSpeedType { get; set; }
}

public class TexturePage
{
    [JsonPropertyName("source_position")]
    public List<ushort> SourcePosition { get; set; }

    [JsonPropertyName("source_size")]
    public List<ushort> SourceSize { get; set; }

    [JsonPropertyName("target_position")]
    public List<ushort> TargetPosition { get; set; }

    [JsonPropertyName("target_size")]
    public List<ushort> TargetSize { get; set; }

    [JsonPropertyName("bounding_size")]
    public List<ushort> BoundingBoxSize { get; set; }

    [JsonPropertyName("texture")]
    public string EmbeddedTextureName { get; set; }
}



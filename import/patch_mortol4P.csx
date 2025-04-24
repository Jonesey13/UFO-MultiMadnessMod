#r ".\lib\UndertaleModLib.dll"
#r ".\lib\Underanalyzer.dll"

using UndertaleModLib.Util;
using UndertaleModLib.Models;
using UndertaleModLib;
using System.Text.Json.Serialization;
using System.Text.Json;

var datafile = new FileInfo("data.win");
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


Dictionary<string, UndertaleEmbeddedTexture> textures = new();

foreach (string texturePath in Directory.EnumerateFiles("textures", "*.png", SearchOption.TopDirectoryOnly))
{
    var textureBytes = File.ReadAllBytes(texturePath);

    UndertaleEmbeddedTexture texture = new();
    string textureName = texturePath.Split("\\")[1].Split(".")[0];
    texture.Name = new UndertaleString(textureName);
    texture.TextureData.Image = GMImage.FromPng(textureBytes);
    gameData.EmbeddedTextures.Add(texture);
    textures.Add(textureName, texture);
}

foreach (string spritePath in Directory.EnumerateFiles("sprites", "*.json", SearchOption.TopDirectoryOnly))
{
    var spriteText = File.ReadAllText(spritePath);

    Sprite sprite = JsonSerializer.Deserialize<Sprite>(spriteText);

    List<UndertaleTexturePageItem> texturePages = new();

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
        texturePages.Add(newPage);
    }
}

using (var stream = new FileStream("data_modded.win", FileMode.Create, FileAccess.Write))
{
    UndertaleIO.Write(stream, gameData);
}


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
    public string Type { get; set; }

    [JsonPropertyName("playback_speed")]
    public ushort PlaybackSpeed { get; set; }

    [JsonPropertyName("playback_speed_type")]
    public string PlaybackSpeedType { get; set; }
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
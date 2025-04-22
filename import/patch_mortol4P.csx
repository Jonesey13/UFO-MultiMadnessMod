#r ".\lib\UndertaleModLib.dll"
#r ".\lib\Underanalyzer.dll"

using UndertaleModLib.Util;
using UndertaleModLib.Models;
using UndertaleModLib;


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

foreach (string texturePath in Directory.EnumerateFiles("textures", "*.png", SearchOption.TopDirectoryOnly))
{
    var textureBytes = File.ReadAllBytes(texturePath);

    UndertaleEmbeddedTexture texture = new();
    texture.Name = new UndertaleString(texturePath);
    texture.TextureData.Image = GMImage.FromPng(textureBytes);
    gameData.EmbeddedTextures.Add(texture);
}

using (var stream = new FileStream("data_modded.win", FileMode.Create, FileAccess.Write))
{
    UndertaleIO.Write(stream, gameData);
}
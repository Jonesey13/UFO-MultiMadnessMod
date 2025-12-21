using ImageMagick;
using System.Drawing;
using System;
using System.IO;
using System.Diagnostics;
using UndertaleModLib.Util;
using UndertaleModLib.Models;
using UndertaleModLib;
using VYaml.Annotations;
using VYaml.Serialization;
using Serilog;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.IO.Hashing;

ExportSprites("../../input/data.win", "original");
ExportSprites("../../output/data.win", "modded");
CompareAndCopyFiles("original", "modded", "converted");
MergeSpriteConfigurations("converted/config");


static void CompareAndCopyFiles(string vanillaFolder, string moddedFolder, string outputFolder)
{
    Log.Information("Executing filediff comparison");

    var vanillaFiles = Directory.GetFiles(vanillaFolder, "*.*", SearchOption.AllDirectories);
    var moddedFiles = Directory.GetFiles(moddedFolder, "*.*", SearchOption.AllDirectories);
    var regex = new Regex(@"^(.*?)(?=_f[0-9])", RegexOptions.Compiled);

    var vanillaFileNames = new ConcurrentDictionary<string, string>();

    Parallel.ForEach(vanillaFiles, vanillaFile =>
    {
        var match = regex.Match(Path.GetFileName(vanillaFile));
        if (match.Success)
        {
            string baseName = match.Groups[1].Value;
            //Log.Information($"Storing basename: {baseName} which equals to {vanillaFile}");
            vanillaFileNames[baseName] = vanillaFile;
        }
        else
        {
            vanillaFileNames[Path.GetFileName(vanillaFile)] = vanillaFile; // Store all other vanilla files
        }
    });

    _ = Parallel.ForEach(moddedFiles, moddedFile =>
    {
        string moddedFilename = Path.GetFileName(moddedFile);
        var match = regex.Match(Path.GetFileName(moddedFile));
        if (match.Success)
        {
            moddedFilename = match.Groups[1].Value;
            //Log.Information($"Storing fileName: {fileName} which equals to {moddedFile}");
        }

        string relativePath = Path.GetRelativePath(moddedFolder, moddedFile);
        string outputFilePath = Path.Combine(outputFolder, relativePath);

        // Always copy spriteData regardless of hash
        if (moddedFilename.Equals("data.json", StringComparison.OrdinalIgnoreCase))
        {
            Log.Information($"Copying {moddedFile}");
            MkDir(Path.GetDirectoryName(outputFilePath));
            File.Copy(moddedFile, outputFilePath, true);
            return;
        }

        if (vanillaFileNames.TryGetValue(moddedFilename, out string vanillaFile))
        {
            long vanillaHash = ComputeFileHash3(vanillaFile);
            long modHash = ComputeFileHash3(moddedFile);

            // Compare the files
            if (vanillaHash != modHash)
            {
                Log.Information($"File hash is different, Copying {Path.GetFileName(moddedFile)}");

                MkDir(Path.GetDirectoryName(outputFilePath));
                File.Copy(moddedFile, outputFilePath, true);
            }
        }
        else
        {
            Log.Information($"Vanilla file not found. Copying {Path.GetFileName(moddedFile)}");

            MkDir(Path.GetDirectoryName(outputFilePath));
            File.Copy(moddedFile, outputFilePath, true);
        }
    });
}

static void ExportSprites(string winFilePath, string outputFilePath) {
    const int MAX_WIDTH = 65536;

    bool padded = true;
    const string ext = ".png";

    string texFolder = outputFilePath + Path.DirectorySeparatorChar;
    string texConfigFolder = texFolder + "config";

    while (File.Exists(texFolder))
    {
        Log.Information("Please delete the Export folder before exporting.\n\nPress any key to continue..");
        Console.ReadKey();
    }


    var originalDatafile = new FileInfo(winFilePath);
    UndertaleData Data;

    try
    {
        using FileStream fs = originalDatafile.OpenRead();
        UndertaleData gmData = UndertaleIO.Read(fs);
        Data = gmData;
    }
    catch (FileNotFoundException e)
    {
        throw new FileNotFoundException($"Data file '{e.FileName}' does not exist");
    }

    Directory.CreateDirectory(texFolder);
    Directory.CreateDirectory(texConfigFolder);

    int coreCount = Environment.ProcessorCount - 1;
    // If you want to use all your cores just uncomment the code below
    coreCount = Environment.ProcessorCount;

    //Realistically no pc should ever only have a single core
    if (coreCount == 0)
        coreCount = 1;

    var options = new ParallelOptions { MaxDegreeOfParallelism = coreCount }; // Adjust the degree of parallelism
    Log.Information($"Using {coreCount} cores to dump the sprites");


    List<string> invalidSpriteNames = [];
    int invalidSprite = 0;

    List<string> invalidSpriteSizeNames = [];
    int invalidSpriteSize = 0;

    TextureWorker worker = null;

    using (worker = new())
    {
        Parallel.ForEach(Data.Sprites, options, DumpSprite);
    }

    Log.Information($"All sprite files has been exported to {Path.GetFullPath(texFolder)}");

    void DumpSprite(UndertaleSprite sprite)
    {
        // Cannot be cached outside of the function because of race condition, these variables needs to be reinitialized
        string spriteName = sprite.Name.Content;
        if (spriteName == "") {
            Log.Error("Skipped sprite that has an empty name to prevent an exception");
            return;
        }

        Log.Information($"Exporting {sprite.Name.Content}");
        int spriteFrame = 0;

        string fileName = spriteName + ext;

        // Calculate total width and maximum height for the strip
        uint totalWidth = 0;
        uint maxHeight = 0;

        List<IMagickImage<byte>> images = [];

        // Gather all textures as bitmaps
        foreach (var texture in sprite.Textures)
        {
            if (texture?.Texture != null)
            {
                var image = worker.GetTextureFor(texture.Texture, sprite.Name.Content, padded);
                images.Add(image);
                totalWidth += image.Width;
                maxHeight = Math.Max(maxHeight, image.Height);

                spriteFrame++;
            }
        }

        if (totalWidth == 0 || maxHeight == 0)
        {
            Log.Error($"Error, {spriteName} has invalid width or height");
            invalidSpriteSizeNames.Add(spriteName);
            invalidSpriteSize++;
            return;
        }

        if (totalWidth < MAX_WIDTH)
        {
            // can be optimized maybe
            var config = new SpriteData
            {
                yml_frame = spriteFrame,
                yml_x = sprite.OriginX,
                yml_y = sprite.OriginY,
                yml_transparent = sprite.Transparent,
                yml_smooth = sprite.Smooth,
                yml_preload = sprite.Preload,
                yml_boundingboxtype = sprite.BBoxMode,
                yml_bboxleft = sprite.MarginLeft,
                yml_bboxright = sprite.MarginRight,
                yml_bboxtop = sprite.MarginTop,
                yml_bboxbottom = sprite.MarginBottom,
                yml_sepmask = (uint)sprite.SepMasks,
                yml_speedtype = (uint)sprite.GMS2PlaybackSpeedType,
                yml_framespeed = sprite.GMS2PlaybackSpeed
            };
            var data = new Dictionary<string, SpriteData>
            {
                [spriteName] = config
            };
            var yamlBytes = YamlSerializer.Serialize<Dictionary<string, SpriteData>>(data);
            string yaml = System.Text.Encoding.UTF8.GetString(yamlBytes.Span);
            string configFileName = Path.Combine(texConfigFolder, spriteName + ".yaml");

            File.WriteAllText(configFileName, yaml);

            // Create the final strip image
            using var stripImage = new MagickImage(MagickColors.Transparent, totalWidth, maxHeight);
            int offsetX = 0; // Prefer `int` over `uint` for offsets (avoids casting issues)
            foreach (var image in images)
            {
                stripImage.Composite(image, offsetX, 0, CompositeOperator.Over); // Correct order
                offsetX += (int)image.Width; // Ensure offsetX is `int` (cast if needed)
                image.Dispose();
            }

            string stripPath = Path.Combine(texFolder, fileName);
            stripImage.Settings.SetDefine(MagickFormat.Png, "exclude-chunks", "all");
            stripImage.Settings.Compression = CompressionMethod.Zip;
            stripImage.Write(stripPath);
        }
        else
        {
            Log.Warning($"Skipped {sprite.Name.Content} because the width is over 65536");
            invalidSpriteNames.Add(sprite.Name.Content);
            invalidSprite++;
        }
    }
}

static void MergeSpriteConfigurations(string basePath)
{
    Log.Information("Merging sprite configuration files");
    string spriteConfigFileName = "MultiMadnessSpriteConfig.yaml";
    string[] spriteConfigFiles = Directory.GetFiles(basePath, "*.yaml", SearchOption.TopDirectoryOnly);
    
    if (spriteConfigFiles.Length != 0)
    {
        using (StreamWriter writer = new(spriteConfigFileName))
        {
            foreach (string file in spriteConfigFiles)
            {
                Log.Information($"Merging {Path.GetFileName(file)}");

                string fileContent = File.ReadAllText(file);
                writer.WriteLine(fileContent);
            }
        }
        foreach (string file in spriteConfigFiles)
        {
            File.Delete(file);
            Log.Debug($"Deleted: {Path.GetFileName(file)}");
        }
    }
    else
    {
        Log.Information("No sprite configuration files found, skipping the process");
    }

    if (File.Exists(spriteConfigFileName))
        File.Move(spriteConfigFileName, Path.Combine(basePath, spriteConfigFileName));
}

static void MkDir(string path)
{
    if (!Directory.Exists(path))
    {
        Directory.CreateDirectory(path);
    }
}

static long ComputeFileHash3(string filePath)
{
    using (var stream = File.OpenRead(filePath))
    {
        byte[] fileBytes = new byte[stream.Length];
        stream.Read(fileBytes, 0, (int)stream.Length);
        return BitConverter.ToInt64(XxHash3.Hash(fileBytes));
    }
}

[YamlObject]
public partial class SpriteData
{
    [YamlMember("frames")]
    public int? yml_frame { get; set; }  

    [YamlMember("x")]
    public int? yml_x { get; set; }      

    [YamlMember("y")]
    public int? yml_y { get; set; }      

    [YamlMember("transparent")]
    public bool? yml_transparent { get; set; }  

    [YamlMember("smooth")]
    public bool? yml_smooth { get; set; }      

    [YamlMember("preload")]
    public bool? yml_preload { get; set; }     

    [YamlMember("speed_type")]
    public uint? yml_speedtype { get; set; }   

    [YamlMember("frame_speed")]
    public float? yml_framespeed { get; set; }

    [YamlMember("bounding_box_type")]
    public uint? yml_boundingboxtype { get; set; }  

    [YamlMember("bbox_left")]
    public int? yml_bboxleft { get; set; }     

    [YamlMember("bbox_right")]
    public int? yml_bboxright { get; set; }    

    [YamlMember("bbox_bottom")]
    public int? yml_bboxbottom { get; set; }   

    [YamlMember("bbox_top")]
    public int? yml_bboxtop { get; set; }     

    [YamlMember("sepmasks")]
    public uint? yml_sepmask { get; set; }     
}
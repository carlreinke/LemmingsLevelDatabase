// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using System.IO.Compression;
using Tetractic.CommandLine;

namespace LemmingsLevelDatabase;

internal static class Program
{
    /// <exception cref="IOException"/>
    internal static int Main(string[] args)
    {
        var rootCommand = new RootCommand("LemmingsLevelDatabase");

        var imagerCommand = rootCommand.AddSubcommand("image", "Create image from level.");
        {
            var fileParameter = imagerCommand.AddParameter("file", "Path to level file.");

            imagerCommand.SetInvokeHandler(() =>
            {
                byte[] data = File.ReadAllBytes(fileParameter.Value);
                var imager = new LevelImager(new ZipArchive(Stream.Null, ZipArchiveMode.Create), string.Empty);
                var level = Lemmings.Data.Level.ReadFrom(data);
                var image = imager.GenerateImage(level);
                var pngEncoder = new PngEncoder()
                {
                    ColorType = PngColorType.Palette,
                    CompressionLevel = PngCompressionLevel.BestCompression,
                };
                image.SaveAsPng(fileParameter.Value + ".png", pngEncoder);
                return 0;
            });
        }

        rootCommand.HelpOption = rootCommand.AddOption('h', "help", "Shows a usage summary.");

        rootCommand.SetInvokeHandler(() =>
        {
            CommandHelp.WriteHelp(rootCommand, Console.Out, verbose: false);
            return 0;
        });

        try
        {
            return rootCommand.Execute(args);
        }
        catch (InvalidCommandLineException ex)
        {
            Console.Error.WriteLine(ex.Message);
            CommandHelp.WriteHelpHint(ex.Command, Console.Error);
            return -1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return -1;
        }
    }
}

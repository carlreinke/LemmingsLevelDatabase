// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Lemmings.Data;
using System;
using System.IO;
using Tetractic.CommandLine;

namespace Lemmings;

internal class Program
{
    internal static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Lemmings");

        var vgagrCommand = rootCommand.AddSubcommand("vgagr", "Show VGAGR?.DAT details.");
        {
            var pathParameter = vgagrCommand.AddParameter("path", "Path to data files.");
            var idParameter = vgagrCommand.AddParameter("n", "Number.");

            vgagrCommand.SetInvokeHandler(() =>
            {
                using (var groundStream = new FileStream($"{pathParameter.Value}\\ground{idParameter.Value}o.dat", FileMode.Open, FileAccess.Read))
                {
                    var ground = Ground.ReadFrom(groundStream);

                    Console.WriteLine("ObjectInfos:");

                    for (int i = 0; i < ground.ObjectInfos.Length; ++i)
                    {
                        var objectInfo = ground.ObjectInfos[i];

                        Console.WriteLine($"  {i}:");
                        Console.WriteLine($"    {nameof(objectInfo.UnknownAndAnimation)}:        {objectInfo.UnknownAndAnimation} ({objectInfo.Animation})");
                        Console.WriteLine($"    {nameof(objectInfo.AnimationInitialFrameIndex)}: {objectInfo.AnimationInitialFrameIndex}");
                        Console.WriteLine($"    {nameof(objectInfo.AnimationFrameCount)}:        {objectInfo.AnimationFrameCount}");
                        Console.WriteLine($"    {nameof(objectInfo.Width)}:                      {objectInfo.Width}");
                        Console.WriteLine($"    {nameof(objectInfo.Height)}:                     {objectInfo.Height}");
                        Console.WriteLine($"    {nameof(objectInfo.AnimationFrameLength)}:       {objectInfo.AnimationFrameLength}");
                        Console.WriteLine($"    {nameof(objectInfo.MaskOffset)}:                 {objectInfo.MaskOffset}");
                        Console.WriteLine($"    {nameof(objectInfo.Unknown10)}:                  {objectInfo.Unknown10}");
                        Console.WriteLine($"    {nameof(objectInfo.Unknown12)}:                  {objectInfo.Unknown12}");
                        Console.WriteLine($"    {nameof(objectInfo.EffectLeft)}:                 {objectInfo.EffectLeft}");
                        Console.WriteLine($"    {nameof(objectInfo.EffectTopPlus1)}:             {objectInfo.EffectTopPlus1} ({objectInfo.EffectTop})");
                        Console.WriteLine($"    {nameof(objectInfo.EffectWidth)}:                {objectInfo.EffectWidth}");
                        Console.WriteLine($"    {nameof(objectInfo.EffectHeight)}:               {objectInfo.EffectHeight}");
                        Console.WriteLine($"    {nameof(objectInfo.Effect)}:                     {(byte)objectInfo.Effect} ({objectInfo.Effect})");
                        Console.WriteLine($"    {nameof(objectInfo.AnimationFramesOffset)}:      {objectInfo.AnimationFramesOffset}");
                        Console.WriteLine($"    {nameof(objectInfo.ImageOffset)}:                {objectInfo.ImageOffset}");
                        Console.WriteLine($"    {nameof(objectInfo.Unknown25)}:                  {objectInfo.Unknown25}");
                        Console.WriteLine($"    {nameof(objectInfo.TrapSoundEffect)}:            {objectInfo.TrapSoundEffect}");
                    }

                    Console.WriteLine("PieceInfos:");

                    for (int i = 0; i < ground.PieceInfos.Length; ++i)
                    {
                        var pieceInfo = ground.PieceInfos[i];

                        Console.WriteLine($"  {i}:");
                        Console.WriteLine($"    {nameof(pieceInfo.Width)}:       {pieceInfo.Width}");
                        Console.WriteLine($"    {nameof(pieceInfo.Height)}:      {pieceInfo.Height}");
                        Console.WriteLine($"    {nameof(pieceInfo.ImageOffset)}: {pieceInfo.ImageOffset}");
                        Console.WriteLine($"    {nameof(pieceInfo.MaskOffset)}:  {pieceInfo.MaskOffset}");
                        Console.WriteLine($"    {nameof(pieceInfo.Unknown6)}:    {pieceInfo.Unknown6}");
                    }

                    Console.WriteLine("EgaPalette:");

                    for (int i = 0; i < 8; ++i)
                        Console.WriteLine($"  {nameof(ground.EgaCustomColors)}[{i}]: {ground.EgaCustomColors[i].R} {ground.EgaCustomColors[i].G} {ground.EgaCustomColors[i].B}");
                    for (int i = 0; i < 8; ++i)
                        Console.WriteLine($"  {nameof(ground.EgaStandardColors)}[{i}]: {ground.EgaStandardColors[i].R} {ground.EgaStandardColors[i].G} {ground.EgaStandardColors[i].B}");
                    for (int i = 0; i < 8; ++i)
                        Console.WriteLine($"  {nameof(ground.EgaPreviewColors)}[{i}]: {ground.EgaPreviewColors[i].R} {ground.EgaPreviewColors[i].G} {ground.EgaPreviewColors[i].B}");

                    Console.WriteLine("VgaPalette:");

                    for (int i = 0; i < 8; ++i)
                        Console.WriteLine($"  {nameof(ground.VgaCustomColors)}[{i}]: {ground.VgaCustomColors[i].R,3} {ground.VgaCustomColors[i].G,3} {ground.VgaCustomColors[i].B,3}");
                    for (int i = 0; i < 8; ++i)
                        Console.WriteLine($"  {nameof(ground.VgaStandardColors)}[{i}]: {ground.VgaStandardColors[i].R,3} {ground.VgaStandardColors[i].G,3} {ground.VgaStandardColors[i].B,3}");
                    for (int i = 0; i < 8; ++i)
                        Console.WriteLine($"  {nameof(ground.VgaPreviewColors)}[{i}]: {ground.VgaPreviewColors[i].R,3} {ground.VgaPreviewColors[i].G,3} {ground.VgaPreviewColors[i].B,3}");

                    _ = Console.ReadKey();

                    using (var vgaGroundStream = new FileStream($"{pathParameter.Value}\\vgagr{idParameter.Value}.dat", FileMode.Open, FileAccess.Read))
                    {
                        var vgaGround = VgaGround.ReadFrom(vgaGroundStream, ground);

                        for (int i = 0; i < vgaGround.PieceInfos.Length; ++i)
                        {
                            var info = vgaGround.PieceInfos[i];

                            if (info.Info.Height == 0)
                                continue;

                            WriteImageToConsole(info.Image, info.Info.Width, info.Info.Height);
                            Console.Write($"{i}");

                            _ = Console.ReadKey();
                        }

                        for (int i = 0; i < vgaGround.ObjectInfos.Length; ++i)
                        {
                            var info = vgaGround.ObjectInfos[i];

                            if (info.Info.Height == 0)
                                continue;

                            WriteImageToConsole(info.Image, info.Info.Width, info.Info.Height);
                            Console.Write($"{i}");

                            _ = Console.ReadKey();

                            for (int j = 0; j < info.Frames.Length; ++j)
                            {
                                WriteImageToConsole(info.Frames[j], info.Info.Width, info.Info.Height);
                                Console.Write($"{i} {j}");

                                _ = Console.ReadKey();
                            }
                        }
                    }
                }

                return 0;
            });
        }

        var vgaspecCommand = rootCommand.AddSubcommand("vgaspec", "Show VGASPEC?.DAT details.");
        {
            var pathParameter = vgaspecCommand.AddParameter("path", "Path to data files.");
            var idParameter = vgaspecCommand.AddParameter("n", "Number.");

            vgaspecCommand.SetInvokeHandler(() =>
            {
                using (var vgaSpecialStream = new FileStream($"{pathParameter.Value}\\vgaspec{idParameter.Value}.dat", FileMode.Open, FileAccess.Read))
                {
                    var vgaSpecial = VgaSpecial.ReadFrom(vgaSpecialStream);

                    for (int i = 0; i < 8; ++i)
                        Console.WriteLine($"{nameof(vgaSpecial.VgaCustomColors)}[{i}]: {vgaSpecial.VgaCustomColors[i].R,3} {vgaSpecial.VgaCustomColors[i].G,3} {vgaSpecial.VgaCustomColors[i].B,3}");
                    for (int i = 0; i < 8; ++i)
                        Console.WriteLine($"{nameof(vgaSpecial.EgaPreviewColors)}[{i}]: {vgaSpecial.EgaPreviewColors[i].R} {vgaSpecial.EgaPreviewColors[i].G} {vgaSpecial.EgaPreviewColors[i].B}");
                    for (int i = 0; i < 8; ++i)
                        Console.WriteLine($"{nameof(vgaSpecial.EgaCustomColors)}[{i}]: {vgaSpecial.EgaCustomColors[i].R} {vgaSpecial.EgaCustomColors[i].G} {vgaSpecial.EgaCustomColors[i].B}");

                    _ = Console.ReadKey();

                    for (int i = 0; i < vgaSpecial.Images.Length; ++i)
                    {
                        byte[] image = vgaSpecial.Images[i];

                        WriteImageToConsole(image, 960, 40);

                        _ = Console.ReadKey();
                    }
                }

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

    private static void WriteImageToConsole(byte[] image, int width, int height)
    {
        Console.Clear();

        int w = Math.Min(Console.BufferWidth - 1, width);
        int h = Math.Min(Console.BufferHeight - 1, height);

        for (int y = 0; y < h; y += 2)
        {
            for (int x = 0; x < w; ++x)
            {
                Console.ForegroundColor = (image[y * width + x] & 0x80) != 0
                    ? (ConsoleColor)(image[y * width + x] & 0x0F)
                    : ConsoleColor.Black;
                Console.BackgroundColor = y + 1 < height && (image[(y + 1) * width + x] & 0x80) != 0
                    ? (ConsoleColor)(image[(y + 1) * width + x] & 0x0F)
                    : ConsoleColor.Black;
                Console.Write("▀");
            }
            Console.WriteLine();
        }
        Console.ResetColor();
    }
}

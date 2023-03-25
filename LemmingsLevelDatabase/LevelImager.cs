// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Lemmings;
using Lemmings.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace LemmingsLevelDatabase
{
    internal sealed class LevelImager
    {
        private readonly SortedList<ushort, VgaGround> _grounds = new();

        private readonly ZipArchive _archive;

        private readonly string _directoryPath;

        public LevelImager(ZipArchive archive, string directoryPath)
        {
            _archive = archive;
            _directoryPath = directoryPath;
        }

        /// <exception cref="InvalidDataException"/>
        public Image GenerateImage(Level level)
        {
            if (level.GroundId > '~' - '0')
                throw new InvalidDataException();

            VgaGround? vgaGround;
            if (!_grounds.TryGetValue(level.GroundId, out vgaGround))
            {
                vgaGround = LoadVgaGround(_archive, _directoryPath, (char)('0' + level.GroundId));
                _grounds.Add(level.GroundId, vgaGround);
            }

            VgaSpecial? vgaSpecial = null;
            if (level.SpecialId != 0)
            {
                if (level.SpecialId - 1 > '~' - '0')
                    throw new InvalidDataException();

                vgaSpecial = LoadVgaSpecial(_archive, _directoryPath, (char)('0' + level.SpecialId - 1));
            }

            var palette = new VgaColor[16];

            for (int i = 0; i < 8; ++i)
                palette[i] = vgaGround.VgaStandardColors[i];

            byte[] piecesImage = new byte[1600 * 160];

            if (vgaSpecial != null)
            {
                palette[7] = new VgaColor(0x1F, 0x1F, 0x00);  // TODO: Where does this come from?
                for (int i = 0; i < 8; ++i)
                    palette[i + 8] = vgaSpecial.VgaCustomColors[i];

                LevelRenderer.RenderSpecial(vgaSpecial, piecesImage);
            }
            else
            {
                palette[7] = vgaGround.VgaCustomColors[0];
                for (int i = 0; i < 8; ++i)
                    palette[i + 8] = vgaGround.VgaCustomColors[i];

                LevelRenderer.RenderPieces(vgaGround, level, piecesImage);
            }

            byte[] objectsImage = new byte[1600 * 160];

            LevelRenderer.RenderObjects(vgaGround, level, piecesImage, objectsImage);

            // XXX
            LevelRenderer.RenderEffectBoundaries(vgaGround, level, objectsImage);

            var rgbImage = new Image<Rgb24>(1600, 160);
            rgbImage.ProcessPixelRows(accessor =>
            {
                Span<Rgb24> colors = stackalloc Rgb24[16];
                for (int i = 0; i < 16; ++i)
                    colors[i] = ToRgb24(palette[i]);

                int index = 0;
                for (int y = 0; y < 160; ++y)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < 1600; ++x)
                    {
                        int objectsPixel = objectsImage[index];
                        int piecesPixel = piecesImage[index];
                        int objectsMask = (sbyte)objectsPixel >> 7;
                        int piecesMask = (sbyte)(piecesPixel & ~objectsPixel) >> 7;
                        int pixel = (objectsPixel & objectsMask) |
                                    (piecesPixel & piecesMask);
                        row[x] = colors[pixel & 0x0F];

                        index += 1;
                    }
                }
            });

            return rgbImage;

            static Rgb24 ToRgb24(VgaColor vgaColor) => new Rgb24(vgaColor.R8, vgaColor.G8, vgaColor.B8);
        }

        private static Ground LoadGround(ZipArchive archive, string directoryPath, char id)
        {
            using (var groundStream = OpenFile(archive, directoryPath, $"GROUND{id}O.DAT"))
                return Ground.ReadFrom(groundStream);
        }

        private static VgaGround LoadVgaGround(ZipArchive archive, string directoryPath, char id)
        {
            var ground = LoadGround(archive, directoryPath, id);

            using (var vgaGrStream = OpenFile(archive, directoryPath, $"VGAGR{id}.DAT"))
                return VgaGround.ReadFrom(vgaGrStream, ground);
        }

        private static VgaSpecial LoadVgaSpecial(ZipArchive archive, string directoryPath, char id)
        {
            using (var vgaSpecStream = OpenFile(archive, directoryPath, $"VGASPEC{id}.DAT"))
                return VgaSpecial.ReadFrom(vgaSpecStream);
        }

        private static Stream OpenFile(ZipArchive archive, string directoryPath, string fileName)
        {
            // TODO: Load from path in archive.

            var stream = typeof(LevelImager).Assembly.GetManifestResourceStream($"{nameof(LemmingsLevelDatabase)}.GameFiles.{fileName}");
            if (stream != null)
                return stream;

            throw new FileNotFoundException();
        }
    }
}

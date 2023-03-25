// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using System;
using System.IO;

namespace Lemmings.Data;

internal sealed class VgaGround
{
    /// <inheritdoc cref="Ground.EgaCustomColors"/>
    public readonly EgaColor[] EgaCustomColors;

    /// <inheritdoc cref="Ground.EgaStandardColors"/>
    public readonly EgaColor[] EgaStandardColors;

    /// <inheritdoc cref="Ground.EgaPreviewColors"/>
    public readonly EgaColor[] EgaPreviewColors;

    /// <inheritdoc cref="Ground.VgaCustomColors"/>
    public readonly VgaColor[] VgaCustomColors;

    /// <inheritdoc cref="Ground.VgaStandardColors"/>
    public readonly VgaColor[] VgaStandardColors;

    /// <inheritdoc cref="Ground.VgaPreviewColors"/>
    public readonly VgaColor[] VgaPreviewColors;

    /// <summary>
    /// Information about the pieces, including images.
    /// </summary>
    public readonly VgaPieceInfo[] PieceInfos;

    /// <summary>
    /// Information about the objects, including images.
    /// </summary>
    public readonly VgaObjectInfo[] ObjectInfos;

    public VgaGround(
        EgaColor[] egaCustomColors,
        EgaColor[] egaStandardColors,
        EgaColor[] egaPreviewColors,
        VgaColor[] vgaCustomColors,
        VgaColor[] vgaStandardColors,
        VgaColor[] vgaPreviewColors,
        VgaPieceInfo[] pieceInfos,
        VgaObjectInfo[] objectInfos)
    {
        EgaCustomColors = egaCustomColors;
        EgaStandardColors = egaStandardColors;
        EgaPreviewColors = egaPreviewColors;
        VgaCustomColors = vgaCustomColors;
        VgaStandardColors = vgaStandardColors;
        VgaPreviewColors = vgaPreviewColors;
        PieceInfos = pieceInfos;
        ObjectInfos = objectInfos;
    }

    /// <exception cref="InvalidDataException"/>
    public static VgaGround ReadFrom(Stream stream, Ground ground)
    {
        using (var reader = new DatFileReader(stream, leaveOpen: true))
        {
            uint dataLength = reader.GetDecompressedLength();
            if (dataLength > 0x10000)
                throw new InvalidDataException();
            byte[] data = new byte[dataLength];
            reader.Decompress(data);

            var pieceInfos = new VgaPieceInfo[ground.PieceInfos.Length];

            for (int i = 0; i < ground.PieceInfos.Length; ++i)
            {
                var info = ground.PieceInfos[i];

                byte[] image = DecodeImage(data, info.Width, info.Height, info.ImageOffset, info.MaskOffset);

                pieceInfos[i] = new VgaPieceInfo(info, image);
            }

            dataLength = reader.GetDecompressedLength();
            if (dataLength > 0x10000)
                throw new InvalidDataException();
            data = new byte[dataLength];
            reader.Decompress(data);

            var objectInfos = new VgaObjectInfo[ground.ObjectInfos.Length];

            for (int i = 0; i < ground.ObjectInfos.Length; ++i)
            {
                var info = ground.ObjectInfos[i];

                byte[] image = DecodeImage(data, info.Width, info.Height, info.ImageOffset, (ushort)(info.ImageOffset + info.MaskOffset));

                byte[][] frames = new byte[info.AnimationFrameCount][];

                for (int j = 0; j < frames.Length; ++j)
                {
                    ushort imageOffset = (ushort)(info.AnimationFramesOffset + j * info.AnimationFrameLength);
                    frames[j] = DecodeImage(data, info.Width, info.Height, imageOffset, (ushort)(imageOffset + info.MaskOffset));
                }

                objectInfos[i] = new VgaObjectInfo(info, image, frames);
            }

            return new VgaGround(
                ground.EgaCustomColors,
                ground.EgaStandardColors,
                ground.EgaPreviewColors,
                ground.VgaCustomColors,
                ground.VgaStandardColors,
                ground.VgaPreviewColors,
                pieceInfos,
                objectInfos);
        }

        static byte[] DecodeImage(byte[] data, byte width, byte height, ushort imageOffset, ushort maskOffset)
        {
            if ((width & 7) != 0)
                throw new InvalidDataException();

            byte[] image = new byte[(width * height)];

            try
            {
                // Color is 4 bpp in bit planes.

                int planeLength = (width >> 3) * height;
                const int planesCount = 4;

                for (int p = 0; p < planesCount; ++p)
                {
                    int planeOffset = imageOffset + p * planeLength;

                    for (int j = 0; j < image.Length; ++j)
                    {
                        int offset = planeOffset + (j >> 3);
                        int shift = 7 - (j & 0x7);
                        image[j] |= (byte)((data[offset] >> shift & 1) << p);
                    }
                }

                // Mask is 1 bpp.  We'll put it into the uppermost bit.

                for (int j = 0; j < image.Length; ++j)
                {
                    int offset = maskOffset + (j >> 3);
                    int shift = 7 - (j & 0x7);
                    image[j] |= (byte)((data[offset] >> shift & 1) << 7);
                }

                return image;
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidDataException();
            }
        }
    }
}

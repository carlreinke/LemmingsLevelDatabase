// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using System;
using System.Diagnostics;
using System.IO;

namespace Lemmings.Data;

internal sealed class VgaSpecial
{
    /// <summary>
    /// VGA colors 8 to 15.
    /// </summary>
    public readonly VgaColor[] VgaCustomColors;

    /// <summary>
    /// EGA colors 8 to 15 used in the level preview.
    /// </summary>
    public readonly EgaColor[] EgaPreviewColors;

    /// <summary>
    /// EGA colors 8 to 15 used in-game.
    /// </summary>
    public readonly EgaColor[] EgaCustomColors;

    /// <summary>
    /// The 4 images.  Each image is 40 rows of 960 pixels.  Each pixel contains the color in the
    /// lower nibble and the mask in the most-significant bit.
    /// </summary>
    public readonly byte[][] Images;

    public VgaSpecial(
        VgaColor[] vgaCustomColors,
        EgaColor[] egaPreviewColors,
        EgaColor[] egaCustomColors,
        byte[][] images)
    {
        VgaCustomColors = vgaCustomColors;
        EgaPreviewColors = egaPreviewColors;
        EgaCustomColors = egaCustomColors;
        Images = images;
    }

    /// <exception cref="InvalidDataException"/>
    public static VgaSpecial ReadFrom(Stream stream)
    {
        using (var reader = new DatFileReader(stream, leaveOpen: true))
        {
            uint dataLength = reader.GetDecompressedLength();
            if (dataLength > 40 + (14400 * 2 + 1) * 4)
                throw new InvalidDataException();
            byte[] data = new byte[dataLength];
            reader.Decompress(data);

            var vgaCustomColors = VgaColor.Read8ColorsFrom(data.AsSpan(0, 24));

            var egaPreviewColors = EgaColor.Read8ColorsFrom(data.AsSpan(24, 8));
            var egaCustomColors = EgaColor.Read8ColorsFrom(data.AsSpan(32, 8));

            byte[][] images = new byte[4][];

            byte[] buffer = new byte[960 / 8 * 40 * 3];

            int ci = 40;

            for (int i = 0; i < images.Length; ++i)
            {
                int di = 0;

                try
                {
                    while (true)
                    {
                        byte b = data[ci];
                        ci += 1;

                        if (b < 0x80)
                        {
                            int length = b + 1;
                            for (int j = 0; j < length; ++j)
                                buffer[di + j] = data[ci + j];
                            ci += length;
                            di += length;
                        }
                        else if (b == 0x80)
                        {
                            Debug.Assert(di == buffer.Length);

                            break;
                        }
                        else
                        {
                            byte value = data[ci];
                            ci += 1;

                            int length = 257 - b;
                            for (int j = 0; j < length; ++j)
                                buffer[di + j] = value;
                            di += length;
                        }
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidDataException();
                }

                const int width = 960;
                const int height = 40;
                byte[] image = new byte[width * height];

                try
                {
                    // Color is 3 bpp in bit planes.

                    int planeLength = (width >> 3) * height;
                    const int planesCount = 3;

                    for (int p = 0; p < planesCount; ++p)
                    {
                        int planeOffset = p * planeLength;

                        for (int j = 0; j < image.Length; ++j)
                        {
                            int offset = planeOffset + (j >> 3);
                            int shift = 7 - (j & 0x7);
                            image[j] |= (byte)((buffer[offset] >> shift & 1) << p);
                        }
                    }

                    // Mask is transparent if all 3 bit planes are zero.  We'll
                    // put it into the uppermost bit.  There is an implicit 4th
                    // bit plane that is the same as the mask.

                    for (int j = 0; j < image.Length; ++j)
                    {
                        byte b = image[j];
                        image[j] = (byte)(b | (~(b - 1) & 0x88));
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidDataException();
                }

                images[i] = image;
            }

            Debug.Assert(ci == data.Length);

            return new VgaSpecial(
                vgaCustomColors,
                egaPreviewColors,
                egaCustomColors,
                images);
        }
    }
}

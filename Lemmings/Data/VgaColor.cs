// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using System;

namespace Lemmings.Data;

internal readonly struct VgaColor
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;

    public VgaColor(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public byte R8 => (byte)(R << 2 | R >> 4);

    public byte G8 => (byte)(G << 2 | G >> 4);

    public byte B8 => (byte)(B << 2 | B >> 4);

    /// <exception cref="ArgumentException">The length of <paramref name="data"/> is not 24.
    ///     </exception>
    public static VgaColor[] Read8ColorsFrom(ReadOnlySpan<byte> data)
    {
        if (data.Length != 8 * 3)
            throw new ArgumentException("Invalid length.", nameof(data));

        var colors = new VgaColor[8];

        for (int i = 0; i < colors.Length; ++i)
        {
            colors[i] = new VgaColor(
                data[0],
                data[1],
                data[2]);

            data = data.Slice(3);
        }

        return colors;
    }
}

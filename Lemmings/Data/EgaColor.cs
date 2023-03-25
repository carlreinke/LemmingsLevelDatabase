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

internal readonly struct EgaColor
{
    public readonly byte Rgb;

    public EgaColor(byte rgb)
    {
        Rgb = rgb;
    }

    public byte R => (byte)(((Rgb & 0b100_000) >> 2 | Rgb & 0b000_100) >> 2);

    public byte G => (byte)(((Rgb & 0b010_000) >> 2 | Rgb & 0b000_010) >> 1);

    public byte B => (byte)((Rgb & 0b001_000) >> 2 | Rgb & 0b000_001);

    /// <exception cref="ArgumentException">The length of <paramref name="data"/> is not 8.
    ///     </exception>
    public static EgaColor[] Read8ColorsFrom(ReadOnlySpan<byte> data)
    {
        if (data.Length != 8)
            throw new ArgumentException("Invalid length.", nameof(data));

        var colors = new EgaColor[8];

        for (int i = 0; i < colors.Length; ++i)
            colors[i] = new EgaColor(data[i]);

        return colors;
    }
}

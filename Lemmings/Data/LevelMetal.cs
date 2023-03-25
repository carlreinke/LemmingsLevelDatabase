// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using System;
using System.Buffers.Binary;

namespace Lemmings.Data;

internal readonly struct LevelMetal
{
    public readonly ushort XPlus4AndY;
    public readonly byte WidthMinus1AndHeightMinus1;
    public readonly byte Unused;

    public LevelMetal(
        ushort xAndY,
        byte widthMinus1AndHeightMinus1,
        byte unused)
    {
        XPlus4AndY = xAndY;
        WidthMinus1AndHeightMinus1 = widthMinus1AndHeightMinus1;
        Unused = unused;
    }

    public short X => (short)((XPlus4AndY >> 7) - 4);

    public byte Y => (byte)(XPlus4AndY & 0x7F);

    public byte Width => (byte)((WidthMinus1AndHeightMinus1 >> 4) + 1);

    public byte Height => (byte)((WidthMinus1AndHeightMinus1 & 0x0F) + 1);

    /// <exception cref="ArgumentException">The length of <paramref name="data"/> is not 4.
    ///     </exception>
    public static LevelMetal ReadFrom(ReadOnlySpan<byte> data)
    {
        if (data.Length != 4)
            throw new ArgumentException("Invalid length.", nameof(data));

        return new LevelMetal(
            BinaryPrimitives.ReadUInt16BigEndian(data),
            data[2],
            data[3]);
    }
}

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

internal readonly struct LevelObject
{
    public readonly short XPlus16;
    public readonly short Y;
    public readonly ushort Id;
    public readonly ObjectModifiers Modifiers;

    public LevelObject(
        short xPlus16,
        short y,
        ushort id,
        ObjectModifiers modifiers)
    {
        XPlus16 = xPlus16;
        Y = y;
        Id = id;
        Modifiers = modifiers;
    }

    public bool Unused => XPlus16 == 0;

    public short X => (short)(XPlus16 - 16);

    /// <exception cref="ArgumentException">The length of <paramref name="data"/> is not 8.
    ///     </exception>
    public static LevelObject ReadFrom(ReadOnlySpan<byte> data)
    {
        if (data.Length != 8)
            throw new ArgumentException("Invalid length.", nameof(data));

        return new LevelObject(
            BinaryPrimitives.ReadInt16BigEndian(data),
            BinaryPrimitives.ReadInt16BigEndian(data.Slice(2)),
            BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4)),
            (ObjectModifiers)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6)));
    }
}

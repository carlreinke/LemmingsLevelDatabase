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

internal readonly struct LevelPiece
{
    public readonly ushort ModifiersAndXPlus16;
    public readonly ushort YPlus4AndId;

    public LevelPiece(
        ushort modifiersAndX,
        ushort yAndId)
    {
        ModifiersAndXPlus16 = modifiersAndX;
        YPlus4AndId = yAndId;
    }

    public bool IsUsed => ModifiersAndXPlus16 != 0xFFFF;

    public PieceModifiers Modifiers => (PieceModifiers)(ModifiersAndXPlus16 & 0xF000);

    public short X => (short)((ModifiersAndXPlus16 & 0x0FFF) - 16);

    public short Y => (short)(((short)YPlus4AndId >> 7) - 4);

    public byte Id => (byte)(YPlus4AndId & 0x7F);

    /// <exception cref="ArgumentException">The length of <paramref name="data"/> is not 4.
    ///     </exception>
    public static LevelPiece ReadFrom(ReadOnlySpan<byte> data)
    {
        if (data.Length != 4)
            throw new ArgumentException("Invalid length.", nameof(data));

        return new LevelPiece(
            BinaryPrimitives.ReadUInt16BigEndian(data),
            BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2)));
    }
}

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

internal readonly struct PieceInfo
{
    /// <summary>
    /// The width of the image.
    /// </summary>
    public readonly byte Width;

    /// <summary>
    /// The height of the image.
    /// </summary>
    public readonly byte Height;

    /// <summary>
    /// The offset in the VGAGRx.DAT file of the image.
    /// </summary>
    public readonly ushort ImageOffset;

    /// <summary>
    /// The offset in the VGAGRx.DAT file of the mask.
    /// </summary>
    public readonly ushort MaskOffset;

    public readonly ushort Unknown6;

    public PieceInfo(
        byte width,
        byte height,
        ushort imageOffset,
        ushort maskOffset,
        ushort unknown6)
    {
        Width = width;
        Height = height;
        ImageOffset = imageOffset;
        MaskOffset = maskOffset;
        Unknown6 = unknown6;
    }

    /// <exception cref="ArgumentException">The length of <paramref name="data"/> is not 8.
    ///     </exception>
    public static PieceInfo ReadFrom(Span<byte> data)
    {
        if (data.Length != 8)
            throw new ArgumentException("Invalid length.", nameof(data));

        return new PieceInfo(
            data[0],
            data[1],
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(2)),
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(4)),
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(6)));
    }
}

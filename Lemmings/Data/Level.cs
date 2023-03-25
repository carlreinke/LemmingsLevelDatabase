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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Lemmings.Data;

internal sealed class Level
{
    public readonly ushort ReleaseRate;
    public readonly ushort ReleaseCount;
    public readonly ushort ToSaveCount;
    public readonly ushort TimeMinutes;
    public readonly ushort ClimberCount;
    public readonly ushort FloaterCount;
    public readonly ushort BomberCount;
    public readonly ushort BlockerCount;
    public readonly ushort BuilderCount;
    public readonly ushort BasherCount;
    public readonly ushort MinerCount;
    public readonly ushort DiggerCount;
    public readonly ushort InitialX;
    public readonly ushort GroundId;
    public readonly ushort SpecialId;
    public readonly LevelMode Mode;
    public readonly LevelObject[] Objects;
    public readonly LevelPiece[] Pieces;  // Pieces are ignored for special levels.
    public readonly LevelMetal[] Metals;
    public readonly byte[] Name;

    public Level(
        ushort releaseRate,
        ushort releaseCount,
        ushort toSaveCount,
        ushort timeMinutes,
        ushort climberCount,
        ushort floaterCount,
        ushort bomberCount,
        ushort blockerCount,
        ushort builderCount,
        ushort basherCount,
        ushort minerCount,
        ushort diggerCount,
        ushort initialX,
        ushort groundId,
        ushort specialId,
        LevelMode mode,
        LevelObject[] objects,
        LevelPiece[] pieces,
        LevelMetal[] metals,
        byte[] name)
    {
        ReleaseRate = releaseRate;
        ReleaseCount = releaseCount;
        ToSaveCount = toSaveCount;
        TimeMinutes = timeMinutes;
        ClimberCount = climberCount;
        FloaterCount = floaterCount;
        BomberCount = bomberCount;
        BlockerCount = blockerCount;
        BuilderCount = builderCount;
        BasherCount = basherCount;
        MinerCount = minerCount;
        DiggerCount = diggerCount;
        InitialX = initialX;
        GroundId = groundId;
        SpecialId = specialId;
        Mode = mode;
        Objects = objects;
        Pieces = pieces;
        Metals = metals;
        Name = name;
    }

    /// <exception cref="InvalidDataException"/>
    public static Level[] ReadLevelsFrom(Stream stream)
    {
        using (var reader = new DatFileReader(stream, leaveOpen: true))
        {
            var levels = new List<Level>();

            byte[] data = new byte[2048];

            while (true)
            {
                uint dataLength = reader.GetDecompressedLength();
                if (dataLength != 2048)
                    throw new InvalidDataException();
                reader.Decompress(data);

                levels.Add(ReadFrom(data));
            }
        }
    }

    /// <exception cref="ArgumentException">The length of <paramref name="data"/> is not 2048.
    ///     </exception>
    public static Level ReadFrom(ReadOnlySpan<byte> data)
    {
        if (data.Length != 2048)
            throw new ArgumentException("Invalid length.", nameof(data));

        ushort releaseRate = BinaryPrimitives.ReadUInt16BigEndian(data);
        ushort releaseCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2));
        ushort toSaveCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4));
        ushort timeMinutes = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6));
        ushort climberCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(8));
        ushort floaterCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(10));
        ushort bomberCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(12));
        ushort blockerCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(14));
        ushort builderCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(16));
        ushort basherCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(18));
        ushort minerCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(20));
        ushort diggerCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(22));

        ushort initialX = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(24));
        ushort groundId = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(26));
        ushort specialId = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(28));
        var mode = (LevelMode)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(30));

        data = data.Slice(32);

        var objects = new LevelObject[32];
        for (int i = 0; i < objects.Length; ++i)
            objects[i] = LevelObject.ReadFrom(data.Slice(i * 8, 8));

        data = data.Slice(objects.Length * 8);

        var pieces = new LevelPiece[400];
        for (int i = 0; i < pieces.Length; ++i)
            pieces[i] = LevelPiece.ReadFrom(data.Slice(i * 4, 4));

        data = data.Slice(pieces.Length * 4);

        var metals = new LevelMetal[32];
        for (int i = 0; i < metals.Length; ++i)
            metals[i] = LevelMetal.ReadFrom(data.Slice(i * 4, 4));

        data = data.Slice(metals.Length * 4);

        byte[] name = data.Slice(0, 32).ToArray();

        Debug.Assert(data.Length == 32);

        return new Level(
            releaseRate,
            releaseCount,
            toSaveCount,
            timeMinutes,
            climberCount,
            floaterCount,
            bomberCount,
            blockerCount,
            builderCount,
            basherCount,
            minerCount,
            diggerCount,
            initialX,
            groundId,
            specialId,
            mode,
            objects,
            pieces,
            metals,
            name);
    }
}

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
using System.Diagnostics;

namespace Lemmings.Data;

internal readonly struct ObjectInfo
{
    public readonly ushort UnknownAndAnimation;

    /// <summary>
    /// The index of the initial frame of the animation.  This only affects the first time that the
    /// object animates.
    /// </summary>
    public readonly byte AnimationInitialFrameIndex;

    /// <summary>
    /// The number of frames in the animation.
    /// </summary>
    public readonly byte AnimationFrameCount;

    /// <summary>
    /// The width of the image.
    /// </summary>
    public readonly byte Width;

    /// <summary>
    /// The height of the image.
    /// </summary>
    public readonly byte Height;

    /// <summary>
    /// The offset of each animation frame from the previous frame.
    /// </summary>
    public readonly ushort AnimationFrameLength;

    /// <summary>
    /// The offset of a mask from an image.
    /// </summary>
    public readonly ushort MaskOffset;

    public readonly ushort Unknown10;

    public readonly ushort Unknown12;

    /// <summary>
    /// The offset, in 4-pixel increments, of the effect area from the left edge of the object
    /// rounded down to a multiple of 4.
    /// </summary>
    public readonly ushort EffectLeft;

    /// <summary>
    /// The offset + 1, in 4-pixel increments, of the effect area from the top edge of the object
    /// rounded down to a multiple of 4.
    /// </summary>
    public readonly ushort EffectTopPlus1;

    /// <summary>
    /// The width, in 4-pixel increments, of the effect area.  0 is 256.
    /// </summary>
    public readonly byte EffectWidth;

    /// <summary>
    /// The height, in 4-pixel increments, of the effect area.  0 is 256.
    /// </summary>
    public readonly byte EffectHeight;

    /// <summary>
    /// The effect.
    /// </summary>
    public readonly Effect Effect;

    /// <summary>
    /// The offset in the VGAGRx.DAT file of the animation frames.
    /// </summary>
    public readonly ushort AnimationFramesOffset;

    /// <summary>
    /// The offset in the VGAGRx.DAT file of the image.
    /// </summary>
    public readonly ushort ImageOffset;

    public readonly ushort Unknown25;

    /// <summary>
    /// The sound effect that is played when the trap is triggered.
    /// </summary>
    public readonly byte TrapSoundEffect;

    public ObjectInfo(
        ushort unknownAndAnimation,
        byte animationInitialFrameIndex,
        byte animationFrameCount,
        byte width,
        byte height,
        ushort animationFrameLength,
        ushort maskOffset,
        ushort unknown10,
        ushort unknown12,
        ushort effectLeft,
        ushort effectTopPlus1,
        byte effectWidth,
        byte effectHeight,
        Effect effect,
        ushort animationFramesOffset,
        ushort imageOffset,
        ushort unknown25,
        byte trapSoundEffect)
    {
        Debug.Assert((ObjectAnimation)(unknownAndAnimation & 0x03) == ObjectAnimation.Never || animationFrameCount != 0);
        Debug.Assert(animationInitialFrameIndex < animationFrameCount || animationInitialFrameIndex == 0);
        Debug.Assert(Enum.IsDefined(effect));

        UnknownAndAnimation = unknownAndAnimation;
        AnimationInitialFrameIndex = animationInitialFrameIndex;
        AnimationFrameCount = animationFrameCount;
        Width = width;
        Height = height;
        AnimationFrameLength = animationFrameLength;
        MaskOffset = maskOffset;
        Unknown10 = unknown10;
        Unknown12 = unknown12;
        EffectLeft = effectLeft;
        EffectTopPlus1 = effectTopPlus1;
        EffectWidth = effectWidth;
        EffectHeight = effectHeight;
        Effect = effect;
        AnimationFramesOffset = animationFramesOffset;
        ImageOffset = imageOffset;
        Unknown25 = unknown25;
        TrapSoundEffect = trapSoundEffect;
    }

    public ObjectAnimation Animation => (ObjectAnimation)(UnknownAndAnimation & 0x03);

    public int EffectTop => EffectTopPlus1 - 1;

    /// <exception cref="ArgumentException">The length of <paramref name="data"/> is not 28.
    ///     </exception>
    public static ObjectInfo ReadFrom(Span<byte> data)
    {
        if (data.Length != 28)
            throw new ArgumentException("Invalid length.", nameof(data));

        return new ObjectInfo(
            BinaryPrimitives.ReadUInt16BigEndian(data),
            data[2],
            data[3],
            data[4],
            data[5],
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(6)),
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(8)),
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(10)),
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(12)),
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(14)),
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(16)),
            data[18],
            data[19],
            (Effect)data[20],
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(21)),
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(23)),
            BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(25)),
            data[27]);
    }

}

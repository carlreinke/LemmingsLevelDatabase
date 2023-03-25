// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using System.Diagnostics;

namespace Lemmings.Data;

internal readonly struct VgaObjectInfo
{
    /// <summary>
    /// The image that is displayed when the trap is not animated, such as in the level preview and
    /// when animation has not started.  The image is <see cref="ObjectInfo.Height"/> rows of
    /// <see cref="ObjectInfo.Width"/> pixels.  Each pixel contains the color in the lower nibble
    /// and the mask in the most-significant bit.
    /// </summary>
    public readonly byte[] Image;

    /// <summary>
    /// The frames of the object animation.  Each image is <see cref="ObjectInfo.Height"/> rows of
    /// <see cref="ObjectInfo.Width"/> pixels.  Each pixel contains the color in the lower nibble
    /// and the mask in the most-significant bit.
    /// </summary>
    public readonly byte[][] Frames;

    public readonly ObjectInfo Info;

    public VgaObjectInfo(ObjectInfo info, byte[] image, byte[][] frames)
    {
        Debug.Assert(image.Length == info.Width * info.Height);
        Debug.Assert(frames.Length == info.AnimationFrameCount);
        foreach (byte[] frame in frames)
            Debug.Assert(frame.Length == info.Width * info.Height);

        Info = info;
        Image = image;
        Frames = frames;
    }
}

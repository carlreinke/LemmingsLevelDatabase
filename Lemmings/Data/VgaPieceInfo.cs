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

internal readonly struct VgaPieceInfo
{
    /// <summary>
    /// The image, which is <see cref="PieceInfo.Height"/> rows of <see cref="PieceInfo.Width"/>
    /// pixels.  Each pixel contains the color in the lower nibble and the mask in the
    /// most-significant bit.
    /// </summary>
    public readonly byte[] Image;

    public readonly PieceInfo Info;

    public VgaPieceInfo(PieceInfo info, byte[] image)
    {
        Debug.Assert(image.Length == info.Width * info.Height);

        Info = info;
        Image = image;
    }
}

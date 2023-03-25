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

[Flags]
internal enum PieceModifiers : ushort
{
    /// <summary>
    /// The piece is unmodified.
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// The piece erases previously-drawn pieces.  This flag has no effect if the
    /// <see cref="Behind"/> flag is present.
    /// </summary>
    Erase = 0x2000,

    /// <summary>
    /// The piece is flipped vertically.
    /// </summary>
    Flip = 0x4000,

    /// <summary>
    /// The piece is drawn where it does not overdraw pieces.
    /// </summary>
    Behind = 0x8000,
}

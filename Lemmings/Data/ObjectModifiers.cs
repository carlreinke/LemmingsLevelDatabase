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
internal enum ObjectModifiers : ushort
{
    /// <summary>
    /// The object is unmodified.
    /// </summary>
    None = 0x0000,

    // TODO: Figure out least-significant nibble.

    /// <summary>
    /// The object is flipped vertically.
    /// </summary>
    Flip = 0x0080,

    /// <summary>
    /// The object is drawn in red and yellow where it overdraws pieces.
    /// </summary>
    Mask = 0x4000,

    /// <summary>
    /// The object is drawn where it does not overdraw pieces.  This flag has no effect if the
    /// <see cref="Mask"/> flag is present.
    /// </summary>
    Behind = 0x8000,
}

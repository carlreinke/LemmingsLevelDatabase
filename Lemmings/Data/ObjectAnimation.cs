// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

namespace Lemmings.Data;

internal enum ObjectAnimation : byte
{
    /// <summary>
    /// The object is not animated.
    /// </summary>
    Never = 0,

    /// <summary>
    /// The object animation starts when the trap is triggered and stops on frame 0.
    /// </summary>
    TrapTriggered = 1,

    /// <summary>
    /// The object animation starts automatically and does not stop.
    /// </summary>
    /// <remarks>
    /// If the object is the entrance, the animation start is delayed.
    /// </remarks>
    Always = 2,

    /// <summary>
    /// The object animation starts automatically and stops on frame 0.
    /// </summary>
    /// <remarks>
    /// If the object is the entrance, the animation start is delayed.
    /// </remarks>
    Once = 3,
}

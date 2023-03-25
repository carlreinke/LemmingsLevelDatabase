// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

namespace Lemmings.Data;

internal enum Effect : byte
{
    None = 0,
    Exit = 1,
    TurnAround = 2,
    Trap = 4,
    Drown = 5,
    Disintegrate = 6,
    OneWayLeft = 7,
    OneWayRight = 8,
    Indestructible = 9,
}

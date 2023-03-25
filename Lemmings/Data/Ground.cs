// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using System;
using System.Diagnostics;
using System.IO;

namespace Lemmings.Data;

internal sealed class Ground
{
    /// <summary>
    /// Information about the objects.
    /// </summary>
    public readonly ObjectInfo[] ObjectInfos;

    /// <summary>
    /// Information about the pieces.
    /// </summary>
    public readonly PieceInfo[] PieceInfos;

    /// <summary>
    /// EGA colors 8 to 15 used in-game.
    /// </summary>
    public readonly EgaColor[] EgaCustomColors;

    /// <summary>
    /// EGA colors 0 to 7.
    /// </summary>
    public readonly EgaColor[] EgaStandardColors;

    /// <summary>
    /// EGA colors 8 to 15 used in the level preview.
    /// </summary>
    public readonly EgaColor[] EgaPreviewColors;

    /// <summary>
    /// VGA colors 8 to 15 used in-game.
    /// </summary>
    public readonly VgaColor[] VgaCustomColors;

    /// <summary>
    /// VGA colors 0 to 7.
    /// </summary>
    public readonly VgaColor[] VgaStandardColors;

    /// <summary>
    /// VGA colors 8 to 15 used in the level preview.
    /// </summary>
    public readonly VgaColor[] VgaPreviewColors;

    public Ground(
        ObjectInfo[] objectInfos,
        PieceInfo[] pieceInfos,
        EgaColor[] egaCustomColors,
        EgaColor[] egaStandardColors,
        EgaColor[] egaPreviewColors,
        VgaColor[] vgaCustomColors,
        VgaColor[] vgaStandardColors,
        VgaColor[] vgaPreviewColors)
    {
        ObjectInfos = objectInfos;
        PieceInfos = pieceInfos;
        EgaCustomColors = egaCustomColors;
        EgaStandardColors = egaStandardColors;
        EgaPreviewColors = egaPreviewColors;
        VgaCustomColors = vgaCustomColors;
        VgaStandardColors = vgaStandardColors;
        VgaPreviewColors = vgaPreviewColors;
    }

    /// <exception cref="ArgumentException"><paramref name="stream"/> does not support reading.
    ///     </exception>
    /// <exception cref="EndOfStreamException">The end of the stream was reached before the expected
    ///     amount of data could be read.</exception>
    /// <exception cref="IOException">An I/O error occurs while reading from the stream.</exception>
    // ExceptionAdjustment: P:System.IO.Stream.Position get -T:System.NotSupportedException
    // ExceptionAdjustment: P:System.IO.Stream.Length get -T:System.NotSupportedException
    // ExceptionAdjustment: M:Lemmings.Data.Ground.Read(System.IO.Stream,System.Span{System.Byte}) -T:System.NotSupportedException
    public static Ground ReadFrom(Stream stream)
    {
        if (!stream.CanRead)
            throw new ArgumentException("Stream must support reading.", nameof(stream));

        Span<byte> wholeBuffer = stackalloc byte[28];

        var buffer = wholeBuffer;

        var objectInfos = new ObjectInfo[16];
        for (int i = 0; i < objectInfos.Length; ++i)
        {
            Read(stream, buffer);
            objectInfos[i] = ObjectInfo.ReadFrom(buffer);
        }

        buffer = wholeBuffer.Slice(0, 8);

        var pieceInfos = new PieceInfo[64];
        for (int i = 0; i < pieceInfos.Length; ++i)
        {
            Read(stream, buffer);
            pieceInfos[i] = PieceInfo.ReadFrom(buffer);
        }

        buffer = wholeBuffer.Slice(0, 8 * 1);

        Read(stream, buffer);
        var egaCustomColors = EgaColor.Read8ColorsFrom(buffer);

        Read(stream, buffer);
        var egaStandardColors = EgaColor.Read8ColorsFrom(buffer);

        Read(stream, buffer);
        var egaPreviewColors = EgaColor.Read8ColorsFrom(buffer);

        buffer = wholeBuffer.Slice(0, 8 * 3);

        Read(stream, buffer);
        var vgaCustomColors = VgaColor.Read8ColorsFrom(buffer);

        Read(stream, buffer);
        var vgaStandardColors = VgaColor.Read8ColorsFrom(buffer);

        Read(stream, buffer);
        var vgaPreviewColors = VgaColor.Read8ColorsFrom(buffer);

        Debug.Assert(!stream.CanSeek || stream.Position == stream.Length);

        return new Ground(
            objectInfos,
            pieceInfos,
            egaCustomColors,
            egaStandardColors,
            egaPreviewColors,
            vgaCustomColors,
            vgaStandardColors,
            vgaPreviewColors);
    }

    /// <exception cref="IOException"/>
    /// <exception cref="EndOfStreamException"/>
    /// <exception cref="NotSupportedException"/>
    private static void Read(Stream stream, Span<byte> buffer)
    {
        while (buffer.Length > 0)
        {
            int read = stream.Read(buffer);
            if (read == 0)
                throw new EndOfStreamException();

            buffer = buffer.Slice(read);
        }
    }
}

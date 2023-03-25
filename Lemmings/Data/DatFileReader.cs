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
using System.IO;

namespace Lemmings.Data;

internal sealed class DatFileReader : IDisposable
{
    private readonly Stream _stream;

    private readonly bool _leaveOpen;

    private readonly byte[] _buffer = new byte[4096];

    private long _nextHeaderPosition;

    private byte _initialBitCount;
    private byte _checksum;
    private uint _decompressedLength;
    private uint _compressedLength;

    private long _position;

    private bool _disposed;

    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.
    ///     </exception>
    /// <exception cref="ArgumentException"><paramref name="stream"/> does not support reading.
    ///     </exception>
    /// <exception cref="ArgumentException"><paramref name="stream"/> does not support seeking.
    ///     </exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    // ExceptionAdjustment: P:System.IO.Stream.Position -T:System.NotSupportedException
    public DatFileReader(Stream stream, bool leaveOpen = false)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead)
            throw new ArgumentException("Stream must support reading.", nameof(stream));
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must support seeking.", nameof(stream));

        _stream = stream;
        _leaveOpen = leaveOpen;

        _nextHeaderPosition = _stream.Position;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (!_leaveOpen)
            _stream.Dispose();

        _disposed = true;
    }

    /// <exception cref="InvalidDataException">The header is invalid.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="ObjectDisposedException">The object is disposed.</exception>
    // ExceptionAdjustment: P:System.IO.Stream.Position -T:System.NotSupportedException
    // ExceptionAdjustment: M:Lemmings.Data.DatFileReader.Read(System.IO.Stream,System.Span{System.Byte}) -T:System.NotSupportedException
    public uint GetDecompressedLength()
    {
        ThrowIfDisposed();

        _stream.Position = _nextHeaderPosition;

        Span<byte> buffer = stackalloc byte[10];

        Read(_stream, buffer);

        _initialBitCount = buffer[0];
        _checksum = buffer[1];
        _decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(2));
        _compressedLength = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(6));

        if (_initialBitCount > 8 || _compressedLength < 10)
            throw new InvalidDataException("Invalid header.");

        _position = _stream.Position;

        _nextHeaderPosition = _position + _compressedLength - 10;

        return _decompressedLength;
    }

    /// <exception cref="ArgumentException">The length of <paramref name="destination"/> does not
    ///     match the length returned from <see cref="GetDecompressedLength"/>.</exception>
    /// <exception cref="InvalidDataException">The compressed data is invalid.</exception>
    /// <exception cref="EndOfStreamException">The end of the stream was reached before the expected
    ///     amount of data could be read.</exception>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="ObjectDisposedException">The object is disposed.</exception>
    // ExceptionAdjustment: P:System.IO.Stream.Position -T:System.NotSupportedException
    public void Decompress(Span<byte> destination)
    {
        ThrowIfDisposed();

        if (_decompressedLength != (uint)destination.Length)
            throw new ArgumentException("Mismatched destination size.", nameof(destination));

        byte checksum = _checksum;
        uint di = _decompressedLength;
        uint si = _compressedLength - 10;

        int bi = 0;

        uint bits = 0;
        int bitCount = 0;

        FillBits();

        if (bitCount < _initialBitCount)
            throw new InvalidDataException("Invalid compressed data.");

        // Discard upper bits of first byte.
        bits = bits >> 8 << _initialBitCount | bits & 0xFFU >> 8 - _initialBitCount;
        bitCount -= 8 - _initialBitCount;

        while (di > 0)
        {
            if (bitCount < 3 + 8 + 12)
                FillBits();

            uint length;
            uint offset;

            switch (bits & 0b111)
            {
                case 0b0_00:
                case 0b1_00:
                {
                    length = Get3ReversedBits(bits >> 2) + 1;

                    bits >>= 2 + 3;
                    bitCount -= 2 + 3;

                    goto WriteLiterals;
                }
                case 0b0_10:
                case 0b1_10:
                {
                    length = 2;
                    offset = Get8ReversedBits(bits >> 2) + 1;

                    bits >>= 2 + 8;
                    bitCount -= 2 + 8;

                    goto WriteCopy;
                }
                case 0b001:
                {
                    length = 3;
                    offset = Get9ReversedBits(bits >> 3) + 1;

                    bits >>= 3 + 9;
                    bitCount -= 3 + 9;

                    goto WriteCopy;
                }
                case 0b101:
                {
                    length = 4;
                    offset = Get10ReversedBits(bits >> 3) + 1;

                    bits >>= 3 + 10;
                    bitCount -= 3 + 10;

                    goto WriteCopy;
                }
                case 0b011:
                {
                    length = Get8ReversedBits(bits >> 3) + 1;
                    offset = Get12ReversedBits(bits >> 11) + 1;

                    bits >>= 3 + 8 + 12;
                    bitCount -= 3 + 8 + 12;

                    goto WriteCopy;
                }
                case 0b111:
                {
                    length = Get8ReversedBits(bits >> 3) + 9;

                    bits >>= 3 + 8;
                    bitCount -= 3 + 8;

                    goto WriteLiterals;
                }
                default:
                    throw new UnreachableException();
            }

        WriteLiterals:
            if (length > di)
                throw new InvalidDataException("Invalid compressed data.");

            do
            {
                if (bitCount < 8)
                    FillBits();

                di -= 1;
                destination[(int)di] = (byte)Get8ReversedBits(bits);

                bits >>= 8;
                bitCount -= 8;

                length -= 1;
            }
            while (length != 0);
            continue;

        WriteCopy:
            if (length > di)
                throw new InvalidDataException("Invalid compressed data.");

            if (offset > _decompressedLength - di)
                throw new InvalidDataException("Invalid compressed data.");

            do
            {
                di -= 1;
                destination[(int)di] = destination[(int)(di + offset)];

                length -= 1;
            }
            while (length != 0);
            continue;
        }

        if (si != 0 || bi != 0 || bitCount != 0 || checksum != 0)
            throw new InvalidDataException("Invalid compressed data.");

        // ExceptionAdjustment: P:System.IO.Stream.Position -T:System.NotSupportedException
        // ExceptionAdjustment: M:System.IO.Stream.Read(System.Byte[],System.Int32,System.Int32) -T:System.NotSupportedException
        void FillBits()
        {
            Debug.Assert((uint)bitCount < 24);

            do
            {
                if (bi == 0)
                {
                    if (si == 0)
                        if (bitCount <= 0)
                            throw new InvalidDataException("Invalid compressed data.");
                        else
                            return;

                    int length = (int)Math.Min(si, 4096);
                    si -= (uint)length;

                    _stream.Position = _position + si;
                    for (int i = 0; i < length;)
                    {
                        int read = _stream.Read(_buffer, i, length - i);
                        if (read == 0)
                            throw new EndOfStreamException();
                        i += read;
                    }

                    bi = length;
                }

                bi -= 1;
                byte b = _buffer[bi];
                bits |= (uint)(b << bitCount);
                checksum ^= b;
                bitCount += 8;
            }
            while (bitCount <= 24);
        }
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

    private static uint Get3ReversedBits(uint bits)
    {
        uint middle = bits & 0b010;
        bits = bits << 2 & 0b100 | bits >> 2 & 0b001;
        return bits | middle;
    }

    private static uint Get8ReversedBits(uint bits)
    {
        bits = bits << 4 & 0b11110000 | bits >> 4 & 0b00001111;
        bits = bits << 2 & 0b11001100 | bits >> 2 & 0b00110011;
        bits = bits << 1 & 0b10101010 | bits >> 1 & 0b01010101;
        return bits;
    }

    private static uint Get9ReversedBits(uint bits)
    {
        uint middle = bits & 0b000010000;
        bits = bits << 5 & 0b111100000 | bits >> 5 & 0b000001111;
        bits = bits << 2 & 0b110001100 | bits >> 2 & 0b001100011;
        bits = bits << 1 & 0b101001010 | bits >> 1 & 0b010100101;
        return bits | middle;
    }

    private static uint Get10ReversedBits(uint bits)
    {
        bits = bits << 5 & 0b1111100000 | bits >> 5 & 0b0000011111;
        uint middle = bits & 0b0010000100;
        bits = bits << 3 & 0b1100011000 | bits >> 3 & 0b0001100011;
        bits = bits << 1 & 0b1001010010 | bits >> 1 & 0b0100101001;
        return bits | middle;
    }

    private static uint Get12ReversedBits(uint bits)
    {
        bits = bits << 6 & 0b111111000000 | bits >> 6 & 0b000000111111;
        bits = bits << 3 & 0b111000111000 | bits >> 3 & 0b000111000111;
        uint middle = bits & 0b010010010010;
        bits = bits << 2 & 0b100100100100 | bits >> 2 & 0b001001001001;
        return bits | middle;
    }

    private static uint GetReversedBits(uint bits, int count)
    {
        uint result = 0;
        while (true)
        {
            result = result << 1 | bits & 1;
            bits >>= 1;
            count -= 1;
            if (count == 0)
                return result;
        }
    }

    /// <exception cref="ObjectDisposedException"/>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(typeof(DatFileReader).FullName);
    }
}

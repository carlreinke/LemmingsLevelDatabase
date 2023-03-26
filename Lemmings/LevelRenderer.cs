// Copyright 2023 Carl Reinke
//
// This file is part of a program that is licensed under the terms of the GNU
// Affero General Public License Version 3 as published by the Free Software
// Foundation.
//
// This license does not grant rights under trademark law for use of any trade
// names, trademarks, or service marks.

using Lemmings.Data;
using System;
using System.Diagnostics;

namespace Lemmings
{
    internal static class LevelRenderer
    {
        public static void RenderSpecial(VgaSpecial vgaSpecial, byte[] piecesImage)
        {
            Debug.Assert(piecesImage.Length == 1600 * 160);

            int dstOffset = 0 * 1600 + 304;

            for (int i = 0; i < 4; ++i)
            {
                byte[] srcImage = vgaSpecial.Images[i];

                int srcOffset = 0;

                for (byte y = 0; y < 40; ++y)
                {
                    for (int x = 0; x < 960; ++x)
                    {
                        byte src = srcImage[srcOffset + x];
                        byte dst = piecesImage[dstOffset + x];
                        int mask = (sbyte)src >> 7;
                        piecesImage[dstOffset + x] = (byte)(dst ^ ((dst ^ src) & mask));
                    }

                    srcOffset += 960;
                    dstOffset += 1600;
                }
            }
        }

        public static void RenderPieces(VgaGround vgaGround, Level level, byte[] piecesImage)
        {
            foreach (var levelPiece in level.Pieces)
            {
                // DOS version stops on first unused piece.
                if (!levelPiece.IsUsed)
                    break;

                RenderPiece(vgaGround, levelPiece, piecesImage);
            }
        }

        public static void RenderPiece(VgaGround vgaGround, in LevelPiece levelPiece, byte[] piecesImage)
        {
            Debug.Assert(levelPiece.IsUsed);
            Debug.Assert(piecesImage.Length == 1600 * 160);

            if (levelPiece.Id >= vgaGround.PieceInfos.Length)
                return;  // TODO: Is this the correct behavior?  Error?

            var vgaPieceInfo = vgaGround.PieceInfos[levelPiece.Id];

            byte width = vgaPieceInfo.Info.Width;
            byte height = vgaPieceInfo.Info.Height;
            byte[] srcImage = vgaPieceInfo.Image;

            int srcOffset;
            int srcStep;
            if (levelPiece.Modifiers.HasFlag(PieceModifiers.Flip))
            {
                srcOffset = srcImage.Length - width;
                srcStep = -width;
            }
            else
            {
                srcOffset = 0;
                srcStep = width;
            }

            int dstOffset = levelPiece.Y * 1600 + levelPiece.X;

            if (levelPiece.Modifiers.HasFlag(PieceModifiers.Behind))
            {
                for (byte y = 0; y < height; ++y)
                {
                    if ((uint)(levelPiece.Y + y) < 160)
                    {
                        for (byte x = 0; x < width; ++x)
                        {
                            if ((uint)(levelPiece.X + x) < 1600)
                            {
                                byte src = srcImage[srcOffset + x];
                                byte dst = piecesImage[dstOffset + x];
                                int mask = (sbyte)(src & ~dst) >> 7;
                                piecesImage[dstOffset + x] = (byte)(dst ^ ((dst ^ src) & mask));
                            }
                        }
                    }

                    srcOffset += srcStep;
                    dstOffset += 1600;
                }
            }
            else if (levelPiece.Modifiers.HasFlag(PieceModifiers.Erase))
            {
                for (byte y = 0; y < height; ++y)
                {
                    if ((uint)(levelPiece.Y + y) < 160)
                    {
                        for (byte x = 0; x < width; ++x)
                        {
                            if ((uint)(levelPiece.X + x) < 1600)
                            {
                                byte src = srcImage[srcOffset + x];
                                int mask = ~((sbyte)src >> 7);
                                piecesImage[dstOffset + x] &= (byte)mask;
                            }
                        }
                    }

                    srcOffset += srcStep;
                    dstOffset += 1600;
                }
            }
            else
            {
                for (byte y = 0; y < height; ++y)
                {
                    if ((uint)(levelPiece.Y + y) < 160)
                    {
                        for (byte x = 0; x < width; ++x)
                        {
                            if ((uint)(levelPiece.X + x) < 1600)
                            {
                                byte src = srcImage[srcOffset + x];
                                byte dst = piecesImage[dstOffset + x];
                                int mask = (sbyte)src >> 7;
                                piecesImage[dstOffset + x] = (byte)(dst ^ ((dst ^ src) & mask));
                            }
                        }
                    }

                    srcOffset += srcStep;
                    dstOffset += 1600;
                }
            }
        }

        public static void RenderObjects(VgaGround vgaGround, Level level, byte[] piecesImage, byte[] objectsImage)
        {
            foreach (var levelObject in level.Objects)
                if (levelObject.IsUsed)
                    RenderObject(vgaGround, levelObject, piecesImage, objectsImage);
        }

        public static void RenderObject(VgaGround vgaGround, in LevelObject levelObject, byte[] piecesImage, byte[] objectsImage)
        {
            if (levelObject.Id >= vgaGround.ObjectInfos.Length)
                return;  // TODO: Is this the correct behavior?  Error?

            var objectInfo = vgaGround.ObjectInfos[levelObject.Id];

            byte[] objectImage = objectInfo.Image;

            RenderObject(objectInfo.Info, objectImage, levelObject, piecesImage, objectsImage);
        }

        public static void RenderObject(VgaGround vgaGround, in LevelObject levelObject, ushort frameId, byte[] piecesImage, byte[] objectsImage)
        {
            if (levelObject.Id >= vgaGround.ObjectInfos.Length)
                return;  // TODO: Is this the correct behavior?  Error?

            var objectInfo = vgaGround.ObjectInfos[levelObject.Id];

            byte[] objectImage = objectInfo.Frames[frameId];

            RenderObject(objectInfo.Info, objectImage, levelObject, piecesImage, objectsImage);
        }

        private static void RenderObject(in ObjectInfo objectInfo, byte[] srcImage, in LevelObject levelObject, byte[] piecesImage, byte[] objectsImage)
        {
            Debug.Assert(levelObject.IsUsed);
            Debug.Assert(piecesImage.Length == 1600 * 160);
            Debug.Assert(objectsImage.Length == 1600 * 160);

            byte height = objectInfo.Height;
            byte width = objectInfo.Width;

            int srcOffset;
            int srcStep;
            if (levelObject.Modifiers.HasFlag(ObjectModifiers.Flip))
            {
                srcOffset = srcImage.Length - width;
                srcStep = -width;
            }
            else
            {
                srcOffset = 0;
                srcStep = width;
            }

            short blitX = (short)(levelObject.X & ~0x07);

            int dstOffset = levelObject.Y * 1600 + blitX;

            if (levelObject.Modifiers.HasFlag(ObjectModifiers.Mask))
            {
                for (byte y = 0; y < height; ++y)
                {
                    if ((uint)(levelObject.Y + y) < 160)
                    {
                        for (byte x = 0; x < width; ++x)
                        {
                            if ((uint)(blitX + x) < 1600)
                            {
                                byte src = srcImage[srcOffset + x];
                                byte sel = piecesImage[dstOffset + x];
                                byte dst = objectsImage[dstOffset + x];
                                int mask = (sbyte)(src & sel & ~dst) >> 7;
                                int src2 = (src & 0x81) | 0x04;  // TODO: Is this the correct mapping to red and yellow?
                                objectsImage[dstOffset + x] = (byte)(dst ^ ((dst ^ src2) & mask));
                            }
                        }
                    }

                    srcOffset += srcStep;
                    dstOffset += 1600;
                }
            }
            else if (levelObject.Modifiers.HasFlag(ObjectModifiers.Behind))
            {
                for (byte y = 0; y < height; ++y)
                {
                    if ((uint)(levelObject.Y + y) < 160)
                    {
                        for (byte x = 0; x < width; ++x)
                        {
                            if ((uint)(blitX + x) < 1600)
                            {
                                byte src = srcImage[srcOffset + x];
                                byte sel = piecesImage[dstOffset + x];
                                byte dst = objectsImage[dstOffset + x];
                                int mask = (sbyte)(src & (~sel | dst)) >> 7;
                                objectsImage[dstOffset + x] = (byte)(dst | (src & mask));
                            }
                        }
                    }

                    srcOffset += srcStep;
                    dstOffset += 1600;
                }
            }
            else
            {
                for (byte y = 0; y < height; ++y)
                {
                    if ((uint)(levelObject.Y + y) < 160)
                    {
                        for (byte x = 0; x < width; ++x)
                        {
                            if ((uint)(blitX + x) < 1600)
                            {
                                byte src = srcImage[srcOffset + x];
                                byte dst = objectsImage[dstOffset + x];
                                int mask = (sbyte)src >> 7;
                                objectsImage[dstOffset + x] = (byte)(dst ^ ((dst ^ src) & mask));
                            }
                        }
                    }

                    srcOffset += srcStep;
                    dstOffset += 1600;
                }
            }
        }

        public static void RenderEffects(VgaGround vgaGround, Level level, Effect[] effectMap)
        {
            foreach (var levelMetal in level.Metals)
                RenderMetalEffect(levelMetal, effectMap);

            // TODO: Objects with index >= 16 have no effect?
            foreach (var levelObject in level.Objects)
                RenderObjectEffect(vgaGround, levelObject, effectMap);
        }

        public static void RenderEffectBoundaries(VgaGround vgaGround, Level level, byte[] image)
        {
            foreach (var levelMetal in level.Metals)
                RenderMetalEffectBoundary(levelMetal, image);

            // TODO: Objects with index >= 16 have no effect?
            foreach (var levelObject in level.Objects)
                if (levelObject.IsUsed)
                    RenderObjectEffectBoundary(vgaGround, levelObject, image);
        }

        public static void RenderMetalEffect(in LevelMetal levelMetal, Effect[] effectMap)
        {
            Debug.Assert(effectMap.Length == 400 * 40);

            // TODO
            throw new NotImplementedException();
        }

        public static void RenderMetalEffectBoundary(in LevelMetal levelMetal, byte[] image)
        {
            Debug.Assert(image.Length == 1600 * 160);

            int x1 = levelMetal.X * 4;
            int x2 = x1 + levelMetal.Width * 4 - 1;
            int y1 = levelMetal.Y * 4;
            int y2 = y1 + levelMetal.Height * 4 - 1;

            RenderEffectBoundary(image, x1, x2, y1, y2, Effect.Indestructible);
        }

        public static void RenderObjectEffect(VgaGround vgaGround, in LevelObject levelObject, Effect[] effectMap)
        {
            Debug.Assert(levelObject.IsUsed);
            Debug.Assert(effectMap.Length == 400 * 40);

            // TODO
            throw new NotImplementedException();
        }

        public static void RenderObjectEffectBoundary(VgaGround vgaGround, in LevelObject levelObject, byte[] image)
        {
            Debug.Assert(levelObject.IsUsed);
            Debug.Assert(image.Length == 1600 * 160);

            if (levelObject.Id >= vgaGround.ObjectInfos.Length)
                return;  // TODO: Is this the correct behavior?  Error?

            var objectInfo = vgaGround.ObjectInfos[levelObject.Id].Info;

            if (objectInfo.Effect == Effect.None || objectInfo.EffectWidth == 0 || objectInfo.EffectHeight == 0)
                return;

            int x1 = (levelObject.X + objectInfo.EffectLeft * 4) & ~0x03;
            int x2 = x1 + objectInfo.EffectWidth * 4 - 1;
            int y1 = (levelObject.Y + objectInfo.EffectTop * 4) & ~0x03;
            int y2 = y1 + objectInfo.EffectHeight * 4 - 1;

            RenderEffectBoundary(image, x1, x2, y1, y2, objectInfo.Effect);
        }

        private static void RenderEffectBoundary(byte[] image, int x1, int x2, int y1, int y2, Effect effect)
        {
            HLine(x1, x2, y1, (byte)effect, image);
            HLine(x1, x2, y2, (byte)effect, image);
            VLine(x1, y1, y2, (byte)effect, image);
            VLine(x2, y1, y2, (byte)effect, image);

            static void HLine(int x1, int x2, int y, byte c, byte[] image)
            {
                if (x1 >= 1600 || x2 < 0 || (uint)y >= 160)
                    return;
                x1 = Math.Max(0, x1);
                x2 = Math.Min(x2, 1600 - 1);

                int dstOffset = y * 1600 + x1;

                for (; (uint)x1 <= (uint)x2; ++x1)
                {
                    int t = ((x1 + y) << 2) & ((x1 + y) << 1) & 0x04;
                    image[dstOffset] = (byte)(0x80 | c ^ t);

                    dstOffset += 1;
                }
            }

            static void VLine(int x, int y1, int y2, byte c, byte[] image)
            {
                if ((uint)x >= 1600 || y1 >= 160 || y2 < 0)
                    return;
                y1 = Math.Max(0, y1);
                y2 = Math.Min(y2, 160 - 1);

                int dstOffset = y1 * 1600 + x;

                for (; (uint)y1 <= (uint)y2; ++y1)
                {
                    int t = ((x + y1) << 2) & ((x + y1) << 1) & 0x04;
                    image[dstOffset] = (byte)(0x80 | c ^ t);

                    dstOffset += 1600;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SpriteRotate
{
    class Program
    {
        private static readonly byte[] memdmp = File.ReadAllBytes("memdmp.bin");

        static void Main(string[] args)
        {
            Bitmap bmpTiles = new Bitmap(52 * 6 + 12, 28 * 10 + 12, PixelFormat.Format32bppArgb);

            FileStream fs = new FileStream("SPRITE.MAC", FileMode.Create);
            StreamWriter writer = new StreamWriter(fs);
            writer.WriteLine("; START OF SPRITE.MAC");
            writer.WriteLine();

            ProcessFont(writer);
            writer.WriteLine("\t.EVEN");
            writer.WriteLine();

            ProcessSmallFont(writer);
            writer.WriteLine("\t.EVEN");
            writer.WriteLine();

            ProcessData6500(writer);
            writer.WriteLine("\t.EVEN");
            writer.WriteLine();

            ProcessSprites6AD0(writer);
            writer.WriteLine("\t.EVEN");
            writer.WriteLine();

            ProcessSprites7100(writer);
            writer.WriteLine("\t.EVEN");
            writer.WriteLine();

            ProcessData7900(writer);
            writer.WriteLine("\t.EVEN");
            writer.WriteLine();

            ProcessMasksAndSprites(writer, bmpTiles);
            writer.WriteLine("\t.EVEN");
            writer.WriteLine();

            writer.WriteLine("; END OF SPRITE.MAC");

            writer.Flush();

            bmpTiles.Save("sprites.png");
        }

        static void ProcessFont(StreamWriter writer)
        {
            const string TileChars = "0123456789 :/.-?!ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            writer.Write("L5B00:");
            for (int tile = 0; tile < 43; tile++)
            {
                int addr = 0x5B00 + tile * 5;
                writer.Write("\t.BYTE\t");
                for (int i = 0; i < 5; i++)
                {
                    byte b = memdmp[addr + i];
                    int bb = 0;
                    for (int j = 0; j < 8; j++)
                        bb |= ((b >> (7 - j)) & 1) << j;

                    bb = bb >> 1;
                    writer.Write(EncodeOctalString((byte)bb));
                    if (i < 4)
                        writer.Write(",");
                    //
                }
                writer.WriteLine("\t; {0} {1}", EncodeOctalString((byte)tile), TileChars[tile]);
            }
        }

        static void ProcessSmallFont(StreamWriter writer)
        {
            writer.WriteLine("; Small Font");
            writer.Write("LB5D7:");
            int addr = 0xB5D7;
            for (int i = 0; i < 80; i++)
            {
                if ((i % 16) == 0)
                    writer.Write("\t.BYTE\t");

                byte b = memdmp[addr + i];
                int bb = 0;
                for (int j = 0; j < 8; j++)
                    bb |= ((b >> (7 - j)) & 1) << j;

                writer.Write(EncodeOctalString((byte)bb));
                if ((i % 16) < 15)
                    writer.Write(",");
                else
                    writer.WriteLine();
            }
        }

        static void ProcessData6500(StreamWriter writer)
        {
            writer.WriteLine("; Data");
            writer.Write("L6500:");
            int addr = 0x6500;
            for (int i = 0; i < 1488; i++)
            {
                if (i == 12) writer.Write("L650C:");
                if ((i % 12) == 0) writer.Write("\t.BYTE\t");

                byte b = memdmp[addr + i];
                writer.Write(EncodeOctalString(b));

                if ((i % 12) < 11 && i < 1488 - 1)
                    writer.Write(",");
                else
                    writer.WriteLine();
            }
        }

        static void ProcessSprites6AD0(StreamWriter writer)
        {
            writer.WriteLine("L6AD0::\t; Data");
            int addr = 0x6AD0;
            for (int sprite = 0; sprite < 99; sprite++)
            {
                writer.Write("\t.BYTE\t");
                for (int i = 0; i < 16; i++) // bytes
                {
                    byte b = memdmp[addr];
                    writer.Write(EncodeOctalString(b));

                    if (i < 15)
                        writer.Write(",");

                    addr++;
                }
                writer.WriteLine();
            }
        }

        static void ProcessSprites7100(StreamWriter writer)
        {
            writer.WriteLine("L7100::\t; Sprites");
            int addr = 0x7100;
            for (int sprite = 0; sprite < 128; sprite++) // sprites
            {
                writer.Write("\t.BYTE\t");
                for (int i = 0; i < 16; i++) // bytes
                {
                    byte b = memdmp[addr];
                    int bb = 0;
                    for (int j = 0; j < 8; j++)
                        bb |= ((b >> (7 - j)) & 1) << j;

                    writer.Write(EncodeOctalString((byte)bb));

                    if (i < 15)
                        writer.Write(",");

                    addr++;
                }
                writer.WriteLine();
            }
        }

        static void ProcessData7900(StreamWriter writer)
        {
            writer.WriteLine("; Data");
            writer.Write("L7900:");
            int addr = 0x7900;
            for (int i = 0; i < 5632; i++)
            {
                if ((i % 16) == 0)
                    writer.Write("\t.BYTE\t");

                byte b = memdmp[addr + i];
                //int bb = 0;
                //for (int j = 0; j < 8; j++)
                //    bb |= ((b >> (7 - j)) & 1) << j;

                writer.Write(EncodeOctalString((byte)b));
                if ((i % 16) < 15 && i < 5632 - 1)
                    writer.Write(",");
                else
                    writer.WriteLine();
            }
        }

        static void ProcessMasksAndSprites(StreamWriter writer, Bitmap bmpTiles)
        {
            writer.WriteLine("; Masks and Sprites, 57. sprites, 6 * 24 = 144 bytes each, 8208 bytes in total");
            for (int sprite = 0; sprite < 57; sprite++)  // sprites
            {
                int addr = 0xB8F0 + sprite * 6 * 24;
                int x = 8 + (sprite % 6) * 52;
                int y = 8 + (sprite / 6) * 28;

                writer.Write("L{0}:", EncodeHexString2(addr));

                for (int i = 0; i < 6 * 24; i++)  // bytes
                {
                    if ((i % 12) == 0)
                        writer.Write("\t.BYTE\t");

                    byte b = memdmp[addr + i];
                    for (int j = 0; j < 8; j++)  // bits
                    {
                        int bit = (b >> (7 - j)) & 1;
                        Color color = (bit == 1) ? Color.Black : Color.White;
                        bmpTiles.SetPixel(x + (i % 6) * 8 + j, y + (23 - i / 6), color);
                    }

                    int bb = 0;
                    for (int j = 0; j < 8; j++)
                        bb |= ((b >> (7 - j)) & 1) << j;
                    if ((i % 6) < 3) bb = (~bb) & 255;  // Inverse the mask
                    writer.Write(EncodeOctalString((byte)bb));

                    if ((i % 12) != 11)
                    {
                        writer.Write(",");
                        if ((i % 12) == 5) writer.Write(" ");
                    }
                    else
                    {
                        if (i == 11)
                            writer.Write(" ; {0}", sprite);
                        writer.WriteLine();
                    }
                }
            }
        }

        static string EncodeOctalString(byte value)
        {
            //convert to int, for cleaner syntax below. 
            int x = (int)value;

            return string.Format(
                @"{0}{1}{2}",
                ((x >> 6) & 7),
                ((x >> 3) & 7),
                (x & 7)
            );
        }

        static string EncodeOctalString2(int x)
        {
            return string.Format(
                @"{0}{1}{2}{3}{4}{5}",
                ((x >> 15) & 7),
                ((x >> 12) & 7),
                ((x >> 9) & 7),
                ((x >> 6) & 7),
                ((x >> 3) & 7),
                (x & 7)
            );
        }

        static string EncodeHexString2(int x)
        {
            return x.ToString("X4");
        }
    }
}

/* Create by: Muhammad Chishty Asheque
 * Date: Friday, April 02, 2010
 * Contact: twinkle_rip@hotmail.com 
 */

using System;
using System.Drawing;
using System.Dynamic;
using System.IO;

static class GrayBMP_File
{
    static byte[] BMP_File_Header = new byte[14];
    static byte[] DIB_header = new byte[40];
    static byte[] Color_palette = new byte[1024]; //a palette containing 256 colors
    static byte[] Bitmap_Data = null;
    private static byte GetIndex(Color color)
    {
        for (int x = 0; x < 256; x++)
        {
            if (color.R == Color_palette[x*4 + 2] &&
                color.G == Color_palette[x*4 + 1] &&
                color.B == Color_palette[x*4])
                return (byte) x;
        }

        return 0;
    }

    //creates byte array of 256 color grayscale palette
    static byte[] create_palette(bool trees=false, bool rivers=false)
    {
        byte[] color_palette = new byte[1024];
        int ii=0;
        if (rivers)
        {
            setColor(ii++, 0, 255, 0, color_palette);
            setColor(ii++, 255, 0, 0, color_palette);
            setColor(ii++, 255, 252, 0, color_palette);
            setColor(ii++, 0, 255, 255, color_palette);
            setColor(ii++, 0, 200, 255, color_palette);
            setColor(ii++, 0, 150, 255, color_palette);
            setColor(ii++, 0, 100, 255, color_palette);
            setColor(ii++, 0, 0, 255, color_palette);
            setColor(ii++, 0, 0, 225, color_palette);
            setColor(ii++, 0, 0, 200, color_palette);
            setColor(ii++, 0, 0, 150, color_palette);
            setColor(ii++, 0, 0, 100, color_palette);
            setColor(ii++, 0, 85, 0, color_palette);
            setColor(ii++, 0, 125, 0, color_palette);
            setColor(ii++, 0, 158, 0, color_palette);
            setColor(ii++, 24, 206, 0, color_palette);
            setColor(254, 255, 0, 128, color_palette);
            setColor(255, 255, 255, 255, color_palette);


        }
        else if (!trees)
        {
            setColor(ii++, 86, 124, 27, color_palette);
            setColor(ii++, 138, 11, 26, color_palette);
            setColor(ii++, 130, 158, 75, color_palette);
            setColor(ii++, 206, 169, 99, color_palette);
            setColor(ii++, 112, 74, 31, color_palette);
            setColor(ii++, 255, 186, 0, color_palette);
            setColor(ii++, 13, 96, 62, color_palette);
            setColor(ii++, 86, 46, 0, color_palette);
            setColor(ii++, 0, 86, 6, color_palette);
            setColor(ii++, 65, 42, 17, color_palette);
            setColor(ii++, 155, 155, 155, color_palette);
            setColor(ii++, 255, 255, 255, color_palette);
            setColor(ii++, 40, 180, 149, color_palette);
            setColor(ii++, 213, 144, 199, color_palette);
            setColor(ii++, 127, 27, 60, color_palette);
            setColor(ii++, 69, 91, 186, color_palette);

        }
        else
        {
            setColor(ii++, 0, 0, 0, color_palette);
            setColor(ii++, 255, 0, 0, color_palette);
            setColor(ii++, 30, 139, 109, color_palette);
            setColor(ii++, 18, 100, 78, color_palette);
            setColor(ii++, 8, 58, 44, color_palette);
            setColor(ii++, 76, 156, 51, color_palette);
            setColor(ii++, 47, 120, 24, color_palette);
            setColor(ii++, 20, 85, 0, color_palette);
            setColor(ii++, 154, 156, 51, color_palette);
            setColor(ii++, 118, 120, 24, color_palette);
            setColor(ii++, 83, 85, 0, color_palette);
            setColor(ii++, 255, 255, 0, color_palette);
            setColor(ii++, 213, 160, 0, color_palette);

        }

        if(!rivers)
        for (int i = 4*16; i < 256; i++)
        {
            color_palette[i * 4 + 0] = (byte)(i); //bule
            color_palette[i * 4 + 1] = (byte)(i); //green
            color_palette[i * 4 + 2] = (byte)(i); //red
            color_palette[i * 4 + 3] = (byte)0; //padding
        }
        return color_palette;
    }

    private static void setColor(int i, int r, int g, int b, byte[] colorPalette)
    {

        colorPalette[i * 4 + 0] = (byte)b;
        colorPalette[i * 4 + 1] = (byte)g;
        colorPalette[i * 4 + 2] = (byte)r;
        colorPalette[i * 4 + 3] = (byte)0;
    }

    //create different part of a bitmap file
    static void create_parts(Image img, bool trees = false, bool rivers = false)
    {
        //Create Bitmap Data
        Color_palette = create_palette(trees, rivers);
        Bitmap_Data = ConvertToGrayscale(img);
        //Create Bitmap File Header (populate BMP_File_Header array)
        Copy_to_Index(BMP_File_Header, new byte[] { (byte)'B', (byte)'M' }, 0); //magic number
        Copy_to_Index(BMP_File_Header, BitConverter.GetBytes(BMP_File_Header.Length
                        + DIB_header.Length + Color_palette.Length + Bitmap_Data.Length), 2); //file size
        Copy_to_Index(BMP_File_Header, new byte[] { (byte)'M', (byte)'C', (byte)'A', (byte)'T' }, 6); //reserved for application generating the bitmap file (not imprtant)
        Copy_to_Index(BMP_File_Header, BitConverter.GetBytes(BMP_File_Header.Length
                        + DIB_header.Length + Color_palette.Length), 10); //bitmap raw data offset
        //Create DIB Header (populate DIB_header array)
        Copy_to_Index(DIB_header, BitConverter.GetBytes(DIB_header.Length), 0); //DIB header length
        Copy_to_Index(DIB_header, BitConverter.GetBytes(((Bitmap)img).Width), 4); //image width
        Copy_to_Index(DIB_header, BitConverter.GetBytes(((Bitmap)img).Height), 8); //image height
        Copy_to_Index(DIB_header, new byte[] { (byte)1, (byte)0 }, 12); //color planes. N.B. Must be set to 1
        Copy_to_Index(DIB_header, new byte[] { (byte)8, (byte)0 }, 14); //bits per pixel
        Copy_to_Index(DIB_header, BitConverter.GetBytes(0), 16); //compression method N.B. BI_RGB = 0
        Copy_to_Index(DIB_header, BitConverter.GetBytes(Bitmap_Data.Length), 20); //lenght of raw bitmap data
        Copy_to_Index(DIB_header, BitConverter.GetBytes(1000), 24); //horizontal reselution N.B. not important
        Copy_to_Index(DIB_header, BitConverter.GetBytes(1000), 28); //vertical reselution N.B. not important
        Copy_to_Index(DIB_header, BitConverter.GetBytes(256), 32); //number of colors in the palette
        Copy_to_Index(DIB_header, BitConverter.GetBytes(0), 36); //number of important colors used N.B. 0 = all colors are imprtant
        //Create Color palett
    }
    //convert the color pixels of Source image into a grayscale bitmap (raw data)
    static byte[] ConvertToGrayscale(Image Source)
    {
        Bitmap source = (Bitmap)Source;
        int padding = (source.Width % 4) != 0 ? 4 - (source.Width % 4) : 0; //determine padding needed for bitmap file
        byte[] bytes = new byte[source.Width * source.Height + padding * source.Height]; //create array to contain bitmap data with paddin
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                Color c = source.GetPixel(x, y);
                bytes[(source.Height - 1 - y) * source.Width + (source.Height - 1 - y) * padding + x] = (byte)GetIndex(c);
            }
            //add the padding
            for (int i = 0; i < padding; i++)
            {
                bytes[(source.Height - y) * source.Width + (source.Height - 1 - y) * padding + i] = (byte)0;
            }
        }
        return bytes;
    }


    //creates a grayscale bitmap file of Image specified by Path
    static public bool CreateGrayBitmapFile(Image Image, string Path, bool trees = false, bool rivers=false)
    {
        try
        {
            create_parts(Image, trees, rivers);
            //Write to file
            FileStream oFileStream;
            oFileStream = new FileStream(Path, System.IO.FileMode.OpenOrCreate);
            oFileStream.Write(BMP_File_Header, 0, BMP_File_Header.Length);
            oFileStream.Write(DIB_header, 0, DIB_header.Length);
            oFileStream.Write(Color_palette, 0, Color_palette.Length);
            oFileStream.Write(Bitmap_Data, 0, Bitmap_Data.Length);
            oFileStream.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }
    //returns a byte array of a grey scale bitmap image
    static public byte[] CreateGrayBitmapArray(Image Image)
    {
        try
        {
            create_parts(Image);
            //Create the array
            byte[] bitmap_array = new byte[BMP_File_Header.Length + DIB_header.Length
                                            + Color_palette.Length + Bitmap_Data.Length];
            Copy_to_Index(bitmap_array, BMP_File_Header, 0);
            Copy_to_Index(bitmap_array, DIB_header, BMP_File_Header.Length);
            Copy_to_Index(bitmap_array, Color_palette, BMP_File_Header.Length + DIB_header.Length);
            Copy_to_Index(bitmap_array, Bitmap_Data, BMP_File_Header.Length + DIB_header.Length + Color_palette.Length);

            return bitmap_array;
        }
        catch
        {
            return new byte[1]; //return a null single byte array if fails
        }
    }
    //adds dtata of Source array to Destinition array at the Index
    static bool Copy_to_Index(byte[] destination, byte[] source, int index)
    {
        try
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination[i + index] = source[i];
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
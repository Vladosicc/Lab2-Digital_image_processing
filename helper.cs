using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace SCOI_2_R
{
    static class _helper
    {
        public static double InBorder(double source, double min = 0, double max = 255)
        {
            if (source > max)
                return max;
            if (source < min)
                return min;
            return source;
        }

        public static CPoint[] AsCPoints(this List<Point> points)
        {
            points.Sort((x, y) => x.X.CompareTo(y.X));
            CPoint[] cPoint = new CPoint[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                cPoint[i] = new CPoint(points[i].X, points[i].Y);
            }
            return cPoint;
        }

        public static Point[] AsArrayPoint(this List<Point> points)
        {
            points.Sort((x, y) => x.X.CompareTo(y.X));
            Point[] ar = new Point[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                ar[i] = new Point(points[i].X, points[i].Y);
            }
            return ar;
        }

        public static Point2D[] AsArrayPoint2D(this List<Point> points)
        {
            points.Sort((x, y) => x.X.CompareTo(y.X));
            Point2D[] ar = new Point2D[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                ar[i] = new Point2D(points[i].X, points[i].Y);
            }
            return ar;
        }

        public static Segment[] GetSegmentAr(Point2D[] values)
        {
            int n = values.Length;
            Segment[] segments = new Segment[n - 1];
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = new Segment();
            }
            return segments;
        }

        public static Point GetBytePoint(this Point point, double Max = 510)
        {
            double scale = Max / 255;
            return new Point(point.X / scale, point.Y / scale);
        }

        public static Point[] GetBytePoints(this Point[] point, double Max = 510)
        {
            double scale = Max / 255;
            Point[] resp = new Point[point.Length];
            int i = 0;
            foreach (var p in point)
            {
                resp[i] = new Point(point[i].X / scale, point[i].Y / scale);
                i++;
            }
            return resp;
        }

        private static int[] GetBytePoints(int[] point, double Max = 510)
        {
            double scale = Max / 255;
            int[] resp = new int[256];
            for (int i = 0; i < 256; i++)
            {
                resp[i] = (int)(point[(int)(i * scale)] / scale);
            }
            return resp;
        }

        private static byte[] GetBytePointsByte(int[] point, double Max = 510)
        {
            double scale = Max / 255;
            byte[] resp = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                resp[i] = (byte)(point[(int)(i * scale)] / scale);
            }
            return resp;
        }

        public static int[] GetValuesFromSegment(this Segment[] segments, Point[] points)
        {
            int[] resp = new int[511];

            int numX = 0;
            double dt = 0.0001;
            //Рассчет значений y в точках i
            for (int i = 0; i < segments.Length; i++)
            {
                for (double dx = 0; dx <= 1; dx += dt)
                {
                    Point curP = segments[i].calc(dx);
                    if (curP.X > numX)
                    {
                        resp[numX] = (int)InBorder(curP.Y, 0, 510);
                        numX++;
                    }
                }
            }
            resp = GetBytePoints(resp);
            //Ручками зададим последние точки
            resp[0] = (int)points[0].GetBytePoint().Y;
            resp[255] = (int)points[points.Length - 1].GetBytePoint().Y;
            return resp;
        }

        public static byte[] GetValuesBytesFromSegment(this Segment[] segments, Point[] points)
        {
            int[] resp = new int[511];

            int numX = 0;
            double dt = 0.0001;
            //Рассчет значений y в точках i
            for (int i = 0; i < segments.Length; i++)
            {
                for (double dx = 0; dx <= 1; dx += dt)
                {
                    Point curP = segments[i].calc(dx);
                    if (curP.X > numX)
                    {
                        resp[numX] = (int)InBorder(curP.Y, 0, 510);
                        numX++;
                    }
                }
            }
            var resp2 = GetBytePointsByte(resp);
            //Ручками зададим последние точки
            resp2[0] = (byte)points[0].GetBytePoint().Y;
            resp2[255] = (byte)points[points.Length - 1].GetBytePoint().Y;
            return resp2;
        }

        public static Bitmap GetBitmap(string path)
        {
            Bitmap resp = new Bitmap(path);
            if(resp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb)
            {
                resp = resp.To24bppRgb();
            }
            return resp;
        }
    }

    public static class ImageConvert
    {
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            byte[] Bytes = new byte[Size];
            System.Runtime.InteropServices.Marshal.Copy(ptr, Bytes, 0, Size);
            bitmap.UnlockBits(bmpData);
            return BitmapSource.Create(bitmap.Width, bitmap.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null, Bytes, bmpData.Stride);
        }

        public static Bitmap ToBitmap(this BitmapSource bitmaps)
        {
            int stride = (int)bitmaps.PixelWidth * (bitmaps.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[(int)bitmaps.PixelHeight * stride];
            bitmaps.CopyPixels(pixels, stride, 0);
            var res = new Bitmap(bitmaps.PixelWidth, bitmaps.PixelHeight);
            var bmpData = res.LockBits(new Rectangle(0, 0, res.Width, res.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, res.PixelFormat);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            byte[] Bytes = new byte[Size];
            System.Runtime.InteropServices.Marshal.Copy(ptr, Bytes, 0, Size);
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, Size);
            res.UnlockBits(bmpData);
            if (res.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                res = res.To24bppRgb();
            return res;
        }

        public static Bitmap To24bppRgb(this Bitmap bitmap) //Возвращает битмап в заданном формате
        {
            var newbit24rgb = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bitmap.Dispose();
            return newbit24rgb;
        }

        public static BitmapSource ToBitmapSource(this byte[] Bytes, int stride, int width, int height)
        {
            return BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null, Bytes, stride);
        }

        public static Bitmap ChangeFormat(this Bitmap bitmap, System.Drawing.Imaging.PixelFormat px)
        {
            var newbit = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), px);
            bitmap.Dispose();
            return newbit;
        }

    }
}

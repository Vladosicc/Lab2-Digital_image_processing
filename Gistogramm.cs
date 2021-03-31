using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace SCOI_2_R
{
    class Gistogramm
    {
        int[] _countPix = new int[256];
        int[] _countPixOrig = new int[256];

        byte[] bytes1;
        byte[] _bytesMinimalize;

        byte[] bClone;
        byte[] _bCloneMinimalize;

        int _stride;
        int _height;
        int _width;

        int _strideMin;
        int _heightMin;
        int _widthMin;

        public bool HasMinValue = false;

        public BitmapSource BitmapSourse { get => GetBitmapSource(bClone); }
        public Bitmap Bitmap { get => GetBitmap(bClone); }

        public BitmapSource BitmapSourseOrig { get => GetBitmapSource(bytes1); }
        public Bitmap BitmapOrig { get => GetBitmap(bytes1); }

        public BitmapSource BitmapSourseMin { get => GetBitmapSource(_bCloneMinimalize, true); }
        public BitmapSource BitmapSourseOrigMin { get => GetBitmapSource(_bytesMinimalize, true); }

        public int[] GistSource { get => _countPix; }

        public int[] GistSourceOrig { get => _countPixOrig; }


        /// <summary>
        /// Инициализация. 
        /// bool - Применяет Bitmap.Dispose в конце функции или нет
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="DisposeThisBitmap"></param>
        public Gistogramm(Bitmap Source, bool DisposeThisBitmap = true)
        {
            var bmpData = Source.LockBits(new Rectangle(0, 0, Source.Width, Source.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Source.PixelFormat);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            _stride = bmpData.Stride;
            _height = Source.Height;
            _width = Source.Width;
            bytes1 = new byte[Size];
            bClone = new byte[bytes1.Length];
            System.Runtime.InteropServices.Marshal.Copy(ptr, bytes1, 0, Size);
            Source.UnlockBits(bmpData);
            minimalazeImg(Source);
            _getCountOrig();
            if (DisposeThisBitmap)
                Source.Dispose();
        }

        private void minimalazeImg(Bitmap Source, int MaxWidth = 854)
        {
            if (Source.Width < MaxWidth)
                return;
            int MaxHeight = MaxWidth * Source.Height / Source.Width;
            var newbit = new Bitmap(Source, MaxWidth, MaxHeight);
            newbit = newbit.To24bppRgb();
            var bmpData = newbit.LockBits(new Rectangle(0, 0, newbit.Width, newbit.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, newbit.PixelFormat);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            _strideMin = bmpData.Stride;
            _heightMin = newbit.Height;
            _widthMin = newbit.Width;
            _bytesMinimalize = new byte[Size];
            _bCloneMinimalize = new byte[_bytesMinimalize.Length];
            System.Runtime.InteropServices.Marshal.Copy(ptr, _bytesMinimalize, 0, Size);
            newbit.UnlockBits(bmpData);
            newbit.Dispose();
            HasMinValue = true;
        }

        private void _getCountOrig()
        {
            Import.GetGistSource(bytes1, bytes1.Length, _countPixOrig);
            _countPixOrig.CopyTo(_countPix,0);
        }

        public void CalculateGist(Bitmap Source)
        {
            var bmpData = Source.LockBits(new Rectangle(0, 0, Source.Width, Source.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Source.PixelFormat);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            byte[] bytes1 = new byte[Size];
            System.Runtime.InteropServices.Marshal.Copy(ptr, bytes1, 0, Size);

            for (int i = 0; i < bytes1.Length; i += 3)
            {
                _countPix[(bytes1[i] + bytes1[i + 1] + bytes1[i + 2]) / 3]++;
            }
            Source.UnlockBits(bmpData);
        }

        public Bitmap ReCalculateBitmap(Bitmap bitmap, Segment[] segments, Point[] points)
        {
            Bitmap Source = (Bitmap)bitmap.Clone();

            var bmpData = Source.LockBits(new Rectangle(0, 0, Source.Width, Source.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Source.PixelFormat);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            byte[] bytes1 = new byte[Size];
            System.Runtime.InteropServices.Marshal.Copy(ptr, bytes1, 0, Size);

            var lev = segments.GetValuesFromSegment(points);

            for (int i = 0; i < bytes1.Length; i++)
            {
                bytes1[i] = (byte)lev[bytes1[i]];
            }

            //Редачим сурс
            System.Runtime.InteropServices.Marshal.Copy(bytes1, 0, ptr, Size);
            Source.UnlockBits(bmpData);
            return Source;
        }

        public Bitmap AllMethods(Bitmap bitmap, Segment[] segments, Point[] points)
        {
            try
            {

                Bitmap Source = (Bitmap)bitmap.Clone();

                var bmpData = Source.LockBits(new Rectangle(0, 0, Source.Width, Source.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Source.PixelFormat);
                var ptr = bmpData.Scan0;
                var Size = bmpData.Stride * bmpData.Height;
                byte[] bytes1 = new byte[Size];
                System.Runtime.InteropServices.Marshal.Copy(ptr, bytes1, 0, Size);

                var lev = segments.GetValuesFromSegment(points);

                for (int i = 0; i < bytes1.Length; i += 3)
                {
                    bytes1[i] = (byte)lev[bytes1[i]];
                    bytes1[i + 1] = (byte)lev[bytes1[i + 1]];
                    bytes1[i + 2] = (byte)lev[bytes1[i + 2]];
                    _countPix[(bytes1[i] + bytes1[i + 1] + bytes1[i + 2]) / 3]++;
                }

                //Редачим сурс
                System.Runtime.InteropServices.Marshal.Copy(bytes1, 0, ptr, Size);
                Source.UnlockBits(bmpData);
                return Source;
            }
            catch
            {
                return bitmap;
            }
        }

        public void GetGistCPP()
        {
            Import.GetGistSource(bytes1, bytes1.Length, _countPix);
        }

        public Bitmap GetNewBitmapCPP(Bitmap bitmap, Segment[] segments, Point[] points)
        {
            Bitmap Source = (Bitmap)bitmap.Clone();

            var bmpData = Source.LockBits(new Rectangle(0, 0, Source.Width, Source.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Source.PixelFormat);
            var ptr = bmpData.Scan0;
            var Size = bmpData.Stride * bmpData.Height;
            byte[] bytes1 = new byte[Size];
            System.Runtime.InteropServices.Marshal.Copy(ptr, bytes1, 0, Size);

            var lev = segments.GetValuesBytesFromSegment(points);

            Import.ReCalcBitmap(bytes1, bytes1.Length, lev);

            //Редачим сурс
            System.Runtime.InteropServices.Marshal.Copy(bytes1, 0, ptr, Size);
            Source.UnlockBits(bmpData);
            return Source;
        }

        public BitmapSource AllMethodsCPP(Segment[] segments, Point[] points)
        {
            var lev = segments.GetValuesBytesFromSegment(points);

            bytes1.CopyTo(bClone, 0);
            int[] pixC = new int[256];

            Import.All(bClone, bClone.Length, lev, pixC);
            _countPix = pixC;

            //Создаем новый битмапsource
            var Source = bClone.ToBitmapSource(_stride, _width, _height);
            return Source;
        }

        public BitmapSource AllMethodsCPPForMinimalize(Segment[] segments, Point[] points)
        {
            var lev = segments.GetValuesBytesFromSegment(points);

            _bytesMinimalize.CopyTo(_bCloneMinimalize, 0);
            int[] pixC = new int[256];

            Import.All(_bCloneMinimalize, _bCloneMinimalize.Length, lev, pixC);
            _countPix = pixC;

            //Создаем новый битмапsource
            var Source = _bCloneMinimalize.ToBitmapSource(_strideMin, _widthMin, _heightMin);
            return Source;
        }

        private BitmapSource GetBitmapSource(byte[] b, bool min = false)
        {
            if (!min)
            {
                var Source = b.ToBitmapSource(_stride, _width, _height);
                return Source;
            }
            else
            {
                var Source = b.ToBitmapSource(_strideMin, _widthMin, _heightMin);
                return Source;
            }
        }

        private Bitmap GetBitmap(byte[] b, bool min = false)
        {
            if (!min)
            {
                var Source = b.ToBitmapSource(_stride, _width, _height);
                return Source.ToBitmap();
            }
            else
            {
                var Source = b.ToBitmapSource(_stride, _width, _height);
                return Source.ToBitmap();
            }
        }
    }
}

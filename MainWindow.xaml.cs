using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;

namespace SCOI_2_R
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Point> _realPoints = new List<Point>();
        Gistogramm gistogramm;

        public MainWindow()
        {
            InitializeComponent();
            _realPoints.Add(new Point(0, 0));
            _realPoints.Add(new Point(255, 255));
            SizeChanged += UpdateGraph;
            //Graph.MouseDown += Graph_MouseDown;
            Canv.MouseDown += Canv_MouseDown;

            Optimiz.IsChecked = true;
            Apply.IsChecked = true;
            Apply.Unchecked += Apply_Unchecked;
            Apply.Checked += Apply_Checked;

            gistogramm = new Gistogramm(new Bitmap(100,100));
            //gistogramm.GetGistCPP();
            Picture.Source = gistogramm.BitmapSourse;

            for (int i = 0; i < 256; i++)
            {
                Gist.Children.Add(new System.Windows.Shapes.Rectangle() { Width = 4, Height = 0 * Gist.Height, Fill = Brushes.Black, Margin = new Thickness(0, Gist.Height - 0 * Gist.Height, 0, 0) });
            }

            //test
            byte[] bts = new byte[] { 0, 1, 3, 4, 5, 6 };
            int[] s = new int[256];
            Import.GetGistSource(bts, bts.Length, s);
            Import.ReCalcBitmap(bts, bts.Length, bts);
            //
        }

        private void Apply_Checked(object sender, RoutedEventArgs e)
        {
            Picture.Source = gistogramm.BitmapSourse;
            BuildGistogramm(gistogramm.GistSource);
        }

        private void Apply_Unchecked(object sender, RoutedEventArgs e)
        {
            Picture.Source = gistogramm.BitmapSourseOrig;
            BuildGistogramm(gistogramm.GistSourceOrig);
        }

        double func(double x) => 510 - x;

        void UpdateGraph(object sender, SizeChangedEventArgs e)
        {
            foreach (var p in _realPoints)
            {
                var rect = new System.Windows.Shapes.Rectangle() { Width = 15, Height = 15, Fill = Brushes.Red };
                rect.MouseDown += Rect_MouseDown;
                rect.PreviewMouseMove += Rect_PreviewMouseMove;
                rect.PreviewMouseDown += Rect_PreviewMouseDown;
                rect.PreviewMouseUp += Rect_PreviewMouseUp;
                Canvas.SetTop(rect, 510 - p.Y * 2);
                Canvas.SetLeft(rect, p.X * 2);
                Canv.Children.Add(rect);
            }
            var pixelWidth = Graph.ActualWidth;
            PointCollection points = new PointCollection((int)pixelWidth + 1);
            for (int pixelX = 0; pixelX <= pixelWidth; pixelX++)
            {
                points.Add(new Point(pixelX, func(pixelX)));
            }
            GraphDefault.Points = points;
            UpdatePoints(Canv);
            UpdAll();
        }

        private void Graph_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = ((System.Windows.Shapes.Polyline)sender).Parent as Canvas;

            var rect = new System.Windows.Shapes.Rectangle() { Width = 15, Height = 15, Fill = Brushes.Red };
            rect.MouseDown += Rect_MouseDown;
            rect.PreviewMouseMove += Rect_PreviewMouseMove;
            rect.PreviewMouseDown += Rect_PreviewMouseDown;
            rect.PreviewMouseUp += Rect_PreviewMouseUp;
            Canvas.SetTop(rect, e.GetPosition(canvas).Y - 2.5);
            Canvas.SetLeft(rect, e.GetPosition(canvas).X - 2.5);
            Canv.Children.Add(rect);

            WriteLog(e.GetPosition(canvas).ToString());
        }

        Point p;
        bool canmove = false;
        private void Canv_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (canmove == false && e.LeftButton == MouseButtonState.Pressed)
            {
                var rect = new System.Windows.Shapes.Rectangle() { Width = 15, Height = 15, Fill = Brushes.Red };
                rect.MouseDown += Rect_MouseDown;
                rect.PreviewMouseMove += Rect_PreviewMouseMove;
                rect.PreviewMouseDown += Rect_PreviewMouseDown;
                rect.PreviewMouseUp += Rect_PreviewMouseUp;
                Canvas.SetTop(rect, e.GetPosition(Canv).Y - 2.5);
                Canvas.SetLeft(rect, e.GetPosition(Canv).X - 2.5);
                Canv.Children.Add(rect);

                WriteLog(e.GetPosition(Canv).ToString());
            }
        }
        private void Rect_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Shapes.Rectangle r = sender as System.Windows.Shapes.Rectangle;
                Mouse.Capture(r);
                p = Mouse.GetPosition(r);
                canmove = true;
            }
        }

        private void Rect_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (canmove)
            {
                System.Windows.Shapes.Rectangle r = (System.Windows.Shapes.Rectangle)sender;
                double XposNew = _helper.InBorder(e.GetPosition(null).X - p.X - Canv.Margin.Left, 0, 510);
                double YposNew = _helper.InBorder(e.GetPosition(null).Y - p.Y - Canv.Margin.Top, 0, 510);

                r.SetValue(Canvas.LeftProperty, XposNew);
                r.SetValue(Canvas.TopProperty, YposNew);

                if (Optimiz.IsChecked.Value)
                    UpdAllMin();
                else
                    UpdAll();
            }
        }

        private void Rect_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            canmove = false;

            UpdatePoints(Canv);
            var points = _realPoints.AsArrayPoint2D();
            var Segment = _helper.GetSegmentAr(points);
            SplineBezier.calculateSpline(points, Segment);

            UpdateGraph(Segment);

            UpdAll();
        }

        private void Rect_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (_realPoints.Count <= 2)
                {
                    return;
                }
                System.Windows.Shapes.Rectangle rectangleForDelete = sender as System.Windows.Shapes.Rectangle;
                Canv.Children.Remove(rectangleForDelete);

                UpdAll();
            }
        }

        private void UpdatePoints(object canv)
        {
            _realPoints.Clear();
            Canvas canvas = canv as Canvas;

            foreach (var r in canvas.Children)
            {
                System.Windows.Shapes.Rectangle rectangle = r as System.Windows.Shapes.Rectangle;
                if (rectangle != null)
                {
                    _realPoints.Add(new Point((double)rectangle.GetValue(Canvas.LeftProperty), 510 - (double)rectangle.GetValue(Canvas.TopProperty)));
                }
            }
            _realPoints.Sort((x, y) => x.X.CompareTo(y.X));
            if (_realPoints[_realPoints.Count - 1].X < 510)
            {
                _realPoints.Add(new Point(255, _realPoints[_realPoints.Count - 1].Y));
                var rect = new System.Windows.Shapes.Rectangle() { Width = 15, Height = 15, Fill = Brushes.Red };
                rect.MouseDown += Rect_MouseDown;
                rect.PreviewMouseMove += Rect_PreviewMouseMove;
                rect.PreviewMouseDown += Rect_PreviewMouseDown;
                rect.PreviewMouseUp += Rect_PreviewMouseUp;
                Canvas.SetTop(rect, 510 - _realPoints[_realPoints.Count - 1].Y);
                Canvas.SetLeft(rect, 510);
                Canv.Children.Add(rect);
            }
            if (_realPoints[0].X > 0)
            {
                _realPoints.Add(new Point(0, _realPoints[0].Y));
                var rect = new System.Windows.Shapes.Rectangle() { Width = 15, Height = 15, Fill = Brushes.Red };
                rect.MouseDown += Rect_MouseDown;
                rect.PreviewMouseMove += Rect_PreviewMouseMove;
                rect.PreviewMouseDown += Rect_PreviewMouseDown;
                rect.PreviewMouseUp += Rect_PreviewMouseUp;
                Canvas.SetTop(rect, 510 - _realPoints[0].Y);
                Canvas.SetLeft(rect, 0);
                Canv.Children.Add(rect);
                _realPoints.Sort((x, y) => x.X.CompareTo(y.X));
            }
        }

        private void UpdateGraph(CSplineSubinterval[] cubicSplines)
        {
            Graph.Points.Clear();
            var Points = _realPoints.AsArrayPoint();
            int numOfSpline = 0;
            for (double i = 0; i < 510; i += 0.5)
            {
                Graph.Points.Add(new Point(i, 510 - cubicSplines[numOfSpline].F(i)));

                if (i > Points[numOfSpline].X)
                {
                    if (numOfSpline != cubicSplines.Length - 1)
                        numOfSpline++;
                }
            }
        }

        private void UpdateGraph(Point[] points)
        {
            Graph.Points.Clear();
            foreach (var p in points)
            {
                Graph.Points.Add(new Point(p.X, 510 - p.Y));
            }
        }

        private void UpdateGraph(Segment[] segments)
        {
            Graph.Points.Clear();
            double delt = 0.01;
            foreach (var s in segments)
            {
                for (double i = 0; i <= 1.01; i += delt)
                {
                    Point curP = s.calc(i);
                    Graph.Points.Add(new Point(_helper.InBorder(curP.X, 0, 510), _helper.InBorder(510 - curP.Y, 0, 510)));
                }
            }
        }

        public void WriteLog(string message, System.Windows.Media.SolidColorBrush color = null)
        {
            if (color == null)
                color = System.Windows.Media.Brushes.Black;
            var text = new TextBlock() { Text = message, Foreground = color };
            Log.Items.Add(text);
            Log.ScrollIntoView(text);
            Log.SelectedItem = text;
        }

        public void BuildGistogramm(int[] src)
        {
            double max = src[0];
            foreach (var item in src)
            {
                if (item > max)
                    max = item;
            }

            for (int i = 0; i < src.Length; i++)
            {
                (Gist.Children[i] as System.Windows.Shapes.Rectangle).Height = (src[i] / max) * Gist.Height;
                (Gist.Children[i] as System.Windows.Shapes.Rectangle).Margin = new Thickness(0, Gist.Height - (src[i] / max) * Gist.Height, 0, 0);
            }
        }

        DateTime timeFromLastUpd = DateTime.UtcNow;
        //Сколько мс пройдет перед следующей обработкой = delay + Freq
        //Freq = время предыдущей обработки в мс
        int delay = 5;
        int Freq = 50;

        public void UpdAll()
        {
            UpdatePoints(Canv);
            var points = _realPoints.AsArrayPoint2D();
            var Segment = _helper.GetSegmentAr(points);
            SplineBezier.calculateSpline(_realPoints.AsArrayPoint2D(), Segment);
            UpdateGraph(Segment);

            if (DateTime.UtcNow - timeFromLastUpd < new TimeSpan(10000 * (Freq + delay)))
            {
                return;
            }

            var startT = DateTime.UtcNow;


            gistogramm.AllMethodsCPP(Segment, _realPoints.AsArrayPoint());
            if (Apply.IsChecked.Value == true)
            {
                Picture.Source = gistogramm.BitmapSourse;
                BuildGistogramm(gistogramm.GistSource);
            }
            else
            {
                Picture.Source = gistogramm.BitmapSourseOrig;
                BuildGistogramm(gistogramm.GistSourceOrig);
            }

            timeFromLastUpd = DateTime.UtcNow;
            Freq = (int)(DateTime.UtcNow - startT).TotalMilliseconds;

            //Показать память
            Process process = Process.GetCurrentProcess();
            long memoryAmount = process.WorkingSet64;
            WriteLog("Памяти скушано - " + (memoryAmount / (1024 * 1024)).ToString(), Brushes.Purple);

            WriteLog("Обработано за - " + Freq + "mc", Brushes.Purple);
        }
        public void UpdAllMin()
        {
            UpdatePoints(Canv);
            var points = _realPoints.AsArrayPoint2D();
            var Segment = _helper.GetSegmentAr(points);
            SplineBezier.calculateSpline(_realPoints.AsArrayPoint2D(), Segment);
            UpdateGraph(Segment);

            if (DateTime.UtcNow - timeFromLastUpd < new TimeSpan(10000 * (Freq + delay)))
            {
                return;
            }

            var startT = DateTime.UtcNow;

            if (gistogramm.HasMinValue)
            {
                gistogramm.AllMethodsCPPForMinimalize(Segment, _realPoints.AsArrayPoint());
                if (Apply.IsChecked.Value == true)
                {
                    Picture.Source = gistogramm.BitmapSourseMin;
                    BuildGistogramm(gistogramm.GistSource);
                }
                else
                {
                    Picture.Source = gistogramm.BitmapSourseOrig;
                    BuildGistogramm(gistogramm.GistSourceOrig);
                }

                timeFromLastUpd = DateTime.UtcNow;
                Freq = (int)(DateTime.UtcNow - startT).TotalMilliseconds;

                //Показать память
                //Process process = Process.GetCurrentProcess();
                //long memoryAmount = process.WorkingSet64;
                //WriteLog("Памяти скушано - " + (memoryAmount / (1024 * 1024)).ToString(), Brushes.Purple);

                WriteLog("Обработано (min) за - " + Freq + "mc", Brushes.DarkMagenta);
            }
            else
            {
                UpdAll();
            }
            
        }

        public void ResetPoints()
        {
            _realPoints.Clear();
            _realPoints.Add(new Point(0, 0));
            _realPoints.Add(new Point(255, 255));
            for (int i = 0; i < Canv.Children.Count; i++)
            {
                var rec = Canv.Children[i] as System.Windows.Shapes.Rectangle;
                if (rec != null)
                {
                    Canv.Children.Remove(rec);
                    i--;
                }
            }
            foreach (var p in _realPoints)
            {
                var rect = new System.Windows.Shapes.Rectangle() { Width = 15, Height = 15, Fill = Brushes.Red };
                rect.MouseDown += Rect_MouseDown;
                rect.PreviewMouseMove += Rect_PreviewMouseMove;
                rect.PreviewMouseDown += Rect_PreviewMouseDown;
                rect.PreviewMouseUp += Rect_PreviewMouseUp;
                Canvas.SetTop(rect, 510 - p.Y * 2);
                Canvas.SetLeft(rect, p.X * 2);
                Canv.Children.Add(rect);
            }
            Graph.Points.Clear();
            for (int pixelX = 0; pixelX <= Canv.Height; pixelX++)
            {
                Graph.Points.Add(new Point(pixelX, func(pixelX)));
            }
            UpdatePoints(Canv);
        }



        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileManager = new OpenFileDialog();
            fileManager.Filter = "Файлы jpg|*.jpg|Файлы jpeg|*.jpeg|Файлы png| *.png";
            fileManager.ShowDialog();
            var item = fileManager.FileName;
            if (item != "")
            {
                gistogramm = new Gistogramm(new Bitmap(item).To24bppRgb());
                Picture.Source = gistogramm.BitmapSourse;
                WriteLog("Загружено", Brushes.DarkGreen);
                ResetPoints();
                UpdAll();
            }
        }

        private void FileIsDropped(object sender, DragEventArgs e)
        {
            var paths = (string[])e.Data.GetData("FileDrop");
            try
            {
                foreach (var item in paths)
                {
                    gistogramm = new Gistogramm(new Bitmap(item).To24bppRgb());
                    Picture.Source = gistogramm.BitmapSourseOrig;
                    WriteLog("Загружено", Brushes.DarkGreen);
                    ResetPoints();
                    UpdAll();
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message, Brushes.Red);
            }
        }

        private void SaveAs(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileManager = new SaveFileDialog();
            fileManager.Filter = "Файлы jpg|*.jpg|Файлы jpeg|*.jpeg|Файлы png| *.png";
            fileManager.ShowDialog();
            var item = fileManager.FileName;
            try
            {
                if (item != "")
                {
                    var bsave = gistogramm.Bitmap;
                    bsave.Save(item);
                    bsave.Dispose();
                }
                WriteLog("Файл " + item + " успешно сохранен", Brushes.DarkBlue);
            }
            catch
            {
                WriteLog("Не удалось сохранить в указанный файл", Brushes.Red);
            }
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(gistogramm.BitmapSourse);
            WriteLog("Скопировано", Brushes.DarkOrange);
        }

        private void CutClick(object sender, RoutedEventArgs e)
        {

        }

        private void PasteClick(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                gistogramm = new Gistogramm(Clipboard.GetImage().ToBitmap().To24bppRgb());
                Picture.Source = gistogramm.BitmapSourseOrig;
                WriteLog("Загружено", Brushes.DarkGreen);
                ResetPoints();
                UpdAll();
            }
            else
            {
                WriteLog("Не картинка", Brushes.Red);
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void UpdAllold()
        {
            if (DateTime.UtcNow - timeFromLastUpd < new TimeSpan((long)(10000 * Freq)))
            {
                UpdatePoints(Canv);
                var p = SplineInterpolation.SplineBezierOptimal(_realPoints.AsArrayPoint());
                UpdateGraph(p);
                return;
            }

            UpdatePoints(Canv);
            var p1 = SplineInterpolation.SplineBezierOptimal(_realPoints.AsArrayPoint());
            UpdateGraph(p1);
        }

        public void UpdAllold2()
        {
            if (DateTime.UtcNow - timeFromLastUpd < new TimeSpan((long)(10000 * Freq)))
            {
                UpdatePoints(Canv);
                CubicSpline cubicSpline1 = new CubicSpline(_realPoints.AsCPoints());
                cubicSpline1.GenerateSplines();
                UpdateGraph(cubicSpline1.Splines);
                return;
            }

            UpdatePoints(Canv);
            CubicSpline cubicSpline = new CubicSpline(_realPoints.AsCPoints());
            cubicSpline.GenerateSplines();
            UpdateGraph(cubicSpline.Splines);
        }
    }
}

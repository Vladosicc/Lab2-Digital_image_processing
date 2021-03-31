using System;
using System.Windows;

namespace SCOI_2_R
{
    class SplineBezier
    {
        static double EPSILON = 1.0e-5;
        static int RESOLUTION = 32;

        public static bool calculateSpline(Point2D[] values, Segment[] bezier)
        {
            int n = values.Length - 1;

            if (n < 1)
                return false;

            Point2D tgL = new Point2D();
            Point2D tgR = new Point2D();
            Point2D cur = new Point2D();
            Point2D next = values[1] - values[0];
            
            next.normalize();

            double l1, l2, tmp, x;

            for (int i = 0; i < n; ++i)
            {
                bezier[i].points[0] = bezier[i].points[1] = values[i];
                bezier[i].points[2] = bezier[i].points[3] = values[i + 1];

                tgL = tgR.Clone();
                cur = next.Clone();

                if (i + 1 < n)
                {
                    next = values[i + 2] - values[i + 1];
                    next.normalize();

                    tgR = cur + next;
                    tgR.normalize();
                }
                else
                {
                    tgR.x = 0.0;
                    tgR.y = 0.0;
                }

                if (Math.Abs(values[i + 1].y - values[i].y) < EPSILON)
                {
                    l1 = l2 = 0.0;
                }
                else
                {
                    tmp = values[i + 1].x - values[i].x;
                    l1 = Math.Abs(tgL.x) > EPSILON ? tmp / (2.0 * tgL.x) : 1.0;
                    l2 = Math.Abs(tgR.x) > EPSILON ? tmp / (2.0 * tgR.x) : 1.0;
                }

                if (Math.Abs(tgL.x) > EPSILON && Math.Abs(tgR.x) > EPSILON)
                {
                    tmp = tgL.y / tgL.x - tgR.y / tgR.x;
                    if (Math.Abs(tmp) > EPSILON)
                    {
                        x = (values[i + 1].y - tgR.y / tgR.x * values[i + 1].x - values[i].y + tgL.y / tgL.x * values[i].x) / tmp;
                        if (x > values[i].x && x < values[i + 1].x)
                        {
                            if (tgL.y > 0.0)
                            {
                                if (l1 > l2)
                                    l1 = 0.0;
                                else
                                    l2 = 0.0;
                            }
                            else
                            {
                                if (l1 < l2)
                                    l1 = 0.0;
                                else
                                    l2 = 0.0;
                            }
                        }
                    }
                }

                bezier[i].points[1] += tgL * l1;
                bezier[i].points[2] -= tgR * l2;
            }

            return true;
        }
    }

    class Point2D
    {
        public double x, y;

        public Point2D() { x = y = 0.0; }
        public Point2D(double _x, double _y) { x = _x; y = _y; }

        public static Point2D operator +(Point2D point1, Point2D point2)
        {
            return new Point2D(point1.x + point2.x, point1.y + point2.y);
        }
        public static Point2D operator -(Point2D point1, Point2D point2)
        {
            return new Point2D(point1.x - point2.x, point1.y - point2.y);
        }

        public static Point2D operator *(Point2D point1, double cnst)
        {
            return new Point2D(point1.x * cnst, point1.y * cnst);
        }

        public Point2D Clone()
        {
            return new Point2D(this.x, this.y);
        }
        public void normalize()
        {
            double l = Math.Sqrt(x * x + y * y);
            x /= l;
            y /= l;
        }
    };

    class Segment
    {
        public Point2D[] points = new Point2D[4];

        public Point calc(double t)
        {
            double t2 = t * t;
            double t3 = t2 * t;
            double nt = 1.0 - t;
            double nt2 = nt * nt;
            double nt3 = nt2 * nt;
            Point p = new Point();
            p.X = nt3 * points[0].x + 3.0 * t * nt2 * points[1].x + 3.0 * t2 * nt * points[2].x + t3 * points[3].x;
            p.Y = nt3 * points[0].y + 3.0 * t * nt2 * points[1].y + 3.0 * t2 * nt * points[2].y + t3 * points[3].y;
            return p;
        }
    };
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvexHull
{
   struct vector
    {
        public double x;
        public double y;
    }
    public partial class Form1 : Form
    {
        Graphics graphic;
        Color color;
        Pen pen;
        Pen pen1;

        Boolean drawFlag = false;
        List<Point> allPoint = new List<Point>();
        List<Point> tmpList = new List<Point>();

        public Form1()
        {
            InitializeComponent();
            color = Color.FromArgb(255, 0, 0);
            pen = new Pen(color);
            pen1 = new Pen(Color.Green);
            graphic = this.drawPane.CreateGraphics();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (allPoint.Count > 0)
            {
                MessageBox.Show("请先清除画布。");
                return;
            }
            drawFlag = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            drawFlag = false;
        }

        private void drawPane_MouseClick(object sender, MouseEventArgs e)
        {
            if (drawFlag)
            {
                allPoint.Add(new Point(e.X, e.Y));
                tmpList.Add(new Point(e.X, e.Y));
                graphic.FillEllipse(Brushes.Blue, e.X - 3, e.Y - 3, 6, 6);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (allPoint.Count < 3)
            {
                MessageBox.Show("至少三个点");
            }
            else if (allPoint.Count == 3)
            {
                drawList(allPoint);
            }
            else
            {
                List<Point> res = calcConvexHull(tmpList);
                drawList(res);
            }
        }
        private List<Point> calcConvexHull(List<Point> list)
        {
            drawFlag = false;
            List<Point> resPoint = new List<Point>();
            int minIndex = 0;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].Y < list[minIndex].Y)
                {
                    minIndex = i;
                }
            }
            Point minPoint = list[minIndex];
            resPoint.Add(list[minIndex]);
            list.RemoveAt(minIndex);
            list.Sort(
                delegate(Point p1, Point p2)
                {
                    vector baseVec;
                    baseVec.x = 1;
                    baseVec.y = 0;

                    vector p1Vec;
                    p1Vec.x = p1.X - minPoint.X;
                    p1Vec.y = p1.Y - minPoint.Y;

                    vector p2Vec;
                    p2Vec.x = p2.X - minPoint.X;
                    p2Vec.y = p2.Y - minPoint.Y;

                    double up1 = p1Vec.x * baseVec.x;
                    double down1 = Math.Sqrt(p1Vec.x * p1Vec.x + p1Vec.y * p1Vec.y);

                    double up2 = p2Vec.x * baseVec.x;
                    double down2 = Math.Sqrt(p2Vec.x * p2Vec.x + p2Vec.y * p2Vec.y);


                    double cosP1 = up1 / down1;
                    double cosP2 = up2 / down2;

                    if (cosP1 > cosP2)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                );
            resPoint.Add(list[0]);
            resPoint.Add(list[1]);
            for (int i = 2; i < list.Count; i++)
            {
                Point basePt = resPoint[resPoint.Count - 2];
                vector v1;
                v1.x = list[i - 1].X - basePt.X;
                v1.y = list[i - 1].Y - basePt.Y;

                vector v2;
                v2.x = list[i].X - basePt.X;
                v2.y = list[i].Y - basePt.Y;

                if (v1.x * v2.y - v1.y * v2.x < 0)
                {
                    resPoint.RemoveAt(resPoint.Count - 1);
                    while (true)
                    {
                        Point basePt2 = resPoint[resPoint.Count - 2];
                        vector v12;
                        v12.x = resPoint[resPoint.Count - 1].X - basePt2.X;
                        v12.y = resPoint[resPoint.Count - 1].Y - basePt2.Y;
                        vector v22;
                        v22.x = list[i].X - basePt2.X;
                        v22.y = list[i].Y - basePt2.Y;
                        if (v12.x * v22.y - v12.y * v22.x < 0)
                        {
                            resPoint.RemoveAt(resPoint.Count - 1);
                        }
                        else
                        {
                            break;
                        }

                    }
                    resPoint.Add(list[i]);
                }
                else
                {
                    resPoint.Add(list[i]);
                }
            }
            return resPoint;
        }
        private void drawList(List<Point> list)
        {
            if (list == null)
            {
                MessageBox.Show("传入了一个null的list");
                return;
            }
            if (list != null && list.Count == 0)
            {
                MessageBox.Show("传入的list里面无数据");
                return;
            }
            if (list != null && list.Count == 1)
            {
                MessageBox.Show("一个点无法画线");
                return;
            }

            var rects = GetAllNewRectDatas(list.Select(t => new XYZ(t.X, t.Y, 0)).ToList());
          var rect=  GetMinAreaRect(rects);
            if (list != null && list.Count > 1)
            {
                int cnt = list.Count;
                for (int i = 0; i < cnt - 1; i++)
                {
                    graphic.DrawLine(pen, list[i], list[i + 1]);
                }
                graphic.DrawLine(pen, list[cnt - 1], list[0]);
            }
            if (list != null && list.Count > 1)
            {
                int cnt = rect.boundary.Count;
                var xyzs = rect.boundary;
                for (int i = 0; i < cnt - 1; i++)
                {
                    graphic.DrawLine(pen1, XyzToPoint(xyzs[i]), XyzToPoint(xyzs[i + 1]));
                }
                graphic.DrawLine(pen1, XyzToPoint( xyzs[cnt - 1]),XyzToPoint(xyzs[0]));
            }
        }
        Point XyzToPoint(XYZ xyz)
        {
            return new Point((int)xyz.X, (int)xyz.Y);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            drawFlag = false;
            allPoint.Clear();
            tmpList.Clear();
            drawPane.Refresh();
        }
        // 1.寻找多边形的中心 
        public XYZ GetCenter(List<XYZ> pts)
        {
            double sumx = 0;
            double sumy = 0;
            foreach (var p in pts)
            {
                sumx = sumx + p.X;
                sumy = sumy + p.Y;
            }
            var pt = new XYZ(sumx / pts.Count(), sumy / pts.Count(), 0);
            return pt;
        }

        // 2.旋转多边形，针对每个点实现绕中心点旋转

        public XYZ RotatePt(XYZ inpt, XYZ centerPt, double theta)
        {
            double ix = inpt.X;
            double iy = inpt.Y;
            double cx = centerPt.X;
            double cy = centerPt.Y;
            double Q = theta / 180 * 3.1415926;  //角度

            double ox, oy;
            ox = (ix - cx) * Math.Cos(Q) - (iy - cy) * Math.Sin(Q) + cx;   //旋转公式
            oy = (ix - cx) * Math.Sin(Q) + (iy - cy) * Math.Cos(Q) + cy;

            var outpt = new XYZ(ox, oy, 0);
            return outpt;
        }

        // 3.多边形旋转后求简单外接矩形

        public List<XYZ> GetRect(List<XYZ> inpts)
        {
            var outpts = new List<XYZ>();
            int size = inpts.Count();
            if (size == 0)
                return null;
            else
            {
                var tempx = new List<double>();
                var tempy = new List<double>();
                for (int i = 0; i < size; i++)
                {
                    tempx.Add(inpts[i].X);
                    tempy.Add(inpts[i].Y);
                }

                XYZ endpoint0 = new XYZ(tempx.Min(), tempy.Max(), 0);
                XYZ endpoint1 = new XYZ(tempx.Max(), tempy.Max(), 0);
                XYZ endpoint2 = new XYZ(tempx.Max(), tempy.Min(), 0);
                XYZ endpoint3 = new XYZ(tempx.Min(), tempy.Min(), 0);
                outpts.Add(endpoint0);
                outpts.Add(endpoint1);
                outpts.Add(endpoint2);
                outpts.Add(endpoint3);
                return outpts;
            }
        }
        // 4.存储每个旋转角度下多边形的外接矩形，记录外接矩形的顶点坐标、面积和此时多边形的旋转角度

        public class RectData
        {
            public List<XYZ> boundary { get; set; }
            public XYZ center { get; set; }
            public double theta { get; set; }
            public double area { get; set; }

        }

        public RectData GetRotateRectDatas(List<XYZ> inpts, double theta)
        {

            XYZ center = GetCenter(inpts);
            var tempvertices = new List<XYZ>();
            for (int i = 0; i < inpts.Count(); i++)
            {
                XYZ temp = RotatePt(inpts[i], center, theta);
                tempvertices.Add(temp);
            }
            List<XYZ> vertices = GetRect(tempvertices);
            double deltaX, deltaY;                     //求每个外接矩形的面积
            deltaX = vertices[0].X - vertices[2].X;
            deltaY = vertices[0].Y - vertices[2].Y;

            var polygen = new RectData
            {
                area = Math.Abs(deltaY * deltaX),
                center = center,
                theta = theta,
                boundary = vertices
            };
            return polygen;
        }

        //获取所有新的矩形
        public List<RectData> GetAllNewRectDatas(List<XYZ> inpts)
        {
            var polygens = new List<RectData>();

            for (int theta = 0; theta <= 90;)
            {
                polygens.Add(GetRotateRectDatas(inpts, theta));
                theta = theta + 5;
            }
            return polygens;
        }
        //获取新的矩形
        public RectData GetMinAreaRect(List<RectData> polygons)
        {

            double minarea = 100000000;
            int N = 0;
            for (int i = 0; i < polygons.Count(); i++)
            {
                if (minarea > polygons[i].area)
                {
                    minarea = polygons[i].area;
                    N = i;
                }
            }
            var polygon = new RectData();
            polygon = polygons[N];

            //旋转到最小面积的方向
            XYZ centerPt = GetCenter(polygon.boundary);
            var boundary = new List<XYZ>();
            foreach (var bound in polygon.boundary)
            {
                XYZ pt = RotatePt(bound, polygon.center, -polygon.theta);
                boundary.Add(pt);
            }
            var outpolygon = new RectData
            {
                center = polygon.center,
                area = polygon.area,
                theta = polygon.theta,
                boundary = boundary
            };
            return outpolygon;
        }

        public class XYZ
        {

            public XYZ(double v1, double v2, int v3)
            {
                this.X = v1;
                this.Y = v2;
                this.Z = v3;
            }

            public double X { get;  set; }
            public double Y { get;  set; }
            public double Z { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plot3D
{
    public partial class Plot : UserControl
    {
        List<List<double[]>> Points = new List<List<double[]>>();
        List<PointF[]> ProjPoints = new List<PointF[]>();
        private double f = 1000;
        private double d = 5;
        private double[] d_w = new double[3];
        private double last_azimuth, azimuth = 0, last_elevation, elevation = 0;
        private bool leftMousePressed = false;
        private PointF ptMouseClick;


        public double Distance
        {
            get { return d; }
            set { d = (value >= 0.1) ? d = value : d; UpdateProjection(); }
        }

        public double F
        {
            get { return f; }
            set { f = value; UpdateProjection(); }
        }

        public double[] CameraPos
        {
            get { return d_w; }
            set { d_w = value; UpdateProjection(); }
        }

        public double Azimuth
        {
            get { return azimuth; }
            set { azimuth = value; UpdateProjection(); }
        }

        public double Elevation
        {
            get { return elevation; }
            set { elevation = value; UpdateProjection(); }
        }

        public Plot()
        {
            InitializeComponent();
            MouseHandler.Add(this, MyOnMouseWheel);
            this.MouseDown += ScatterPlot_MouseDown;
            this.MouseUp += ScatterPlot_MouseUp;
            this.MouseMove += ScatterPlot_MouseMove;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        Color[] colorIdx = new Color[] { Color.Blue, Color.Red, Color.Green, Color.Orange, Color.Fuchsia, Color.Black };

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = this.CreateGraphics();
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, this.Width, this.Height));
            if (ProjPoints != null)
            {
                for (int i = 0; i < ProjPoints.Count; i++)
                {
                    foreach (PointF p in ProjPoints[i])
                    {
                        g.FillEllipse(new SolidBrush(colorIdx[i % colorIdx.Length]), new RectangleF(p.X, p.Y, 4, 4));
                    }
                }
            }
        }

        public void AddPoint(double x, double y, double z, int series)
        {
            if (Points.Count - 1 < series)
            {
                Points.Add(new List<double[]>());
            }

            Points[series].Add(new double[] { x, y, z });

            foreach (List<double[]> ser in Points)
            {
                if (ProjPoints.Count - 1 < series)
                    ProjPoints.Add(Projection.ProjectVector(ser, this.Width, this.Height, f, d_w, azimuth, elevation));
                else
                    ProjPoints[series] = Projection.ProjectVector(ser, this.Width, this.Height, f, d_w, azimuth, elevation);
            }
            this.Invalidate();
        }

        public void AddPoints(List<double[]> points)
        {
            List<double[]> _tmp = new List<double[]>(points);
            Points.Add(_tmp);
            ProjPoints.Add(Projection.ProjectVector(Points[Points.Count - 1], this.Width, this.Height, f, d_w, azimuth, elevation));
            UpdateProjection();
        }

        public void Clear()
        {
            ProjPoints.Clear();
            Points.Clear();
            Azimuth = 0;
            Elevation = 0;
        }

        private void ScatterPlot_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftMousePressed)
            {
                azimuth = last_azimuth - (ptMouseClick.X - e.X) / 100;
                elevation = last_elevation + (ptMouseClick.Y - e.Y) / 100;
                UpdateProjection();
            }
        }

        private void ScatterPlot_SizeChanged(object sender, EventArgs e)
        {
            if (ProjPoints != null)
                UpdateProjection();
        }

        private void ScatterPlot_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                leftMousePressed = true;
                ptMouseClick = new PointF(e.X, e.Y);
                last_azimuth = azimuth;
                last_elevation = elevation;
            }
        }

        private void ScatterPlot_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                leftMousePressed = false;
        }

        private void MyOnMouseWheel(MouseEventArgs e)
        {
            Distance += -e.Delta / 500D;
        }

        private void UpdateProjection()
        {
            if (ProjPoints == null)
                return;
            double x = d * Math.Cos(elevation) * Math.Cos(azimuth);
            double y = d * Math.Cos(elevation) * Math.Sin(azimuth);
            double z = d * Math.Sin(elevation);
            d_w = new double[3] { -y, z, -x };
            for (int i = 0; i < ProjPoints.Count; i++)
                ProjPoints[i] = Projection.ProjectVector(Points[i], this.Width, this.Height, f, d_w, azimuth, elevation);
            this.Invalidate();
        }

    }

    static class Matrice
    {
        public class Matrix<T>
        {
            int rows;
            int columns;

            private T[,] matrix;

            public Matrix(int n, int m)
            {
                matrix = new T[n, m];
                rows = n;
                columns = m;
            }

            public void SetValByIdx(int m, int n, T x)
            {
                matrix[n, m] = x;
            }

            public T GetValByIndex(int n, int m)
            {
                return matrix[n, m];
            }

            public void SetMatrix(T[] arr)
            {
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < columns; c++)
                        matrix[r, c] = arr[r * columns + c];
            }

            public static Matrix<T> operator |(Matrix<T> m1, Matrix<T> m2)
            {
                Matrix<T> m = new Matrix<T>(m1.rows, m1.columns + m2.columns);
                for (int r = 0; r < m1.rows; r++)
                {
                    for (int c = 0; c < m1.columns; c++)
                        m.matrix[r, c] = m1.matrix[r, c];
                    for (int c = 0; c < m2.columns; c++)
                        m.matrix[r, c + m1.columns] = m2.matrix[r, c];
                }
                return m;
            }

            public static Matrix<T> operator *(Matrix<T> m1, Matrix<T> m2)
            {
                Matrix<T> m = new Matrix<T>(m1.rows, m2.columns);
                for (int r = 0; r < m.rows; r++)
                    for (int c = 0; c < m.columns; c++)
                    {
                        T tmp = (dynamic)0;
                        for (int i = 0; i < m2.rows; i++)
                            tmp += (dynamic)m1.matrix[r, i] * (dynamic)m2.matrix[i, c];
                        m.matrix[r, c] = tmp;
                    }
                return m;
            }

            public static Matrix<T> operator ~(Matrix<T> m)
            {
                Matrix<T> tmp = new Matrix<T>(m.columns, m.rows);
                for (int r = 0; r < m.rows; r++)
                    for (int c = 0; c < m.columns; c++)
                        tmp.matrix[c, r] = m.matrix[r, c];
                return tmp;
            }

            public static Matrix<T> operator -(Matrix<T> m)
            {
                Matrix<T> tmp = new Matrix<T>(m.columns, m.rows);
                for (int r = 0; r < m.rows; r++)
                    for (int c = 0; c < m.columns; c++)
                        tmp.matrix[r, c] = -(dynamic)m.matrix[r, c];
                return tmp;
            }

            public override string ToString()
            {
                String output = "";
                for (int r = 0; r < rows; r++)
                {
                    output += "[\t";
                    for (int c = 0; c < columns; c++)
                    {
                        output += matrix[r, c].ToString();
                        if (c < columns - 1) output += ",\t";
                    }
                    output += "]\n";
                }
                return output;
            }
        }


    }
    static class Projection
    {
        public static PointF Project(double[] coordinates, double screenWidth, double screenHeight, double focalLength, double[] cameraPosition, double azimuth, double elevation)
        {
            Matrice.Matrix<double> Mext = GetMext(azimuth, elevation, cameraPosition);
            Matrice.Matrix<double> Mint = GetMint(screenWidth, screenHeight, focalLength);
            Matrice.Matrix<double> X_h = new Matrice.Matrix<double>(4, 1);
            X_h.SetMatrix(new double[] { coordinates[0], coordinates[1], coordinates[2], 1.0 });
            Matrice.Matrix<double> P = Mint * Mext * X_h;
            return new PointF((float)(P.GetValByIndex(0, 0) / P.GetValByIndex(2, 0)), (float)(P.GetValByIndex(1, 0) / P.GetValByIndex(2, 0)));
        }

        public static PointF[] ProjectVector(List<double[]> coordinates, double screenWidth, double screenHeight, double focalLength, double[] cameraPosition, double azimuth, double elevation)
        {
            Matrice.Matrix<double> Mext = GetMext(azimuth, elevation, cameraPosition);
            Matrice.Matrix<double> Mint = GetMint(screenWidth, screenHeight, focalLength);
            Matrice.Matrix<double> X_h = new Matrice.Matrix<double>(4, 1);

            PointF[] projectedPoints = new PointF[coordinates.Count];
            for (int i = 0; i < coordinates.Count; i++)
            {
                X_h.SetMatrix(new double[] { coordinates[i][0], coordinates[i][1], coordinates[i][2], 1.0 });
                Matrice.Matrix<double> P = Mint * Mext * X_h;
                projectedPoints[i] = new PointF((float)(P.GetValByIndex(0, 0) / P.GetValByIndex(2, 0)), (float)(P.GetValByIndex(1, 0) / P.GetValByIndex(2, 0)));
            }
            return projectedPoints;
        }

        private static Matrice.Matrix<double> GetMint(double screenWidth, double screenHeight, double focalLength)
        {
            Matrice.Matrix<double> Mint = new Matrice.Matrix<double>(3, 3);
            double o_x = screenWidth / 2;
            double o_y = screenHeight / 2;
            double a = 1;
            Mint.SetMatrix(new double[] { focalLength, 0, o_x, 0, focalLength * a, o_y, 0, 0, 1 });
            return Mint;
        }

        private static Matrice.Matrix<double> GetMext(double azimuth, double elevation, double[] cameraPosition)
        {
            Matrice.Matrix<double> R = RotationMatrix(azimuth, elevation);
            Matrice.Matrix<double> dw = new Matrice.Matrix<double>(3, 1);
            dw.SetMatrix(cameraPosition);
            Matrice.Matrix<double> Mext = R | (-R * dw);
            return Mext;
        }

        private static Matrice.Matrix<double> RotationMatrix(double azimuth, double elevation)
        {
            Matrice.Matrix<double> R = new Matrice.Matrix<double>(3, 3);
            R.SetMatrix(new double[] { Math.Cos(azimuth), 0, -Math.Sin(azimuth),
                                   Math.Sin(azimuth) * Math.Sin(elevation), Math.Cos(elevation), Math.Cos(azimuth) * Math.Sin(elevation),
                                   Math.Cos(elevation) * Math.Sin(azimuth), -Math.Sin(elevation), Math.Cos(azimuth) * Math.Cos(elevation) });
            return R;
        }
    }

    public static class MouseHandler
    {
        public static void Add(Control control, Action<MouseEventArgs> onMouseWheel)
        {
            if (control == null || onMouseWheel == null)
                throw new ArgumentNullException();

            var filter = new MouseWheelMessageFilter(control, onMouseWheel);
            control.MouseWheel += filter.OnMouseWheel;
            control.Disposed += (s, e) => control.MouseWheel -= filter.OnMouseWheel;
        }

        private class MouseWheelMessageFilter
        {
            private readonly Control control;
            private readonly Action<MouseEventArgs> onMouseWheel;

            public MouseWheelMessageFilter(Control control, Action<MouseEventArgs> onMouseWheel)
            {
                this.control = control;
                this.onMouseWheel = onMouseWheel;
            }

            public void OnMouseWheel(object sender, MouseEventArgs e)
            {
                if (IsMouseWithinControl())
                {
                    onMouseWheel(e);
                }
            }

            private bool IsMouseWithinControl()
            {
                var mousePosition = control.PointToClient(Control.MousePosition);
                return control.ClientRectangle.Contains(mousePosition);
            }
        }



    }


}
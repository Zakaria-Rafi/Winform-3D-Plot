using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace plot
{
    public partial class GraphiqueUtilisateur : UserControl
    {
        List<List<double[]>> Points = new List<List<double[]>>();
        List<PointF[]> PointsProj = new List<PointF[]>();
        private double f = 1000;
        private double d = 5;
        private double[] d_w = new double[3];
        private double dernierAzimut, azimut = 0, dernierElevation, elevation = 0;
        private bool clicGaucheAppuye = false;
        private PointF ptClicSouris;
        private Stopwatch fpsTimer = new Stopwatch();

        public double Distance
        {
            get { return d; }
            set { d = (value >= 0.1) ? d = value : d; MettreAJourProjection(); }
        }

        public double F
        {
            get { return f; }
            set { f = value; MettreAJourProjection(); }
        }

        public double[] PositionCamera
        {
            get { return d_w; }
            set { d_w = value; MettreAJourProjection(); }
        }

        public double Azimut
        {
            get { return azimut; }
            set { azimut = value; MettreAJourProjection(); }
        }

        public double Elevation
        {
            get { return elevation; }
            set { elevation = value; MettreAJourProjection(); }
        }

        public GraphiqueUtilisateur()
        {
            InitializeComponent();
            GestionnaireSouris.Ajouter(this, MonGestionnaireSourisMolette);

        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;    // Activer WS_EX_COMPOSITED
                return cp;
            }
        }

        Color[] indicesCouleur = new Color[] { Color.Blue, Color.Red, Color.Green, Color.Orange, Color.Fuchsia, Color.Black };

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Start the FPS timer
            if (!fpsTimer.IsRunning)
                fpsTimer.Start();

            Graphics g = this.CreateGraphics();
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, this.Width, this.Height));
            if (PointsProj != null)
            {
                // Draw the points
                for (int i = 0; i < PointsProj.Count; i++)
                {
                    foreach (PointF p in PointsProj[i])
                    {
                        g.FillEllipse(new SolidBrush(indicesCouleur[i % indicesCouleur.Length]), new RectangleF(p.X, p.Y, 4, 4));
                    }
                }

                // Calculate and display the FPS
                fpsTimer.Stop();
                double fps = 1000.0 / fpsTimer.ElapsedMilliseconds;
                g.DrawString($"FPS: {fps:F2}", Font, Brushes.Black, 10, 10);

                // Restart the FPS timer
                fpsTimer.Reset();
                fpsTimer.Start();
            }
        }

        public void AjouterPoint(double x, double y, double z, int serie)
        {
            if (Points.Count - 1 < serie)
            {
                Points.Add(new List<double[]>());
            }

            Points[serie].Add(new double[] { x, y, z });

            foreach (List<double[]> ser in Points)
            {
                if (PointsProj.Count - 1 < serie)
                    PointsProj.Add(Projection.ProjeterPoints(ser, this.Width, this.Height, f, d_w, azimut, elevation));
                else
                    PointsProj[serie] = Projection.ProjeterPoints(ser, this.Width, this.Height, f, d_w, azimut, elevation);
            }
            this.Invalidate();
        }

        public void AjouterPoints(List<double[]> points)
        {
            List<double[]> _tmp = new List<double[]>(points);
            Points.Add(_tmp);
            PointsProj.Add(Projection.ProjeterPoints(Points[Points.Count - 1], this.Width, this.Height, f, d_w, azimut, elevation));
            MettreAJourProjection();
        }

        public void Effacer()
        {
            PointsProj.Clear();
            Points.Clear();
            Azimut = 0;
            Elevation = 0;
        }

        private void GraphiqueUtilisateur_MouseMove(object sender, MouseEventArgs e)
        {
            if (clicGaucheAppuye)
            {
                azimut = dernierAzimut - (ptClicSouris.X - e.X) / 100;
                elevation = dernierElevation + (ptClicSouris.Y - e.Y) / 100;
                MettreAJourProjection();
            }
        }

        private void GraphiqueUtilisateur_SizeChanged(object sender, EventArgs e)
        {
            if (PointsProj != null)
                MettreAJourProjection();
        }

        private void GraphiqueUtilisateur_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                clicGaucheAppuye = true;
                ptClicSouris = new PointF(e.X, e.Y);
                dernierAzimut = azimut;
                dernierElevation = elevation;
            }
        }

        private void GraphiqueUtilisateur_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                clicGaucheAppuye = false;
        }

        private void MonGestionnaireSourisMolette(MouseEventArgs e)
        {
            Distance += -e.Delta / 500D;
        }

        private void MettreAJourProjection()
        {
            if (PointsProj == null)
                return;
            double x = d * Math.Cos(elevation) * Math.Cos(azimut);
            double y = d * Math.Cos(elevation) * Math.Sin(azimut);
            double z = d * Math.Sin(elevation);
            d_w = new double[3] { -y, z, -x };
            for (int i = 0; i < PointsProj.Count; i++)
                PointsProj[i] = Projection.ProjeterPoints(Points[i], this.Width, this.Height, f, d_w, azimut, elevation);
            this.Invalidate();
        }

    }

    static class CreationMatrice
    {
        public class Matrice<T>
        {
            int lignes;
            int colonnes;

            private T[,] matrice;

            public Matrice(int n, int m)
            {
                matrice = new T[n, m];
                lignes = n;
                colonnes = m;
            }

            public void DefinirValeurParIndice(int m, int n, T x)
            {
                matrice[n, m] = x;
            }

            public T ObtenirValeurParIndex(int n, int m)
            {
                return matrice[n, m];
            }

            public void DefinirMatrice(T[] arr)
            {
                for (int r = 0; r < lignes; r++)
                    for (int c = 0; c < colonnes; c++)
                        matrice[r, c] = arr[r * colonnes + c];
            }

            public static Matrice<T> operator |(Matrice<T> m1, Matrice<T> m2)
            {
                Matrice<T> m = new Matrice<T>(m1.lignes, m1.colonnes + m2.colonnes);
                for (int r = 0; r < m1.lignes; r++)
                {
                    for (int c = 0; c < m1.colonnes; c++)
                        m.matrice[r, c] = m1.matrice[r, c];
                    for (int c = 0; c < m2.colonnes; c++)
                        m.matrice[r, c + m1.colonnes] = m2.matrice[r, c];
                }
                return m;
            }

            public static Matrice<T> operator *(Matrice<T> m1, Matrice<T> m2)
            {
                Matrice<T> m = new Matrice<T>(m1.lignes, m2.colonnes);
                for (int r = 0; r < m.lignes; r++)
                    for (int c = 0; c < m.colonnes; c++)
                    {
                        T tmp = (dynamic)0;
                        for (int i = 0; i < m2.lignes; i++)
                            tmp += (dynamic)m1.matrice[r, i] * (dynamic)m2.matrice[i, c];
                        m.matrice[r, c] = tmp;
                    }
                return m;
            }

            public static Matrice<T> operator ~(Matrice<T> m)
            {
                Matrice<T> tmp = new Matrice<T>(m.colonnes, m.lignes);
                for (int r = 0; r < m.lignes; r++)
                    for (int c = 0; c < m.colonnes; c++)
                        tmp.matrice[c, r] = m.matrice[r, c];
                return tmp;
            }

            public static Matrice<T> operator -(Matrice<T> m)
            {
                Matrice<T> tmp = new Matrice<T>(m.colonnes, m.lignes);
                for (int r = 0; r < m.lignes; r++)
                    for (int c = 0; c < m.colonnes; c++)
                        tmp.matrice[r, c] = -(dynamic)m.matrice[r, c];
                return tmp;
            }

            public override string ToString()
            {
                String output = "";
                for (int r = 0; r < lignes; r++)
                {
                    output += "[\t";
                    for (int c = 0; c < colonnes; c++)
                    {
                        output += matrice[r, c].ToString();
                        if (c < colonnes - 1) output += ",\t";
                    }
                    output += "]\n";
                }
                return output;
            }
        }


    }
    static class Projection
    {
        public static PointF ProjeterPoint(double[] coordonneesPoint, double largeurEcran, double hauteurEcran, double distanceFocale, double[] positionCamera, double azimut, double elevation)
        {
            CreationMatrice.Matrice<double> matriceExterne = ObtenirMatriceExterne(azimut, elevation, positionCamera);
            CreationMatrice.Matrice<double> matriceInterne = ObtenirMatriceInterne(largeurEcran, hauteurEcran, distanceFocale);
            CreationMatrice.Matrice<double> pointHomogene = new CreationMatrice.Matrice<double>(4, 1);
            pointHomogene.DefinirMatrice(new double[] { coordonneesPoint[0], coordonneesPoint[1], coordonneesPoint[2], 1.0 });
            CreationMatrice.Matrice<double> pointProjete = matriceInterne * matriceExterne * pointHomogene;
            return new PointF((float)(pointProjete.ObtenirValeurParIndex(0, 0) / pointProjete.ObtenirValeurParIndex(2, 0)), (float)(pointProjete.ObtenirValeurParIndex(1, 0) / pointProjete.ObtenirValeurParIndex(2, 0)));
        }

        public static PointF[] ProjeterPoints(List<double[]> points, double largeurEcran, double hauteurEcran, double distanceFocale, double[] positionCamera, double azimut, double elevation)
        {
            CreationMatrice.Matrice<double> matriceExterne = ObtenirMatriceExterne(azimut, elevation, positionCamera);
            CreationMatrice.Matrice<double> matriceInterne = ObtenirMatriceInterne(largeurEcran, hauteurEcran, distanceFocale);
            CreationMatrice.Matrice<double> pointHomogene = new CreationMatrice.Matrice<double>(4, 1);

            PointF[] pointsProj = new PointF[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                pointHomogene.DefinirMatrice(new double[] { points[i][0], points[i][1], points[i][2], 1.0 });
                CreationMatrice.Matrice<double> pointProjete = matriceInterne * matriceExterne * pointHomogene;
                pointsProj[i] = new PointF((float)(pointProjete.ObtenirValeurParIndex(0, 0) / pointProjete.ObtenirValeurParIndex(2, 0)), (float)(pointProjete.ObtenirValeurParIndex(1, 0) / pointProjete.ObtenirValeurParIndex(2, 0)));
            }
            return pointsProj;
        }

        private static CreationMatrice.Matrice<double> ObtenirMatriceInterne(double largeurEcran, double hauteurEcran, double distanceFocale)
        {
            CreationMatrice.Matrice<double> matriceInterne = new CreationMatrice.Matrice<double>(3, 3);
            double centreX = largeurEcran / 2;
            double centreY = hauteurEcran / 2;
            double rapportAspect = 1;
            matriceInterne.DefinirMatrice(new double[] { distanceFocale, 0, centreX, 0, distanceFocale * rapportAspect, centreY, 0, 0, 1 });
            return matriceInterne;
        }

        private static CreationMatrice.Matrice<double> ObtenirMatriceExterne(double azimut, double elevation, double[] positionCamera)
        {
            CreationMatrice.Matrice<double> matriceRotation = ObtenirMatriceRotation(azimut, elevation);
            CreationMatrice.Matrice<double> matricePositionCamera = new CreationMatrice.Matrice<double>(3, 1);
            matricePositionCamera.DefinirMatrice(positionCamera);
            CreationMatrice.Matrice<double> matriceExterne = matriceRotation | (-matriceRotation * matricePositionCamera);
            return matriceExterne;
        }

        private static CreationMatrice.Matrice<double> ObtenirMatriceRotation(double azimut, double elevation)
        {
            CreationMatrice.Matrice<double> matriceRotation = new CreationMatrice.Matrice<double>(3, 3);
            matriceRotation.DefinirMatrice(new double[] { Math.Cos(azimut), 0, -Math.Sin(azimut),
                                            Math.Sin(azimut) * Math.Sin(elevation), Math.Cos(elevation), Math.Cos(azimut) * Math.Sin(elevation),
                                            Math.Cos(elevation) * Math.Sin(azimut), -Math.Sin(elevation), Math.Cos(azimut) * Math.Cos(elevation) });
            return matriceRotation;
        }
    }

    public static class GestionnaireSouris
    {
        public static void Ajouter(Control control, Action<MouseEventArgs> onMoletteSouris)
        {
            if (control == null || onMoletteSouris == null)
                throw new ArgumentNullException();

            var filtre = new FiltreMessageMoletteSouris(control, onMoletteSouris);
            control.MouseWheel += filtre.OnMoletteSouris;
            control.Disposed += (s, e) => control.MouseWheel -= filtre.OnMoletteSouris;
        }

        private class FiltreMessageMoletteSouris
        {
            private readonly Control control;
            private readonly Action<MouseEventArgs> onMoletteSouris;

            public FiltreMessageMoletteSouris(Control control, Action<MouseEventArgs> onMoletteSouris)
            {
                this.control = control;
                this.onMoletteSouris = onMoletteSouris;
            }

            public void OnMoletteSouris(object sender, MouseEventArgs e)
            {
                if (EstSourisDansControl())
                {
                    onMoletteSouris(e);
                }
            }

            private bool EstSourisDansControl()
            {
                var positionSouris = control.PointToClient(Control.MousePosition);
                return control.ClientRectangle.Contains(positionSouris);
            }
        }
    }




}

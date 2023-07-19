using plot;
namespace _3Dplot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            graphiqueUtilisateur2.Effacer();
            Random rand = new Random();
            double R = 1;
            List<double[]> Points = new List<double[]>();

            int randomPointCount = 50;  // Number of random points to generate
            int circularPointCount = 100; // Number of circular pattern points to generate

            // Generate random points
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < randomPointCount; i++)
                {
                    double theta = Math.PI * rand.NextDouble();
                    double phi = 2 * Math.PI * rand.NextDouble();
                    double x = R * Math.Sin(theta) * Math.Cos(phi);
                    double y = R * Math.Sin(theta) * Math.Sin(phi);
                    double z = R * Math.Cos(theta);
                    Points.Add(new double[] { x, y, z });
                }
                graphiqueUtilisateur2.AjouterPoints(Points);
                Points.Clear();
            }

            // Generate circular pattern points
            for (int i = 0; i < circularPointCount; i++)
            {
                double theta = 10D / 180 * Math.PI * Math.Sin(10 * 2 * Math.PI * i / circularPointCount);
                double phi = 2 * Math.PI * i / circularPointCount;
                double x = R * Math.Cos(theta) * Math.Cos(phi);
                double y = R * Math.Cos(theta) * Math.Sin(phi);
                double z = R * Math.Sin(theta);
                Points.Add(new double[] { x, y, z });
            }
            graphiqueUtilisateur2.AjouterPoints(Points);
        }
    }
}
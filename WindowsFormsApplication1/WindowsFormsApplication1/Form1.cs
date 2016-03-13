using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PointD = System.Drawing.Point;
using PointW = System.Windows.Point;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private PointD startPoint;
        private bool isDrawingModeEnabled = false;
        private Graphics g;
        private Pen pen = new Pen(Color.Black);
        private List<Element> drawedElements = new List<Element>();

        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawingModeEnabled)
            {
                isDrawingModeEnabled = false;
                var endPoint = new PointD(e.X, e.Y);
                drawedElements.Add(HomeGrammar.GetTerminalElement(new Line(GetCortanianCoordinates(startPoint), GetCortanianCoordinates(endPoint))));
                g.DrawLine(pen, startPoint, endPoint);
            }
            else
            {
                isDrawingModeEnabled = true;
                var newPoint = new PointD(e.X, e.Y);
                startPoint = newPoint;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var grammar = new HomeGrammar();
            Element home = grammar.GetHome();

            g.Clear(Color.White);
            g.DrawLine(pen, new PointD(0, 0), new PointD(0, pictureBox1.Height));
            g.DrawLine(pen, new PointD(0, pictureBox1.Height), new PointD(pictureBox1.Width, pictureBox1.Height));
            g.DrawLine(pen, new PointD(pictureBox1.Width, pictureBox1.Height), new PointD(pictureBox1.Width, 0));
            g.DrawLine(pen, new PointD(pictureBox1.Width, 0), new PointD(0, 0));
            drawedElements = new List<Element>();

            foreach (Line line in home.Lines)
                drawedElements.Add(HomeGrammar.GetTerminalElement(line));

            home.ScaleTransform(pictureBox1.Width / home.Length, pictureBox1.Height / home.Height);
            home.GetGeometryGroup(g);
        }

        private PointW GetCortanianCoordinates(PointD position)
        {
            return new PointW(position.X, pictureBox1.Height - position.Y);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var grammar = new HomeGrammar();
            RecognazingResult recognazingResult = grammar.IsHome(drawedElements);
            
            if (recognazingResult.IsHome)
                MessageBox.Show("Рисунок соответствует грамматике");
            else
                MessageBox.Show(string.Format("Рисунок НЕ соответствует грамматике. Не найден элемент: {0}", recognazingResult.ErrorElementName));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            g.DrawLine(pen, new PointD(0, 0), new PointD(0, pictureBox1.Height));
            g.DrawLine(pen, new PointD(0, pictureBox1.Height), new PointD(pictureBox1.Width, pictureBox1.Height));
            g.DrawLine(pen, new PointD(pictureBox1.Width, pictureBox1.Height), new PointD(pictureBox1.Width, 0));
            g.DrawLine(pen, new PointD(pictureBox1.Width, 0), new PointD(0, 0));
            drawedElements = new List<Element>();
        }
    } 
}

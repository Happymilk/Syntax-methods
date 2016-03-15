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
        PointD startPoint;
        bool draw = false;
        Graphics g;
        Pen pen = new Pen(Color.Black);
        List<Objects> drawed = new List<Objects>();

        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
        }
        private PointW GetCoordinates(PointD position)
        {
            return new PointW(position.X, pictureBox1.Height - position.Y);
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (draw)
            {
                draw = false;
                var endPoint = new PointD(e.X, e.Y);
                drawed.Add(Grammar.GetTerminalElement(new Line(GetCoordinates(startPoint), GetCoordinates(endPoint))));
                g.DrawLine(pen, startPoint, endPoint);
            }
            else
            {
                draw = true;
                var newPoint = new PointD(e.X, e.Y);
                startPoint = newPoint;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var grammar = new Grammar();
            var objects = grammar.GetGrammar();

            g.Clear(Color.White);

            drawed = new List<Objects>();

            foreach (Line line in objects.Lines)
                drawed.Add(Grammar.GetTerminalElement(line));

            objects.Scale(pictureBox1.Width / objects.Length, pictureBox1.Height / objects.Height);
            objects.Draw(g);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var grammar = new Grammar();
            var recognazingResult = grammar.IsAtGrammar(drawed);
            
            if (recognazingResult.IsHome)
                MessageBox.Show("Рисунок соответствует грамматике");
            else
                MessageBox.Show(string.Format("Рисунок не соответствует грамматике. Не найден элемент: {0}", recognazingResult.ErrorElementName));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            drawed = new List<Objects>();
        }
    } 
}

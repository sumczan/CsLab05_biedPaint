using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace $safeprojectname$
{
    public partial class Form1 : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private enum Shape
        {
            Pen,
            Rectangle,
            Line
        }

        private Shape currentShape;
        private Color currentColor;

        Point start;
        Point end;

        List<Bitmap> bitmap = new List<Bitmap>();
        Graphics drawing;

        Pen pen;

        MyShape myShape;

        float penThickness;

        public Form1()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

            tabControl1.SelectedIndex = 0;
            bitmap.Add(new Bitmap(pictureBox1.Width, pictureBox1.Height));
        }



        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.TabPages.Add(new TabPage());
            bitmap.Add(new Bitmap(pictureBox1.Width, pictureBox1.Height));
            tabControl1.SelectedIndex = tabControl1.TabCount - 1;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            int current = tabControl1.SelectedIndex;
            tabControl1.TabPages.RemoveAt(current);
            bitmap.RemoveAt(current);
            if (tabControl1.SelectedIndex == -1)
            {
                System.Windows.Forms.Application.Exit();
            }
            tabControl1.SelectedIndex = ((current != 0) ? current - 1 : current);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            bitmap[tabControl1.SelectedIndex].Save(saveFileDialog1.FileName + ".png", ImageFormat.Png);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files|*.png;";
            openFileDialog1.ShowDialog();
            bitmap[tabControl1.SelectedIndex] = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = bitmap[tabControl1.SelectedIndex];
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            currentShape = (Shape)Enum.Parse(typeof(Shape), (String)comboBox1.SelectedItem);
            currentColor = Color.FromName((String)comboBox2.SelectedItem);
            penThickness = int.Parse(comboBox3.SelectedItem.ToString());

            Brush brush = new SolidBrush(currentColor);
            pen = new Pen(brush);
            pen.Width = penThickness;

            start.X = e.X;
            start.Y = e.Y;

            switch (currentShape)
            {
                case Shape.Pen:
                    myShape = new MyShape();
                    break;
                case Shape.Rectangle:
                    myShape = new MyRectangle();
                    break;
                case Shape.Line:
                    myShape = new MyLine();
                    break;
                default:
                    break;
            }
            myShape.start = start;

            drawing = pictureBox1.CreateGraphics();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                end.X = e.X;
                end.Y = e.Y;
                myShape.end = end;
                bitmap[tabControl1.SelectedIndex] = myShape.Draw(pen, drawing, bitmap[tabControl1.SelectedIndex], true);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            bitmap[tabControl1.SelectedIndex] = myShape.Draw(pen, drawing, bitmap[tabControl1.SelectedIndex], false);
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            try
            {
                pictureBox1.Image = bitmap[tabControl1.SelectedIndex];
            }
            catch (System.ArgumentOutOfRangeException)
            {
                System.Windows.Forms.Application.Exit();
            }
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            // po dodaniu poruszania oknem przestało działać;
            this.WindowState = ((!(this.WindowState == FormWindowState.Maximized)) ? FormWindowState.Maximized : FormWindowState.Normal);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                button6.Text = "Normalize";
            }
            else if(this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                button6.Text = "Maximize";
            }
        }
    }

    public class MyShape
    {
        public Point start { get; set; }
        public Point end { get; set; }

        public virtual Bitmap Draw(Pen pen, Graphics drawing, Bitmap bitmap, bool clean)
        {
            Graphics draw = Graphics.FromImage(bitmap);

            drawing.SmoothingMode = SmoothingMode.AntiAlias;
            draw.SmoothingMode = SmoothingMode.AntiAlias;

            drawing.DrawLine(pen, this.start, this.end);
            draw.DrawLine(pen, this.start, this.end);

            this.start = this.end;
            return bitmap;
        }
    }

    public class MyRectangle : MyShape
    {
        public override Bitmap Draw(Pen pen, Graphics drawing, Bitmap bitmap, bool clean)
        {
            int x;
            int y;
            int width;
            int height;
            Graphics draw = Graphics.FromImage(bitmap);

            x = ((start.X < end.X) ? start.X : end.X);
            y = ((start.Y < end.Y) ? start.Y : end.Y);
            width = Math.Abs(start.X - end.X);
            height = Math.Abs(start.Y - end.Y);

            if (!clean)
            {
                drawing.SmoothingMode = SmoothingMode.AntiAlias;
                draw.SmoothingMode = SmoothingMode.AntiAlias;

                drawing.DrawRectangle(pen, x, y, width, height);
                draw.DrawRectangle(pen, x, y, width, height);
            }

            return bitmap;
        }
    }

    public class MyLine : MyShape
    {
        public override Bitmap Draw(Pen pen, Graphics drawing, Bitmap bitmap, bool clean)
        {
            Graphics draw = Graphics.FromImage(bitmap);
            drawing.SmoothingMode = SmoothingMode.AntiAlias;
            draw.SmoothingMode = SmoothingMode.AntiAlias;
            if (!clean)
            {
                drawing.DrawLine(pen, this.start, this.end);
                draw.DrawLine(pen, this.start, this.end);
            }
            return bitmap;
        }
    }
}

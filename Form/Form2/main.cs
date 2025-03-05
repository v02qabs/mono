using System;
using System.Windows.Forms;

class MyForm : Form
{
    public MyForm()
    {
        this.Text = "My Form";
        this.Width = 300;
        this.Height = 200;
    }
}

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MyForm());
    }
}

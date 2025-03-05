using System;
using System.Windows.Forms;


class Hello : Form{
	
	public Hello(){
		TextBox box = new TextBox();
		this.Controls.Add(box);
	}
	


	public static void Main(){
		Application.Run(new Hello());
	}
}

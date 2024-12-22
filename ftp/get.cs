using System.Net;
using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using static System.EventHandler;
class get_ftp{
	public get_ftp(){
			//gfile();
	}
	public static void Main(){
		Console.WriteLine("Hello https download");
		new get_ftp();
		Application.Run(new get_ftp().form1());
	}
	private void gfile(string url, string dir_file_name){
		try{
			WebClient client = new WebClient();
			Byte[] data = client.DownloadData(url);
			string html = Encoding.UTF8.GetString(data);
			Console.WriteLine(html);
			client.DownloadFile(url, dir_file_name);
		}
		catch(Exception e){
			Console.WriteLine(e);
		}
	}
	private TextBox url_box, save_box;
	private Form form1(){
		Form form = new Form();
		Label url = new Label();
		url.Text = "url";
		url.Location = new Point(0,0);
		url_box = new TextBox();
		url_box.Location = new Point(200,0);
		Label save_as = new Label();
		save_as.Text = "save dir and file name.";
		save_as.Location = new Point(0,30);
		
		save_box = new TextBox();
		save_box.Location = new Point(200,30);

		Button comp_button = new Button();
		comp_button.Location = new Point(0,60);
		comp_button.Text = "exec";

		comp_button.Click += new EventHandler(comp_button1);

		form.Controls.Add(url);
		form.Controls.Add(url_box);
		form.Controls.Add(save_as);
		form.Controls.Add(save_box);
		form.Controls.Add(comp_button);
		return form;
	}
	private void comp_button1(object sendeGetLineTextr, System.EventArgs e){
		
		string url_string = url_box.Text;		
		string dir_file_name_string = save_box.Text;
		Console.WriteLine(url_string);
		Console.WriteLine(dir_file_name_string);
		MessageBox.Show("ok");
		gfile(url_string, dir_file_name_string);
	}
}

using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

class Hello : Form {
    private Label count_up_label; // メンバ変数として宣言

    public Hello() {
        count_up_label = new Label(); // this.count_up_label に代入
        count_up_label.Location = new Point(10, 10);
        count_up_label.Text = "0";
        this.Controls.Add(count_up_label);

        Button download_button = new Button();
        download_button.Location = new Point(200, 30);
        download_button.Text = "download"; // タイポ修正
        this.Controls.Add(download_button);

        // イベントハンドラ名修正
        download_button.Click += new EventHandler(DownloadButton_Click);
    }

    public static void Main() {
        Application.Run(new Hello());
    }

    // イベントハンドラ名を修正
    private void DownloadButton_Click(object sender, EventArgs e) {
        int currentValue = Convert.ToInt32(count_up_label.Text);
        count_up_label.Text = (currentValue + 1).ToString();
    }
}


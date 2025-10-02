using System;
using System.Collections.Generic;
using System.Text;

class Program
{
    // ====== 簡易 UI 基本 ======
    static int Width => Console.BufferWidth;
    static int Height => Console.BufferHeight;

    static void WriteAt(int x, int y, string text)
    {
        if (x < 0 || y < 0 || y >= Height) return;
        if (x + text.Length > Width) text = text.Substring(0, Math.Max(0, Width - x));
        Console.SetCursorPosition(Math.Max(0, x), Math.Max(0, y));
        Console.Write(text);
    }

    static void FillLine(int y, char ch, ConsoleColor? fg = null, ConsoleColor? bg = null)
    {
        var oldFg = Console.ForegroundColor;
        var oldBg = Console.BackgroundColor;
        if (fg.HasValue) Console.ForegroundColor = fg.Value;
        if (bg.HasValue) Console.BackgroundColor = bg.Value;
        WriteAt(0, y, new string(ch, Math.Max(0, Width)));
        Console.ForegroundColor = oldFg;
        Console.BackgroundColor = oldBg;
    }

    static void DrawBox(int x, int y, int w, int h, string title = null)
    {
        if (w < 2 || h < 2) return;
        string top = "┌" + new string('─', w - 2) + "┐";
        string mid = "│" + new string(' ', w - 2) + "│";
        string bot = "└" + new string('─', w - 2) + "┘";

        WriteAt(x, y, top);
        for (int i = 1; i < h - 1; i++) WriteAt(x, y + i, mid);
        WriteAt(x, y + h - 1, bot);

        if (!string.IsNullOrEmpty(title))
        {
            var t = " " + title + " ";
            int tx = x + Math.Max(1, (w - t.Length) / 2);
            WriteAt(tx, y, t);
        }
    }

    // ====== メニュー ======
    class MenuItem
    {
        public string Text;
        public Action OnClick;
        public MenuItem(string text, Action onClick) { Text = text; OnClick = onClick; }
    }

    class MenuBar
    {
        public List<MenuItem> Items = new List<MenuItem>();
        public int SelectedIndex = 0;

        public void Draw()
        {
            // 背景ライン
            FillLine(0, ' ', ConsoleColor.Black, ConsoleColor.DarkCyan);

            // アイテム描画（横並び）
            int x = 1;
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                bool sel = (i == SelectedIndex);

                var oldFg = Console.ForegroundColor;
                var oldBg = Console.BackgroundColor;
                Console.ForegroundColor = sel ? ConsoleColor.Black : ConsoleColor.White;
                Console.BackgroundColor = sel ? ConsoleColor.Yellow : ConsoleColor.DarkCyan;

                string label = $" {item.Text} ";
                WriteAt(x, 0, label);
                x += label.Length + 1;

                Console.ForegroundColor = oldFg;
                Console.BackgroundColor = oldBg;
            }
        }

        public void Next() => SelectedIndex = (SelectedIndex + 1) % Math.Max(1, Items.Count);
        public void Prev() => SelectedIndex = (SelectedIndex - 1 + Items.Count) % Math.Max(1, Items.Count);
        public void Activate() { if (Items.Count > 0) Items[SelectedIndex].OnClick?.Invoke(); }
    }

    // ====== ラベル ======
    class Label
    {
        public int X, Y, W;
        public string Text;
        public ConsoleColor Fg = ConsoleColor.White;
        public ConsoleColor Bg = ConsoleColor.Black;

        public void Draw()
        {
            var oldFg = Console.ForegroundColor;
            var oldBg = Console.BackgroundColor;
            Console.ForegroundColor = Fg;
            Console.BackgroundColor = Bg;

            // 1行表示（長い場合は切詰）
            string s = Text ?? "";
            if (W > 0 && s.Length > W) s = s.Substring(0, W);
            WriteAt(X, Y, s.PadRight(Math.Max(0, W)));
            Console.ForegroundColor = oldFg;
            Console.BackgroundColor = oldBg;
        }
    }

    // ====== ステータスバー ======
    static void Status(string msg)
    {
        var old = Console.ForegroundColor;
        var oldBg = Console.BackgroundColor;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.DarkGray;
        WriteAt(0, Height - 1, (msg ?? "").PadRight(Math.Max(0, Width)));
        Console.ForegroundColor = old;
        Console.BackgroundColor = oldBg;
    }

    // ====== 画面レイアウト ======
    static MenuBar menu;
    static Label lblTitle;
    static Label lblInfo;

    static void Layout()
    {
        Console.Clear();
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        // メニュー
        menu = new MenuBar();
        menu.Items.Add(new MenuItem("File", () => Status("File menu clicked")));
        menu.Items.Add(new MenuItem("Edit", () => Status("Edit menu clicked")));
        menu.Items.Add(new MenuItem("View", () => Status("View menu clicked")));
        menu.Items.Add(new MenuItem("Help", () => Status("Help → About: CUI sample")));
        menu.Draw();

        // メインボックス
        int pad = 2;
        int boxX = 0 + pad;
        int boxY = 1 + pad; // メニューの下
        int boxW = Math.Max(20, Width - pad * 2);
        int boxH = Math.Max(8, Height - (1 + pad) - (1 + pad)); // 上:メニュー1行、下:余白

        DrawBox(boxX, boxY, boxW, boxH, "CUI Window");

        // ラベル
        lblTitle = new Label
        {
            X = boxX + 2,
            Y = boxY + 2,
            W = boxW - 4,
            Text = "Hello CUI (no dependency)",
            Fg = ConsoleColor.Cyan
        };
        lblTitle.Draw();

        lblInfo = new Label
        {
            X = boxX + 2,
            Y = boxY + 4,
            W = boxW - 4,
            Text = "← → でメニュー選択 / Enterで実行 / Qで終了",
            Fg = ConsoleColor.White
        };
        lblInfo.Draw();

        Status("Ready");
    }

    static void Redraw()
    {
        // 画面全体を再レイアウト（リサイズ対応）
        Layout();
    }

    // ====== メインループ ======
    static void Main()
    {
        try
        {
            Layout();

            while (true)
            {
                if (Console.KeyAvailable == false)
                {
                    // ターミナルサイズ変更対策
                    System.Threading.Thread.Sleep(10);
                    continue;
                }

                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Q) break;

                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        menu.Prev();
                        menu.Draw();
                        break;
                    case ConsoleKey.RightArrow:
                        menu.Next();
                        menu.Draw();
                        break;
                    case ConsoleKey.Enter:
                        menu.Activate();
                        break;
                    case ConsoleKey.Escape:
                        Status("Esc");
                        break;
                    default:
                        // F5: 再描画、その他はキー名を表示
                        if (key.Key == ConsoleKey.F5) { Redraw(); }
                        else Status($"Key: {key.Key} {(key.Modifiers != 0 ? $"({key.Modifiers})" : "")}");
                        break;
                }
            }
        }
        finally
        {
            Console.CursorVisible = true;
            Console.ResetColor();
            Console.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

class Program
{
    static int Width => Console.BufferWidth;
    static int Height => Console.BufferHeight;

    static void WriteAt(int x, int y, string text)
    {
        if (x < 0 || y < 0 || y >= Height) return;
        if (x + text.Length > Width) text = text.Substring(0, Math.Max(0, Width - x));
        Console.SetCursorPosition(Math.Max(0, x), Math.Max(0, y));
        Console.Write(text);
    }

    static void FillLine(int y, char ch, ConsoleColor fg, ConsoleColor bg)
    {
        var oldFg = Console.ForegroundColor;
        var oldBg = Console.BackgroundColor;
        Console.ForegroundColor = fg;
        Console.BackgroundColor = bg;
        WriteAt(0, y, new string(ch, Width));
        Console.ForegroundColor = oldFg;
        Console.BackgroundColor = oldBg;
    }

    class MenuItem
    {
        public string Text;
        public Action OnClick;
        public List<MenuItem> SubItems = new List<MenuItem>();
        public MenuItem(string text, Action onClick = null)
        {
            Text = text;
            OnClick = onClick;
        }
    }

    class MenuBar
    {
        public List<MenuItem> Items = new List<MenuItem>();
        public int SelectedIndex = 0;
        public int SubSelectedIndex = -1;
        public bool DropdownOpen = false;

        public void Draw()
        {
            // メニューバーライン
            FillLine(0, ' ', ConsoleColor.White, ConsoleColor.DarkBlue);

            int x = 1;
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                bool sel = (i == SelectedIndex);

                var oldFg = Console.ForegroundColor;
                var oldBg = Console.BackgroundColor;
                Console.ForegroundColor = sel ? ConsoleColor.Black : ConsoleColor.White;
                Console.BackgroundColor = sel ? ConsoleColor.Yellow : ConsoleColor.DarkBlue;

                string label = $" {item.Text} ";
                WriteAt(x, 0, label);
                x += label.Length + 1;

                Console.ForegroundColor = oldFg;
                Console.BackgroundColor = oldBg;
            }

            if (DropdownOpen) DrawDropdown();
        }

        void DrawDropdown()
        {
            var item = Items[SelectedIndex];
            int startX = 2 + SelectedIndex * 8;
            int startY = 1;

            for (int i = 0; i < item.SubItems.Count; i++)
            {
                var sub = item.SubItems[i];
                bool sel = (i == SubSelectedIndex);

                Console.ForegroundColor = sel ? ConsoleColor.Black : ConsoleColor.White;
                Console.BackgroundColor = sel ? ConsoleColor.Yellow : ConsoleColor.DarkGray;

                WriteAt(startX, startY + i, $" {sub.Text} ".PadRight(15));
                Console.ResetColor();
            }
        }

        public void Next() => SelectedIndex = (SelectedIndex + 1) % Items.Count;
        public void Prev() => SelectedIndex = (SelectedIndex - 1 + Items.Count) % Items.Count;

        public void OpenDropdown()
        {
            if (Items[SelectedIndex].SubItems.Count > 0)
            {
                DropdownOpen = true;
                SubSelectedIndex = 0;
            }
        }

        public void CloseDropdown()
        {
            DropdownOpen = false;
            SubSelectedIndex = -1;
        }

        public void SubNext()
        {
            var subs = Items[SelectedIndex].SubItems;
            if (subs.Count == 0) return;
            SubSelectedIndex = (SubSelectedIndex + 1) % subs.Count;
        }

        public void SubPrev()
        {
            var subs = Items[SelectedIndex].SubItems;
            if (subs.Count == 0) return;
            SubSelectedIndex = (SubSelectedIndex - 1 + subs.Count) % subs.Count;
        }

        public void Activate()
        {
            if (DropdownOpen && SubSelectedIndex >= 0)
                Items[SelectedIndex].SubItems[SubSelectedIndex].OnClick?.Invoke();
            else
                Items[SelectedIndex].OnClick?.Invoke();
        }
    }

    static MenuBar menu;

    // ✅ 画面リフレッシュ機能
    static void RefreshScreen()
    {
        Console.Clear();
        menu.Draw();
        ShowMsg("Screen refreshed");
    }

    static void Main()
    {
        Console.Clear();
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        menu = new MenuBar();
        menu.Items.Add(new MenuItem("File"));
        menu.Items[0].SubItems.Add(new MenuItem("Open", () => ShowMsg("Open clicked")));
        menu.Items[0].SubItems.Add(new MenuItem("Save", () => ShowMsg("Save clicked")));
        menu.Items[0].SubItems.Add(new MenuItem("Exit", () => { Environment.Exit(0); }));

        menu.Items.Add(new MenuItem("Edit"));
        menu.Items[1].SubItems.Add(new MenuItem("Copy", () => ShowMsg("Copy clicked")));
        menu.Items[1].SubItems.Add(new MenuItem("Paste", () => ShowMsg("Paste clicked")));

        menu.Items.Add(new MenuItem("Help"));
        menu.Items[2].SubItems.Add(new MenuItem("About", () => ShowMsg("About: CUI Example")));

        menu.Draw();

        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Q) break;

            if (!menu.DropdownOpen)
            {
                if (key.Key == ConsoleKey.LeftArrow) menu.Prev();
                else if (key.Key == ConsoleKey.RightArrow) menu.Next();
                else if (key.Key == ConsoleKey.Enter) menu.OpenDropdown();
                else if (key.Key == ConsoleKey.F5) RefreshScreen(); // ✅ F5でリフレッシュ
            }
            else
            {
                if (key.Key == ConsoleKey.UpArrow) menu.SubPrev();
                else if (key.Key == ConsoleKey.DownArrow) menu.SubNext();
                else if (key.Key == ConsoleKey.Escape) menu.CloseDropdown();
                else if (key.Key == ConsoleKey.Enter) { menu.Activate(); menu.CloseDropdown(); }
            }

            menu.Draw();
        }

        Console.ResetColor();
        Console.Clear();
        Console.CursorVisible = true;
    }

    static void ShowMsg(string msg)
    {
        Console.SetCursorPosition(0, Height - 2);
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = ConsoleColor.Gray;
        Console.Write(msg.PadRight(Width));
        Console.ResetColor();
    }
}

using Mindustry_Compiler.NativeInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mindustry_Compiler
{

    namespace NativeInterop
    {
        using System.Runtime.InteropServices;
        public static partial class User32
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }
        }
    }

    public partial class CompiledAlertForm : Form
    {
        float time = 0;
        public CompiledAlertForm()
        {
            InitializeComponent();
            PositionInRectangle(Screen.FromControl(this).Bounds);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            const float timeMax = 1.5F;
            time += timer1.Interval / 1000.0F;

            float n = time / timeMax;
            this.Opacity = Math.Min(2 * (1 - Math.Abs(2 * (n - 0.5))), 1);

            if (time >= timeMax)
            {
                timer1.Enabled = false;
                this.Opacity = 0.01;
                this.Visible = false;
            }
        }


        IntPtr GetMindustryGameHwnd()
        {
            IntPtr hWnd = IntPtr.Zero;
            foreach (Process pList in Process.GetProcesses())
                if (pList.MainWindowTitle == "Mindustry")
                    return pList.MainWindowHandle;
            return hWnd;
        }

        public void ShowCompilerMessage(bool success)
        {
            // Color/text (success/fail)
            lblMsg.Text = success ? "Compiled Mindustry Code" : "Compilation Failed";
            lblMsg.BackColor = success ? Color.Lime : Color.Red;

            time = 0.0F;
            this.Visible = true;
            PositionOverMindustryWindow();
            timer1.Start();
        }


        public void ShowWarning(string warningText)
        {
            lblMsg.Text = warningText;
            lblMsg.BackColor = Color.Yellow;

            time = 0.0F;
            this.Visible = true;
            PositionOverMindustryWindow();
            timer1.Start();
        }

        public void PositionOverMindustryWindow()
        {
            // Position inside Mindustry window ...
            var handle = GetMindustryGameHwnd();
            if (handle == IntPtr.Zero)
            {
                PositionInRectangle(Screen.FromControl(this).Bounds);
                return;
            }

            User32.RECT WINREC = new User32.RECT() { Left = 0, Right = 0, Top = 0, Bottom = 0 };
            User32.GetWindowRect(handle, ref WINREC);
            Rectangle b = new Rectangle(
                WINREC.Left,
                WINREC.Top,
                WINREC.Right - WINREC.Left,
                WINREC.Bottom - WINREC.Top
                );

            if (b.Width == 0 || b.Height == 0)
            {
                PositionInRectangle(Screen.FromControl(this).Bounds);
                return;
            }

            PositionInRectangle(b);
        }

        public void PositionInRectangle(Rectangle b)
        {
            this.Location = new Point(
                b.X + (b.Width - this.Width) / 2,
                b.Y + b.Height - this.Height - 80);
        }
    }
}

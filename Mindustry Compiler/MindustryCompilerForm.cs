using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.Json;
using static Mindustry_Compiler.StringBuilderExtensions;
using System.Reflection;
using System.Drawing.Text;
using System.Diagnostics;

namespace Mindustry_Compiler
{

    

    public partial class MindustryCompilerForm : Form
    {
        Match lineMatch;                        // Regex Match from line classification
        LineClass lineClass;                    // The class of the current line (assign, memwrite, etc.)
        List<string> lines;                     // Source code lines.
        public List<string> code;               // Current pointer to code additions (stack frames change)
        StackFrame baseFrame;                   // The base stack-frame
        Stack<StackFrame> stackFrames;          // Stack of all current stack-frames
        CompiledAlertForm compiledAlert;        // Popup-window which shows on compilation/switch to game
        bool doPrintFlush;                      // True if 'print/println' is called. Adds 'printflush' to end of 'main()'

        bool wasGameFocused = false;
        IDataObject originalClipboardData = null;


        /// <summary>
        /// Souce code file path. Recompiles on changed.
        /// </summary>
        public string SourcePath
        {
            get => txtPath.Text;
            set
            {
                txtPath.Text = value;
                if (File.Exists(value))
                {
                    fswSource.Path = Path.GetDirectoryName(value);
                    fswSource.Filter = Path.GetFileName(value);
                    CompileFromSourceFile();
                }
            }
        }

        //==============================================================================
        public MindustryCompilerForm()
        {
            InitializeComponent();
            chkAutoCompile.Checked = true;
        }

        /// <summary>
        /// On closing, save window size/position and source path.
        /// </summary>
        private void MindustryCompilerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.Maximized = true;
                Properties.Settings.Default.Minimized = false;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Location = Location;
                Properties.Settings.Default.Size = Size;
                Properties.Settings.Default.Maximized = false;
                Properties.Settings.Default.Minimized = false;
            }
            else
            {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.Maximized = false;
                Properties.Settings.Default.Minimized = true;
            }


            Properties.Settings.Default.SourcePath = txtPath.Text;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// On load, restore window size/position and source path.
        /// </summary>
        private void MindustryCompilerForm_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Maximized)
            {
                Location = Properties.Settings.Default.Location;
                WindowState = FormWindowState.Maximized;
                Size = Properties.Settings.Default.Size;
            }
            else if (Properties.Settings.Default.Minimized)
            {
                Location = Properties.Settings.Default.Location;
                WindowState = FormWindowState.Minimized;
                Size = Properties.Settings.Default.Size;
            }
            else
            {
                Location = Properties.Settings.Default.Location;
                Size = Properties.Settings.Default.Size;
            }

            // ~~~~~~~~ Restore source path
            SourcePath = Properties.Settings.Default.SourcePath;
            CompileFromSourceFile();
        }


        /// <summary>
        /// Compiles the source file. Returns the output assembly.
        /// </summary>
        public string CompileFromSourceFile()
        {
            string txt = "";
            try
            {
                string fpath = txtPath.Text;
                txt = File.ReadAllText(fpath);
            }
            catch (Exception e)
            {
                txtCompileMsg.ForeColor = Color.Red;
                txtCompileMsg.Text =  "Error reading source file.";
                return "";
            }

            // Compile source
            string asm = Compile(txt);
            txtAsm.Text = asm.Replace("\n", "\r\n");
            return asm;
        }

        /// <summary>
        /// Recompiles the source when it is updated/saved.
        /// </summary>
        private void fswSource_Changed(object sender, FileSystemEventArgs e)
        {
            CompileToClipboard();
        }



        //==============================================================================
        /// <summary>
        /// Finds the currenty focused window's title (check if Mindustry).
        /// </summary>
        
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return "";
        }

        /// <summary>
        /// Checks if the game is focused, and compiles to clipboard if true.
        /// Remembers the previous clipboard data, and restores when game loses focus.
        /// </summary>
        private void tmrGameFocused_Tick(object sender, EventArgs e)
        {
            // Feature not enabled? Return ...
            if (!chkAutoCompile.Checked)
                return;

            // Is mindustry focused?
            string focusedWindow = GetActiveWindowTitle();
            bool isMindustryFocused = focusedWindow == "Mindustry";

            // Switched into game?
            if (!wasGameFocused)
            {
                if (isMindustryFocused)
                {
                    originalClipboardData = Clipboard.GetDataObject();
                    CompileToClipboard();
                }
            }

            // Out of game?
            else if (wasGameFocused)
            {
                if (!isMindustryFocused && originalClipboardData != null)
                {
                    // Do not overwrite the clipbaord if the clipboard data has changed ...
                    string asm = txtAsm.Text.Replace("\r", "");
                    if (Clipboard.GetText() == asm)
                        Clipboard.SetDataObject(originalClipboardData);
                }
            }

            wasGameFocused = isMindustryFocused;
        }

        /// <summary>
        /// Compiles the source file into the clipboard.
        /// </summary>
        private void btnCompile_Click(object sender, EventArgs e)
        {
            if (File.Exists(SourcePath))
                CompileToClipboard();
        }

        /// <summary>
        /// Compiles the source file into the users clipboard.
        /// </summary>
        public void CompileToClipboard()
        {
            string asm = CompileFromSourceFile();

            try
            {
                if (asm.Length > 0)
                    Clipboard.SetText(asm);
            }
            catch(Exception e)
            {
                asm = "";
                if (compiledAlert == null)
                    compiledAlert = new CompiledAlertForm();
                compiledAlert.ShowWarning("Compile to Clipboard Fail");
            }

            if (compiledAlert == null)
                compiledAlert = new CompiledAlertForm();
            compiledAlert.ShowCompilerMessage(asm.Length > 0);
        }

        /// <summary>
        /// Copy source on click the assembly window.
        /// </summary>
        private void txtAsm_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtAsm.Text != null && txtAsm.Text.Length > 0)
                Clipboard.SetText(txtAsm.Text);
        }

        /// <summary>
        /// Mouse-drag resize on lower 'compiler message' panel.
        /// </summary>
        private void pnlMsgResize_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pnlCompilerMsg.Height -= e.Y;
                pnlCompilerMsg.Height = Math.Max(10, pnlCompilerMsg.Height);
            }
        }

        /// <summary>
        /// Choose source file.
        /// </summary>
        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (File.Exists(SourcePath))
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(SourcePath);
                else
                    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                openFileDialog.Filter = "Source files (*.c, *.cpp, *.h, *.txt)|*.c;*.cpp;*.h;*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtAsm.Text = "";
                    SourcePath = openFileDialog.FileName;
                }
            }
        }

        /// <summary>
        /// Launches the default editor for the source file.
        /// </summary>
        private void btnEditSource_Click(object sender, EventArgs e)
        {
            if (File.Exists(SourcePath))
            {
                System.Diagnostics.Process.Start(SourcePath);
            }
        }

        //==============================================================================
        /// <summary>
        /// Builds/combines into a Mindustry assembly command.
        /// </summary>
        public static string BuildCode(params string[] parts)
        {
            string output = parts[0];
            for (int i = 1; i < parts.Length; i++)
                output += " " + parts[i];
            return output;
        }
    }
}

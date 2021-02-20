using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTA_5___Solo_Public_Lobby
{
    public partial class MainForm : Form
    {
        Process[] processes;
        int processID;

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        private static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid); // throws exception if process does not exist

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        public static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }

        string[] procList;
        bool isGTA5Running = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void getAllProcess()
        {
            processes = Process.GetProcesses();
            procList = new string[processes.Length];

            for (int i = 0; i < processes.Length; i++)
                procList[i] = processes[i].ProcessName;
        }

        private void controlForGTA5()
        {
            for(int i = 0; i < processes.Length; i++)
            {
                if (procList[i] == "GTA5")
                {
                    isGTA5Running = true;
                    processID = processes[i].Id;
                    break;
                }
            }

            if (isGTA5Running == false)
            {
                MessageBox.Show("GTA 5 isn't running! Program will be closed.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
            else if (isGTA5Running == true)
            {
                MessageBox.Show("Please make sure GTA 5 is fullscreen. Otherwize, the game may crash.",
                    "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            getAllProcess();
            controlForGTA5();
        }

        private void kickButton_Click(object sender, EventArgs e)
        {
            kickButton.Enabled = false;
            SuspendProcess(processID);
            timerProgress.Enabled = true;
            int milliSeconds = 15000;
            if (!timer.Enabled)
            {
                progressBar.Value = 0;
                timer.Interval = milliSeconds / 100;
                timer.Enabled = true;
            }

            if (progressBar.Value == 100)
                ResumeProcess(processID);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (progressBar.Value < 100)
            {
                progressBar.Value += 1;
                progressBar.Refresh();
            }
            else
            {
                timer.Enabled = false;
            }
        }

        private void timerProgress_Tick(object sender, EventArgs e)
        {
            if (progressBar.Value == 100)
            {
                timerProgress.Enabled = false;
                kickButton.Enabled = true;
                ResumeProcess(processID);
                MessageBox.Show("Now you're your own in a public session.", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            ResumeProcess(processID);
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.instagram.com/samedzzzz/");
            System.Diagnostics.Process.Start("https://www.youtube.com/SamedZirhlioglu");
        }
    }
}

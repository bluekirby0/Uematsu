using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;


namespace Uematsu
{
    static class Program
    {
        private static Thread thCheckChars;
        private static List<Thread> m_CharThreads = new List<Thread>();
        private static Process[] pol;
        private static bool runThreads = true;
        internal static bool runCharThreads = true;

        private static void SpawnCharThreads()
        {
            runCharThreads = true;
            foreach(Process proc in pol)
            {
                ProcessChar thChar = new ProcessChar();
                thChar.pol = proc;
                Thread oThread = new Thread(new ThreadStart(thChar.BackgroundProcess));
                oThread.IsBackground = true;
                m_CharThreads.Add(oThread);
                oThread.Start();
            }
        }

        private static void KillCharThreads()
        {
            runCharThreads = false;
            foreach(Thread item in m_CharThreads)
            {
                item.Join();
            }
        }

        private static void CheckChars()
        {
            pol = Process.GetProcessesByName("pol");
            SpawnCharThreads();
            do
            {
                Process[] polTmp = Process.GetProcessesByName("pol");
                if (pol.Length != polTmp.Length)
                {
                    KillCharThreads();
                    SpawnCharThreads();
                }
                Thread.Sleep(500);
            } while (runThreads);
        }

        static void Main()
        {
            thCheckChars = new Thread(new ThreadStart(Program.CheckChars));
            thCheckChars.IsBackground = true;
            thCheckChars.Start();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Uematsu());
            
            runThreads = false;
            thCheckChars.Join();
            KillCharThreads();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using FFACETools;
using System.Xml;

namespace Uematsu
{
    static class Program
    {
        private static Thread thCheckChars;
        private static List<Thread> m_CharThreads = new List<Thread>();
        internal static Process[] pol;
        private static bool runThreads = true;
        internal static bool runCharThreads = true;
        internal static List<string> zoneList = new List<string>();
        internal static List<string> statusList = new List<string>();
        private static XmlReader trackXml = XmlReader.Create("TrackList.xml");
        internal static Dictionary<string,uint> trackList = new Dictionary<string,uint>();

        internal static Process selectedProcess;

        private static void populateResources()
        {
            foreach(Zone i in Enum.GetValues(typeof(Zone)))
                zoneList.Add(FFACE.ParseResources.GetAreaName(i));
            foreach (StatusEffect i in Enum.GetValues(typeof(StatusEffect)))
                statusList.Add(FFACE.ParseResources.GetStatusEffectName(i));
            while (trackXml.Read())
            {
                if (trackXml.IsStartElement() && trackXml.Name == "song")
                {
                    uint index = uint.MaxValue;
                    try{
                        index = Convert.ToUInt32(trackXml["index"]);
                    }
                    catch(FormatException e)
                    {
                        Console.WriteLine("Invalid Track Number: {0}",trackXml["index"]);
                    }
                    catch (OverflowException e)
                    {
                        Console.WriteLine("Track number too large: {0}",trackXml["index"]);
                    }
                    finally{
                        if (trackXml.Read() && index != uint.MaxValue)
                        {
                            trackList.Add(trackXml.Value.Trim(),index);
                        }
                    }
                }
            }
            
        }

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
            if (pol.Length != 0)
                SpawnCharThreads();
            do
            {
                Process[] polTmp = Process.GetProcessesByName("pol");
                if (pol.Length != polTmp.Length)
                {
                    KillCharThreads();
                    pol = polTmp;
                    if(polTmp.Length != 0)
                        SpawnCharThreads();
                }
                Thread.Sleep(500);
            } while (runThreads);
        }

        static void Main()
        {
            populateResources();
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

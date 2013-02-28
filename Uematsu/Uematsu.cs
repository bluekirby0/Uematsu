using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Uematsu
{
    public partial class Uematsu : Form
    {
        //TODO: Set up callback functions for form objects

        private void ResetProcessList()
        {
            this.comboSelectCharacterGlobal.Items.Clear();
            foreach (Process proc in Program.pol)
            {
                string item = proc.MainWindowTitle + " (" + proc.Id + ")";
                this.comboSelectCharacterGlobal.Items.Add(item);
            }
        }

        public Uematsu()
        {
            InitializeComponent();
            foreach (KeyValuePair<string, uint> i in Program.trackList)
            {
                this.comboZoneDaytimeMusic.Items.Add(i.Key);
                this.comboZoneNighttimeMusic.Items.Add(i.Key);
                this.comboZonePartyMusic.Items.Add(i.Key);
                this.comboZoneSoloMusic.Items.Add(i.Key);
                this.listEditTracks.Items.Add(i.Key);
                this.listSelectTrackAdv.Items.Add(i.Key);
                this.listSelectTrackEnemy.Items.Add(i.Key);
                this.listTrackBuff.Items.Add(i.Key);
                this.listSelectTrackBF.Items.Add(i.Key);
            }
            this.listSelectZoneZone.Items.AddRange(Program.zoneList.ToArray());
            this.listSelectZoneEnemy.Items.AddRange(Program.zoneList.ToArray());
            this.listZoneBF.Items.AddRange(Program.zoneList.ToArray());
            this.listZoneAdv.Items.AddRange(Program.zoneList.ToArray());
            this.listSelectBuffAdv.Items.AddRange(Program.statusList.ToArray());
            this.listSelectBuffBuff.Items.AddRange(Program.statusList.ToArray());
            
            ResetProcessList();
        }

        private void Uematsu_Load(object sender, EventArgs e)
        {

        }

        private void tabControlMain_Click(object sender, EventArgs e)
        {

        }

        private void comboSelectCharacterGlobal_DropDown(object sender, EventArgs e)
        {
            ResetProcessList();
        }

        private void comboSelectCharacterGlobal_SelectedValueChanged(object sender, EventArgs e)
        {
            string optString = Convert.ToString(sender);
            string option = optString.Split(' ')[optString.Split(' ').Length - 1];
            foreach (Process proc in Program.pol)
            {
                string item = "(" + proc.Id + ")";
                if (item == option)
                {
                    Program.selectedProcess = proc;
                }
            }
        }

    }
}

using System;
using System.Threading;
using System.Diagnostics;
using FFACETools;

namespace Uematsu
{
    public class ProcessChar
    {
        UInt16 currentTrack = 65535;

        internal Process pol;
        FFACE _FFACE { get; set; }

        string sCharName;
        Zone m_Zone;
        byte uVanaHour;
        Status m_Status;
        StatusEffect[] m_Buffs;
        bool bPartyMember;

        private bool CheckConditions()
        {
            bool execScripts = false;
            bool bPartyMemberTmp = false;
            
            if (_FFACE.Party.Party0Count == 1 && _FFACE.Party.Party1Count == 0 && _FFACE.Party.Party2Count == 0)
                bPartyMemberTmp = false;
            else
                bPartyMemberTmp = true;


            if (m_Zone != _FFACE.Player.Zone) // Zone Changed
            {
                m_Zone = _FFACE.Player.Zone;
                execScripts = true;
            }
            else if (uVanaHour != _FFACE.Timer.GetVanaTime().Hour) // Hour Changed
            {
                uVanaHour = _FFACE.Timer.GetVanaTime().Hour;
                execScripts = true;
            }
            else if (m_Status != _FFACE.Player.Status) // Combat? Status Changed
            {
                m_Status = _FFACE.Player.Status;
                execScripts = true;
            }
            else if (m_Buffs != _FFACE.Player.StatusEffects) // Buffs Changed
            {
                m_Buffs = _FFACE.Player.StatusEffects;
                execScripts = true;
            }
            else if (bPartyMember != bPartyMemberTmp) // Party Status Changed
            {
                bPartyMember = bPartyMemberTmp;
                execScripts = true;
            }

            return execScripts;
        }

        private UInt16 ExecScript()
        {
            //TODO: process the profile's script and return its value
            return 0;
        }
        public void BackgroundProcess()
        {
            _FFACE = new FFACE(pol.Id);
            sCharName = _FFACE.Player.Name;
            
            m_Zone = _FFACE.Player.Zone;
            uVanaHour = _FFACE.Timer.GetVanaTime().Hour;
            m_Status = _FFACE.Player.Status;
            m_Buffs = _FFACE.Player.StatusEffects;

            
            if (_FFACE.Party.Party0Count == 1 && _FFACE.Party.Party1Count == 0 && _FFACE.Party.Party2Count == 0)
                bPartyMember = false;
            else
                bPartyMember = true;

            do{
                if (CheckConditions())
                {
                    UInt16 thisTrack = ExecScript();
                    if (thisTrack != currentTrack)
                    {
                        currentTrack = thisTrack;
                        _FFACE.Windower.SendString("/vanatunes hold on");
                        _FFACE.Windower.SendString("/vanatunes playOverride " + currentTrack);
                    }
                }
                
                Thread.Sleep(500);
            } while(Program.runCharThreads);
        }

        public bool bPartyMemberTemp { get; set; }
    };
};

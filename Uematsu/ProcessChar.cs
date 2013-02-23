using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using FFACETools;
using LuaInterface;

namespace Uematsu
{
    public class ProcessChar
    {
        
        
        UInt16 currentTrack = 65535;

        internal Process pol;
        FFACE _FFACE { get; set; }


        Lua _LUA = new Lua();

        string sCharName;
        Zone m_Zone;
        byte uVanaHour;
        Status m_Status;
        bool bFighting;
        StatusEffect[] m_Buffs;
        bool bPartyMember;
        bool bInBattlefield;
        bool bInResidence;

        string profile;

        private bool CheckConditions()
        {
            bool execScripts = false;
            bool bPartyMemberTmp = false;
            bool bFightingTmp = false;
            bool bInBattlefieldTmp = false;
            bool bInResidenceTmp = false;

            if (_FFACE.Party.Party0Count == 1 && _FFACE.Party.Party1Count == 0 && _FFACE.Party.Party2Count == 0)
                bPartyMemberTmp = false;
            else
                bPartyMemberTmp = true;

            bInResidenceTmp = (_FFACE.NPC.Name(2048) == "Door: Back to Town");

            foreach (StatusEffect item in m_Buffs)
            {
                if (item == StatusEffect.Battlefield)
                    bInBattlefieldTmp = true;
            }

            if (m_Zone != _FFACE.Player.Zone) // Zone Changed
            {
                m_Zone = _FFACE.Player.Zone;
                execScripts = true;
            }
            
            if (uVanaHour != _FFACE.Timer.GetVanaTime().Hour) // Hour Changed
            {
                uVanaHour = _FFACE.Timer.GetVanaTime().Hour;
                execScripts = true;
            }
            
            if (m_Status != _FFACE.Player.Status) // Combat Status Changed
            {
                bFightingTmp = !(m_Status == Status.Fighting);
                if (bFighting != bFightingTmp)
                {
                    bFighting = bFightingTmp;
                    execScripts = true;
                }
                m_Status = _FFACE.Player.Status;
                
            }
            for (int i = 0; i < m_Buffs.Length; i++)
            {
                if (m_Buffs[i] != _FFACE.Player.StatusEffects[i]) // Buffs Changed
                {
                    m_Buffs = _FFACE.Player.StatusEffects;
                    execScripts = true;
                    break;
                }
                
            }
            
            if (bPartyMember != bPartyMemberTmp) // Party Status Changed
            {
                bPartyMember = bPartyMemberTmp;
                execScripts = true;
            }
            
            if (bInBattlefield != bInBattlefieldTmp) // Entered or left battlefield
            {
                bInBattlefield = bInBattlefieldTmp;
                execScripts = true;
            }

            if (bInResidence != bInResidenceTmp) // Entered or left residence
            {
                bInResidence = bInResidenceTmp;
                execScripts = true;
            }

            return execScripts;
        }

        private UInt16 ExecScript()
        {
            List<int> buffList = new List<int>();
            foreach (StatusEffect buff in m_Buffs)
            {
                buffList.Add((int)buff);
            }
            
            //TODO: Provide more vars to the Lua environment as needed
            _LUA["target"] = _FFACE.Target;
            _LUA["player"] = _FFACE.Player;
            _LUA["currentTrack"] = currentTrack;
            _LUA["charName"] = sCharName;
            _LUA["zone"] = (short)m_Zone;
            _LUA["status"] = (short)m_Status;
            _LUA["isFighting"] = bFighting;
            _LUA["targetMobID"] = _FFACE.Target.ServerID;
            _LUA["targetMobShort"] = _FFACE.Target.ID;
            _LUA["targetMobName"] = _FFACE.Target.Name;
            _LUA["buffs"] = buffList;
            _LUA["isInParty"] = bPartyMember;
            _LUA["isInBattlefield"] = bInBattlefield;
            _LUA["isInResidence"] = bInResidence;
            
            _LUA.DoFile(profile+".lua");
            return (ushort)Math.Floor((double)_LUA["currentTrack"]);
        }

        public void BackgroundProcess()
        {
            Thread.Sleep(5000);
            _FFACE = new FFACE(pol.Id);
            while (_FFACE.Player.Name == "")
                Thread.Sleep(500);
            
            _FFACE.Windower.SendString("/vanatunes hold on");
            sCharName = _FFACE.Player.Name;
            profile = "(none)";
            //TODO: Check player name and set default profile
            
            m_Zone = _FFACE.Player.Zone;
            uVanaHour = _FFACE.Timer.GetVanaTime().Hour;
            m_Status = _FFACE.Player.Status;
            m_Buffs = _FFACE.Player.StatusEffects;

            
            if (_FFACE.Party.Party0Count == 1 && _FFACE.Party.Party1Count == 0 && _FFACE.Party.Party2Count == 0)
                bPartyMember = false;
            else
                bPartyMember = true;

            bFighting = false;
            bInBattlefield = false;

            do{
                if (CheckConditions())
                {
                    UInt16 thisTrack = ExecScript();
                    if (thisTrack != currentTrack)
                    {
                        currentTrack = thisTrack;
                        _FFACE.Windower.SendString("/vanatunes playOverride " + currentTrack);
                    }
                }
                
                Thread.Sleep(500);
            } while(Program.runCharThreads);
            _FFACE.Windower.SendString("/vanatunes hold off");
        }
    };
};

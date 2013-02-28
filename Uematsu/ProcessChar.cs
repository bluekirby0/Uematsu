using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using FFACETools;
using LuaInterface;

namespace Uematsu
{
    public class ProcessChar
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, ref IntPtr lpNumberOfBytesRead);
        ProcessModule ffximain;
        byte[] mobFlags = new byte[4096*4];

        UInt16 currentTrack = 65535;

        internal Process pol;
        FFACE _FFACE { get; set; }

        Lua _Lua = new Lua();

        string sCharName;
        Zone m_Zone;
        byte uVanaHour;
        Status m_Status;
        bool bFighting;
        StatusEffect[] m_Buffs;
        bool bPartyMember;
        bool bInBattlefield;
        bool bInResidence;
        bool bPartyFighting = false;
        bool bPartyFightingNM;

        string profile;

        private bool MobPartyCheck()
        {
            bool bPartyFightingTmp = false;
            for (UInt16 i = 1; i < 4096; i++)
            {
                int thisMob = i;
                foreach (KeyValuePair<byte, FFACE.PartyMemberTools> member in _FFACE.PartyMember)
                {
                    if (_FFACE.NPC.IsActive(thisMob) && _FFACE.NPC.IsClaimed(i) && _FFACE.NPC.NPCType(i) == NPCType.Mob &&
                        !(_FFACE.NPC.Status(i) == Status.Dead1) && !(_FFACE.NPC.Status(i) == Status.Dead2) && !(_FFACE.NPC.Status(i) == Status.Standing))
                    {
                        if (_FFACE.NPC.ClaimedID(thisMob) == member.Value.ServerID)
                        {
                            if(((mobFlags[thisMob * 4]) & 4) != 0)
                                bPartyFightingNM = true;
                            bPartyFightingTmp = true;
                        }
                    }
                }
            }
            return bPartyFightingTmp;
        }
        private bool CheckConditions()
        {
            bool execScripts = false;
            bool bPartyMemberTmp = false;
            bool bFightingTmp = false;
            bool bInBattlefieldTmp = false;
            bool bInResidenceTmp = false;
            bool bPartyFightingTmp = false;
            bPartyFightingNM = false;
            
            if (_FFACE.Party.Party0Count == 1 && _FFACE.Party.Party1Count == 0 && _FFACE.Party.Party2Count == 0)
                bPartyMemberTmp = false;
            else
                bPartyMemberTmp = true;

            bPartyFightingTmp = MobPartyCheck();

            bInResidenceTmp = (_FFACE.NPC.Name(2048) == "Door: Back to Town");

            foreach (StatusEffect item in m_Buffs)
            {
                if (item == StatusEffect.Battlefield)
                    bInBattlefieldTmp = true;
            }

            if (m_Zone != _FFACE.Player.Zone) // Zone Changed
            {
                byte[] mobPtrTmp = new byte[4];
                IntPtr mobPtrArraySize = new IntPtr();
                ReadProcessMemory(pol.Handle, new IntPtr(ffximain.BaseAddress.ToInt32() + 0x8020c), mobPtrTmp, 4, ref mobPtrArraySize);
                IntPtr mobPtr = new IntPtr(BitConverter.ToInt32(mobPtrTmp, 0));
                ReadProcessMemory(pol.Handle, mobPtr, mobFlags, 4096*4, ref mobPtrArraySize);

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

            if (bPartyFighting != bPartyFightingTmp) //Party started or stopped combat
            {
                bPartyFighting = bPartyFightingTmp;
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
            _Lua["target"] = _FFACE.Target;
            _Lua["player"] = _FFACE.Player;
            _Lua["vanaHour"] = uVanaHour;
            _Lua["currentTrack"] = currentTrack;
            _Lua["charName"] = sCharName;
            _Lua["zone"] = (short)m_Zone;
            _Lua["status"] = (short)m_Status;
            _Lua["isFighting"] = bFighting;
            _Lua["isPartyFighting"] = bPartyFighting;
            _Lua["targetMobID"] = _FFACE.Target.ServerID;
            _Lua["targetMobShort"] = _FFACE.Target.ID;
            _Lua["targetMobName"] = _FFACE.Target.Name;
            _Lua["isNM"] = (bool)(((mobFlags[_FFACE.Target.ID * 4]) & 4) != 0 );
            _Lua["isPartyFightingNM"] = bPartyFightingNM;
            _Lua["buffs"] = buffList;
            _Lua["isInParty"] = bPartyMember;
            _Lua["isInBattlefield"] = bInBattlefield;
            _Lua["isInResidence"] = bInResidence;
            
            _Lua.DoFile(profile+".lua");
            return (ushort)Math.Floor((double)_Lua["currentTrack"]);
        }

        public void BackgroundProcess()
        {
            do
            {
                Thread.Sleep(500);
                ffximain = (from ProcessModule m in pol.Modules where m.ModuleName.ToLower() == "ffximain.dll" select m).FirstOrDefault();
            }
            while (ffximain == null);
                
            _FFACE = new FFACE(pol.Id);
            while (_FFACE.Player.Name == "")
                Thread.Sleep(500);
            
            _FFACE.Windower.SendString("/vanatunes hold on");
            sCharName = _FFACE.Player.Name;
            profile = "(none)";
            //TODO: Check player name and set default profile

            byte[] mobPtrTmp = new byte[4];
            IntPtr mobPtrArraySize = new IntPtr();
            ReadProcessMemory(pol.Handle, new IntPtr(ffximain.BaseAddress.ToInt32() + 0x8020c), mobPtrTmp, 4, ref mobPtrArraySize);
            IntPtr mobPtr = new IntPtr(BitConverter.ToInt32(mobPtrTmp, 0));
            ReadProcessMemory(pol.Handle, mobPtr, mobFlags, 4096*4, ref mobPtrArraySize);
            
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
            if(ffximain != null)
                _FFACE.Windower.SendString("/vanatunes hold off");
        }
    };
};

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uematsu
{
    public static class ScriptGen
    {
        enum time : byte
        {
            any     = 0,
            daytime = 1,
            night   = 2
        };

        enum combat : byte
        {
            any     = 0,
            solo    = 1,
            party   = 2
        };

        private static void appendRule(ref string rule, string appendText, ref bool prevRule)
        {
            if(prevRule)
                rule += " and";
            rule += appendText;
            prevRule = true;
        }

        public static string genAdvancedRule(uint zoneID, string mobName, uint mobID, 
            byte timeOfDay, byte combatStatus, uint buffID, uint trackID)
        {
            bool prevRule = false;
            string rule = "if(";

            if (zoneID != 0 && mobID < 4096) // Ignore zone if long ID is given since long ID is totally unique
                appendRule(ref rule," zone == " + zoneID, ref prevRule);

            if (mobID >= 4096) // Long ID
                appendRule(ref rule, " targetMobID == " + mobID, ref prevRule);
            else if (mobID < 4096) // Short ID
                appendRule(ref rule, " targetMobShort == " + mobID, ref prevRule);
            else if (mobName != "") // Only gets used if no ID is given since this is the least specific
                appendRule(ref rule," targetMobName == \"" + mobName + "\"", ref prevRule);

            if (timeOfDay != 0)
            {
                if(timeOfDay == (byte)time.daytime)
                    appendRule(ref rule, " (vanaHour < 18 and vanaHour >= 6)", ref prevRule);
                else if (timeOfDay == (byte)time.night)
                    appendRule(ref rule, " (vanaHour >= 18 or vanaHour < 6)", ref prevRule);
            }

            if (combatStatus != 0)
            {
                if (combatStatus == (byte)combat.solo)
                    appendRule(ref rule, " (isFighting and not isInParty)", ref prevRule);
                if (combatStatus == (byte)combat.party)
                    appendRule(ref rule, " (isPartyFighting and isInParty)", ref prevRule);
            }

            if (buffID != 0)
                appendRule(ref rule, " hasBuff(" + buffID + ")", ref prevRule);

            rule += " ) then currentTrack = " + trackID + " end;\n";
            
            return rule;
        }

        public static string genZoneRule(uint zoneID, uint trackDay, uint trackNight, uint trackSolo, uint trackParty)
        {
            string rule = "-- Zone Ruleset\n";
            rule += genAdvancedRule(zoneID, "", 0, (byte)time.daytime, 0, 0, trackDay);
            rule += genAdvancedRule(zoneID, "", 0, (byte)time.night, 0, 0, trackNight);
            rule += genAdvancedRule(zoneID, "", 0, 0, (byte)combat.solo, 0, trackSolo);
            rule += genAdvancedRule(zoneID, "", 0, 0, (byte)combat.party, 0, trackParty);
            rule += "-- End Ruleset\n\n";
            return rule;
        }

        public static string genBuffRule(uint buffID, uint trackID)
        {
            string rule = "-- Buff Ruleset\n";
            rule += genAdvancedRule(0, "", 0, 0, 0, buffID, trackID);
            rule += "-- End Ruleset\n\n";
            return rule;
        }

        public static string genEnemyRule(string mobName, uint mobID, uint trackID)
        {
            string rule = "-- Enemy Ruleset\n";
            rule += genAdvancedRule(0, mobName, mobID, 0, 0, 0, trackID);
            rule += "-- End Ruleset\n\n";
            return rule;
        }

        public static string genBattlefieldRule(uint zoneID, uint trackID)
        {
            string rule = "-- Battlefield Ruleset\n";
            rule += genAdvancedRule(zoneID, "", 0, 0, 0, 254, trackID);
            rule += "-- End Ruleset\n\n";
            return rule;
        }
    }
}

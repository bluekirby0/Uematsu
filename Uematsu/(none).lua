require("__utils");
require("__aliases");
require("__template");

--[[
Vars available to this Lua environment
currentTrack		Defines what track the game is playing right now. Changing this value will cause the track 
					to change.
charName			Current Character Name. Profiles can be auto-set based on character name, so this is usually 
					not needed.
vanaHour			Hour of the day in the game-world.
zone				ID of the current zone. Check __aliases.lua for some shortcuts to call these.
status				Status of your character. Can be used to check for just about anything from fighting to 
					fishing to riding a chocobo
isFighting			A boolean value that is a shortcut to status == 1
isPartyFighting		A smarter combat detection that will check if any mobs in range are currently claimed by a 
					party or alliance member.
targetMobName		Currently Targeted Mob's name. Should be combined with isFighting to prevent spastic track 
					changes.
targetMobID			Currently Targeted Mob's ID. Should be combined with isFighting to prevent spastic track 
					changes.
targetMobShort		Currently Targeted Mob's Local ID. Should be combined with isFighting to prevent spastic 
					track changes.
isNM				A boolean value that tells you if your current target is a NM. Not necessarily 100% accurate, 
					but it is reliable enough and doesn't depend on a database, so it won't become outdated.
isPartyFightingNM	A boolean value that combines the logic of isPartyFighting and isNM effectively.
buffs				A list type with all of the (de)buffs you have applied. Use hasBuff(buff) for easy checking.
isInParty			A boolean value that tells you if you are currently in a party.
isInBattlefield		A boolean value that is a shortcut to check if you have buff 254
isInResidence		A boolean value that specifies whether you are in your mog house or not

Users familiar with FFACETools can access the player and target objects directly using "player" and "target"
]]--
currentTrack = getDefaultZoneTrack(zone, isFighting, isInParty, isPartyFighting, isInResidence);
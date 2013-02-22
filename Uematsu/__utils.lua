function listToTable(clrlist)
    local t = {};
    local it = clrlist:GetEnumerator();
    while it:MoveNext() do
      t[#t+1] = it.Current;
    end
    return t;
end;

function hasBuff(buffID)
	local iterBuffs = listToTable(buffs);
	for key, val in pairs(iterBuffs) do
		if(val == buffID) then
			return true;
		end
	end
	return false;
end;

--function getStatus(player,status)
	
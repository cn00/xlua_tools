

local util = require("util")
local luasql = require "luasql.mysql"

-- banner
local function update_banner_url()
    local id_paths = {}
    util.GetFiles(
        "datafile/img/scout_banner"
        ,function ( fpath )
            local fsplit = util.split(fpath, '/')
            local id = fsplit[4]
            if id_paths[id] == nil then
                print(id, fpath)
                id_paths[id] = fpath
            end
        end
        ,"png|jpg|jpeg"
    )
    local source, user, pward, host = "a3_m_308", "a3", "654123", "10.23.22.233"
    local db = luasql.mysql():connect(source, user, pward, host)
    for k, v in pairs(id_paths) do
        local sql = string.format([[update a3_m_308.m_scout t set banner_url = "%s" where scout_id = %s;]], v, k)
        print(sql)
        local res, err = db:exec(sql)
    end
    db:close()
end
update_banner_url()

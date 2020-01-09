--
-- Created by IntelliJ IDEA.
-- User: cn
-- Date: 2019-12-27
-- Time: 10:23
-- To change this template use File | Settings | File Templates.
--

local CS = CS
local NPOI = CS.NPOI
local Workbook = NPOI.XSSF.UserModel.XSSFWorkbook


local wb = Workbook("m_event_0tmp.xlsx")
local sheet = wb['m_event_0tmp']

print ('wb='.. wb .. ";sheet=" .. sheet) 
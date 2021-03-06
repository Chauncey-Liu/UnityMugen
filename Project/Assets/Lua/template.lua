local trigger = require("trigger")
local _InitCommonCns = require("commonCns")

local setmetatable = setmetatable

local [替换] = {}
[替换].__index = [替换]

function [替换]:new()
	-- 静态数据
   if self._isInit == nil then
		self._isInit = true
		self:_initData()
		self:_initSize()
    end
   -- 动态数据
   local t = {PlayerDisplay = nil}
   local ret = setmetatable(t, [替换])
   --print(ret)
   return ret
end

--====================外部调用接口==============================

function [替换]:OnInit(playerDisplay)
	--print(playerDisplay)
	self.PlayerDisplay = playerDisplay;
	--print(self.PlayerDisplay)
	trigger:Help_InitLuaPlayer(self, self)
	-- 初始化默认Cns状态
	_InitCommonCns(self)
  self:_initCmds()
end

function [替换]:OnDestroy()
  self.PlayerDisplay = nil
  --print(null)
end

function [替换]:OnGetAICommandName(cmdName)
	return ""
end

--===========================================================

function [替换]:_initData()
  if self.Data ~= nil then
	return
  end
  self.Data = {};
  
  self.Data.life = 1000
  self.Data.Power = 3000
  self.Data.attack = 100
  self.Data.defence = 100
  
  
  self.Data.fall = {}
  self.Data.fall.defence_up = 50
  
  self.Data.liedown = {}
  self.Data.liedown.time = 60
  
  self.Data.airjuggle = 15
  self.Data.sparkno = 200
  
  self.Data.guard = {}
  self.Data.guard.sparkno = 40
  
  self.Data.KO = {}
  self.Data.KO.echo = 0
  
  self.Data.volume = 0
  self.Data.IntPersistIndex = 60
  self.Data.FloatPersistIndex = 40

  	self.velocity = {}
	self.velocity.run = {}
	self.velocity.run.fwd = Vector2.New(6.5, 0)
	self.velocity.run.back = Vector2.New(-6.5,-2.9)
end

function [替换]:_initSize()
  if self.Size ~= nil then
	return
  end
  self.Size = {}
  self.Size.xscale = 1
  self.Size.yscale = 1
end

function [替换]:_initCmds()
	local luaCfg = trigger:GetLuaCnsCfg("[替换]")
	if luaCfg == nil then
		return
	end
end

setmetatable([替换], {__call = [替换].new})

return [替换]



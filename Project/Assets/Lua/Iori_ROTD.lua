local trigger = require("trigger")

local setmetatable = setmetatable
local GlobaConfigMgr = MonoSingleton_GlobalConfigMgr.GetInstance()

local Iori_ROTD = {}
Iori_ROTD.__index = Iori_ROTD


function Iori_ROTD:new()
   -- 静态数据
   if self._isInit == nil then
		self._isInit = true
		self:_initData()
		self:_initSize()
		self:_initStateDefs()
		self:_initCmds()
    end
   -- 动态数据
   local t = {PlayerDisplay = nil}
   local ret = setmetatable(t, Iori_ROTD)
   --print(ret)
   return ret
end

--====================外部调用接口==============================

function Iori_ROTD:OnInit(playerDisplay)
	--print(playerDisplay)
	self.PlayerDisplay = playerDisplay;
	--print(self.PlayerDisplay)
	trigger:Help_InitLuaPlayer(self, self)
end

function Iori_ROTD:OnDestroy()
  self.PlayerDisplay = nil
  --print(null)
end

function Iori_ROTD:OnGetAICommandName(cmdName)
	
end

--===========================================================

function Iori_ROTD:_initData()
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
end

function Iori_ROTD:_initSize()
  if self.Size ~= nil then
	return
  end
  self.Size = {}
  self.Size.xscale = 1
  self.Size.yscale = 1
end

--=====================================创建StateDef===================================

--创建StateDef
function Iori_ROTD:_initStateDefs()
	local luaCfg = GlobaConfigMgr:GetLuaCnsCfg("Iori-ROTD")
	if luaCfg == nil then
		return
	end
	
	-- 创建各种状态
	self:_initStateDef_2000(luaCfg)
end

function Iori_ROTD:_initCmds()
	local luaCfg = GlobaConfigMgr:GetLuaCnsCfg("Iori-ROTD")
	if luaCfg == nil then
		return
	end
	
	--禁千弐百十壱式・八稚女
	self:_initCmd_SuperCode1(luaCfg)
end

--======================================================================================


--==禁千弐百十壱式・八稚女

function Iori_ROTD:_initStateDef_2000(luaCfg)
	local id = luaCfg:CreateStateDef("2000")
	local def = luaCfg:GetStateDef(id)
	def.Type = Mugen.Cns_Type.S
	def.MoveType = Mugen.Cns_MoveType.A
	def.PhysicsType = Mugen.Cns_PhysicsType.N
	def.Juggle = 4
	def.PowerAdd = 0
	def.Animate = 2000
	def.Ctrl = 0
	def.Sprpriority = 3
	def.Velset_x = 0
	def.Velset_y = 0
	
end

function Iori_ROTD:OnAICmd_SuperCode1(aiName)
	--print(self.PlayerDisplay)
	local triggerAll = (aiName == "禁千弐百十壱式・八稚女") and (trigger:Statetype(self) ~= Mugen.Cns_Type.A)
	triggerAll = triggerAll and (trigger:Power(self) >= 1000)
	local trigger1 = trigger:CanCtrl(self)
	local ret = triggerAll and trigger1
	--print(triggerAll)
	--print(trigger1)
	return ret
end

function Iori_ROTD:_initCmd_SuperCode1(luaCfg)
	local cmd = luaCfg:CreateCmd("禁千弐百十壱式・八稚女", "禁千弐百十壱式・八稚女")
	cmd.time = 30
	cmd:AttachKeyCommands("~D, DF, F, DF, D, DB, x")
	
	-- 创建状态
	local aiCmd = luaCfg:CreateAICmd("禁千弐百十壱式・八稚女")
	aiCmd.type = Mugen.AI_Type.ChangeState
	aiCmd.value = "2000"
	aiCmd.OnTriggerEvent = self.OnAICmd_SuperCode1
end

--==


setmetatable(Iori_ROTD, {__call = Iori_ROTD.new})
return Iori_ROTD
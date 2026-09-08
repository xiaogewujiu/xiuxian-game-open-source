<template>
  <div class="event-data-editor">
    <!-- Battle -->
    <template v-if="eventType === 1">
      <div class="editor-grid editor-grid--three">
        <label class="field">
          <span>怪物模板</span>
          <select v-model="data.monsterId">
            <option value="">未选择</option>
            <option v-for="m in monsterOptions" :key="m.value" :value="m.value">{{ m.label }}</option>
          </select>
        </label>
        <label class="field">
          <span>数量</span>
          <input v-model.number="battleQuantity" type="number" min="1" max="20" />
        </label>
        <label class="field checkbox-field">
          <input v-model="data.ambush" type="checkbox" /><span>伏击</span>
        </label>
        <label v-if="data.ambush" class="field">
          <span>伏击伤害(%)</span>
          <input v-model.trim="startDamageStr" type="text" placeholder="5%" />
        </label>
        <label class="field">
          <span>额外奖励事件ID</span>
          <input v-model.trim="data.extraReward" type="text" placeholder="可选" />
        </label>
      </div>
    </template>

    <!-- Heal -->
    <template v-else-if="eventType === 2">
      <div class="editor-grid editor-grid--three">
        <label class="field">
          <span>回复模式</span>
          <select v-model="healMode">
            <option value="percent">百分比恢复</option>
            <option value="item">给道具</option>
          </select>
        </label>
      </div>
      <div v-if="healMode === 'percent'" class="editor-grid editor-grid--three">
        <label class="field"><span>恢复百分比</span><input v-model.number="data.healPercent" type="number" min="1" max="100" /></label>
        <label class="field">
          <span>目标</span>
          <select v-model="data.target">
            <option value="hp">HP</option>
            <option value="mp">MP</option>
            <option value="both">HP+MP</option>
          </select>
        </label>
      </div>
      <div v-else class="editor-grid editor-grid--three">
        <label class="field">
          <span>道具</span>
          <select v-model="data.itemId">
            <option value="">未选择</option>
            <option v-for="it in itemOptions" :key="it.value" :value="it.value">{{ it.label }}</option>
          </select>
        </label>
        <label class="field"><span>数量</span><input v-model.trim="quantityStr" type="text" placeholder="1 或 1-3" /></label>
      </div>
    </template>

    <!-- Buff -->
    <template v-else-if="eventType === 3">
      <div class="editor-grid editor-grid--three">
        <label class="field">
          <span>Buff类型</span>
          <select v-model="data.buffType">
            <option v-for="t in BUFF_TYPES" :key="t.value" :value="t.value">{{ t.label }}</option>
          </select>
        </label>
        <label class="field"><span>数值</span><input v-model.number="data.value" type="number" /></label>
        <label class="field checkbox-field"><input v-model="data.isPercent" type="checkbox" /><span>百分比</span></label>
        <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
        <label class="field">
          <span>持续类型</span>
          <select v-model="data.durationType">
            <option value="battle">战斗(场次)</option>
            <option value="dungeon">整个秘境</option>
          </select>
        </label>
      </div>
    </template>

    <!-- Debuff -->
    <template v-else-if="eventType === 4">
      <div class="editor-grid editor-grid--three">
        <label class="field">
          <span>Debuff类型</span>
          <select v-model="data.debuffType">
            <option v-for="t in DEBUFF_TYPES" :key="t.value" :value="t.value">{{ t.label }}</option>
          </select>
        </label>
      </div>
      <!-- Stat debuffs -->
      <template v-if="isStatDebuff">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>数值</span><input v-model.number="data.value" type="number" /></label>
          <label class="field checkbox-field"><input v-model="data.isPercent" type="checkbox" /><span>百分比</span></label>
          <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="0" /></label>
          <label class="field">
            <span>持续类型</span>
            <select v-model="data.durationType">
              <option value="battle">战斗(场次)</option>
              <option value="tick">探索步数</option>
            </select>
          </label>
        </div>
      </template>
      <!-- DamageOverTime -->
      <template v-else-if="data.debuffType === 'DamageOverTime'">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>每Tick伤害</span><input v-model.number="data.value" type="number" /></label>
          <label class="field">
            <span>目标</span>
            <select v-model="data.target"><option value="hp">HP</option><option value="mp">MP</option></select>
          </label>
          <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
          <label class="field">
            <span>持续类型</span>
            <select v-model="data.durationType"><option value="battle">战斗(场次)</option><option value="tick">探索步数</option></select>
          </label>
        </div>
      </template>
      <!-- DamagePercent -->
      <template v-else-if="data.debuffType === 'DamagePercent'">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>伤害百分比</span><input v-model.number="data.value" type="number" /></label>
          <label class="field">
            <span>目标</span>
            <select v-model="data.target"><option value="hp">HP</option><option value="mp">MP</option></select>
          </label>
          <label class="field"><span>即时伤害</span><input v-model.trim="data.instantDamage" type="text" placeholder="可选, 如 10%" /></label>
        </div>
        <div class="editor-section">
          <label class="field checkbox-field"><input v-model="hasBonusBuff" type="checkbox" /><span>附带增益</span></label>
          <div v-if="hasBonusBuff" class="editor-grid editor-grid--three">
            <label class="field">
              <span>增益类型</span>
              <select v-model="bonusBuff.buffType"><option v-for="t in BUFF_TYPES" :key="t.value" :value="t.value">{{ t.label }}</option></select>
            </label>
            <label class="field"><span>增益数值</span><input v-model.number="bonusBuff.value" type="number" /></label>
            <label class="field"><span>增益持续</span><input v-model.number="bonusBuff.duration" type="number" min="1" /></label>
          </div>
        </div>
      </template>
      <!-- Crowd control: Silence/Stun/HealBlock -->
      <template v-else-if="['Silence','Stun','HealBlock'].includes(data.debuffType)">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
          <label class="field">
            <span>持续类型</span>
            <select v-model="data.durationType"><option value="battle">战斗(场次)</option><option value="tick">探索步数</option></select>
          </label>
          <label v-if="data.debuffType === 'Stun'" class="field">
            <span>眩晕伤害</span>
            <input v-model.trim="data.instantDamage" type="text" placeholder="可选, 如 10%" />
          </label>
        </div>
      </template>
      <!-- WeightModifier -->
      <template v-else-if="data.debuffType === 'WeightModifier'">
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>目标类型(数字)</span><input v-model.number="data.targetType" type="number" /></label>
          <label class="field"><span>倍率</span><input v-model.number="data.multiplier" type="number" /></label>
          <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
          <label class="field">
            <span>持续类型</span>
            <select v-model="data.durationType"><option value="battle">战斗(场次)</option><option value="tick">探索步数</option></select>
          </label>
        </div>
      </template>
    </template>

    <!-- Resource -->
    <template v-else-if="eventType === 5">
      <div class="editor-grid editor-grid--three">
        <label class="field">
          <span>奖励类型</span>
          <select v-model="data.rewardType">
            <option v-for="t in RESOURCE_TYPES" :key="t.value" :value="t.value">{{ t.label }}</option>
          </select>
        </label>
      </div>
      <div v-if="['Gold','SpiritStone','Exp'].includes(data.rewardType)" class="editor-grid editor-grid--three">
        <label class="field"><span>数量</span><input v-model.trim="amountStr" type="text" placeholder="100 或 100-500" /></label>
      </div>
      <div v-else-if="data.rewardType === 'Item'" class="editor-grid editor-grid--three">
        <label class="field">
          <span>道具</span>
          <select v-model="data.itemId">
            <option value="">未选择</option>
            <option v-for="it in itemOptions" :key="it.value" :value="it.value">{{ it.label }}</option>
          </select>
        </label>
        <label class="field"><span>数量</span><input v-model.trim="quantityStr" type="text" placeholder="1 或 1-3" /></label>
      </div>
      <div v-else-if="data.rewardType === 'Equipment'" class="editor-grid editor-grid--three">
        <label class="field">
          <span>装备模板</span>
          <select v-model="data.equipmentId">
            <option value="">随机装备</option>
            <option v-for="eq in equipmentOptions" :key="eq.value" :value="eq.value">{{ eq.label }}</option>
          </select>
        </label>
        <label v-if="!data.equipmentId" class="field"><span>品质范围</span><input v-model.trim="qualityStr" type="text" placeholder="1-2 或 3" /></label>
      </div>
      <div v-else-if="data.rewardType === 'Collection'" class="editor-grid editor-grid--three">
        <label class="field">
          <span>图鉴系列</span>
          <select v-model="data.collectionSeriesId">
            <option value="">未选择</option>
            <option v-for="s in collectionSeriesOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
          </select>
        </label>
      </div>
    </template>

    <!-- Treasure -->
    <template v-else-if="eventType === 6">
      <div class="editor-grid editor-grid--three">
        <label class="field">
          <span>宝箱模式</span>
          <select v-model="treasureMode">
            <option value="normal">普通宝箱</option>
            <option value="trap">陷阱宝箱</option>
            <option value="locked">上锁宝箱</option>
          </select>
        </label>
      </div>
      <div v-if="treasureMode === 'trap'" class="editor-grid editor-grid--three">
        <label class="field">
          <span>怪物模板</span>
          <select v-model="data.monsterId">
            <option value="">未选择</option>
            <option v-for="m in monsterOptions" :key="m.value" :value="m.value">{{ m.label }}</option>
          </select>
        </label>
      </div>
      <div v-if="treasureMode === 'locked'" class="editor-grid editor-grid--three">
        <label class="field">
          <span>需求钥匙</span>
          <select v-model="data.requireItem">
            <option value="">未选择</option>
            <option v-for="it in itemOptions" :key="it.value" :value="it.value">{{ it.label }}</option>
          </select>
        </label>
      </div>
      <div v-if="treasureMode !== 'trap'" class="editor-section">
        <h4 class="editor-section__title">奖励列表</h4>
        <div v-for="(r, i) in rewardsList" :key="i" style="display:flex;gap:8px;align-items:center;margin-bottom:6px;flex-wrap:wrap;">
          <select v-model="r.type" style="width:120px">
            <option value="Gold">金币</option>
            <option value="SpiritStone">灵石</option>
            <option value="Equipment">装备</option>
            <option value="Item">道具</option>
            <option value="Collection">图鉴</option>
          </select>
          <input v-if="r.type === 'Gold' || r.type === 'SpiritStone'" v-model.trim="r.amount" type="text" placeholder="200-500" style="width:120px" />
          <select v-if="r.type === 'Equipment'" v-model="r.equipmentId" style="width:160px">
            <option value="">随机装备</option>
            <option v-for="eq in equipmentOptions" :key="eq.value" :value="eq.value">{{ eq.label }}</option>
          </select>
          <input v-if="r.type === 'Equipment' && !r.equipmentId" v-model.trim="r.quality" type="text" placeholder="品质 1-2" style="width:100px" />
          <select v-if="r.type === 'Item'" v-model="r.id" style="width:160px">
            <option value="">未选择</option>
            <option v-for="it in treasureItemOptions" :key="it.value" :value="it.value">{{ it.label }}</option>
          </select>
          <input v-if="r.type === 'Item'" v-model.number="r.count" type="number" min="1" placeholder="数量" style="width:80px" />
          <select v-if="r.type === 'Collection'" v-model="r.collectionSeriesId" style="width:160px">
            <option value="">未选择</option>
            <option v-for="s in collectionSeriesOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
          </select>
          <button class="danger-button" style="padding:2px 8px;font-size:12px" type="button" @click="rewardsList.splice(i, 1)">删</button>
        </div>
        <button class="secondary-button" style="font-size:12px" type="button" @click="rewardsList.push({ type: 'Gold', amount: '100-500' })">+ 添加奖励</button>
      </div>
    </template>

    <!-- Adventure -->
    <template v-else-if="eventType === 7">
      <div class="editor-grid editor-grid--three">
        <label class="field">
          <span>奇遇类型</span>
          <select v-model="data.adventureType">
            <option v-for="t in ADVENTURE_TYPES" :key="t.value" :value="t.value">{{ t.label }}</option>
          </select>
        </label>
      </div>
      <!-- randomBuff -->
      <div v-if="data.adventureType === 'randomBuff'" class="editor-grid editor-grid--three">
        <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
        <label class="field">
          <span>持续类型</span>
          <select v-model="data.durationType"><option value="battle">战斗(场次)</option><option value="tick">探索步数</option></select>
        </label>
        <div class="field-note">从增益池中随机选择一个增益效果。</div>
      </div>
      <!-- grantExp -->
      <div v-else-if="data.adventureType === 'grantExp'" class="editor-grid editor-grid--three">
        <label class="field"><span>经验数量</span><input v-model.trim="amountStr" type="text" placeholder="5000-10000" /></label>
      </div>
      <!-- grantItem -->
      <div v-else-if="data.adventureType === 'grantItem'" class="editor-grid editor-grid--three">
        <label class="field">
          <span>道具</span>
          <select v-model="data.itemId">
            <option value="">未选择</option>
            <option v-for="it in itemOptions" :key="it.value" :value="it.value">{{ it.label }}</option>
          </select>
        </label>
        <label class="field"><span>数量</span><input v-model.trim="quantityStr" type="text" placeholder="1 或 1-3" /></label>
      </div>
      <!-- permanentBuff -->
      <div v-else-if="data.adventureType === 'permanentBuff'" class="editor-grid editor-grid--three">
        <label class="field"><span>Buff类型</span><select v-model="data.buffType"><option v-for="t in BUFF_TYPES" :key="t.value" :value="t.value">{{ t.label }}</option></select></label>
        <label class="field"><span>数值</span><input v-model.number="data.value" type="number" /></label>
        <label class="field checkbox-field"><input v-model="data.isPercent" type="checkbox" /><span>百分比</span></label>
        <div class="field-note">整个秘境持续有效。</div>
      </div>
      <!-- riddle -->
      <div v-else-if="data.adventureType === 'riddle'" class="editor-grid editor-grid--three">
        <label class="field"><span>成功率(%)</span><input v-model.number="data.successRate" type="number" min="0" max="100" /></label>
      </div>
      <!-- choice -->
      <div v-else-if="data.adventureType === 'choice'" class="field-note">
        自动随机选择一个选项执行。选项内容在事件描述中说明。
      </div>
      <!-- sacrifice -->
      <div v-else-if="data.adventureType === 'sacrifice'" class="editor-grid editor-grid--three">
        <label class="field"><span>HP消耗</span><input v-model.trim="data.hpCost" type="text" placeholder="20%" /></label>
      </div>
      <!-- purgeDebuff -->
      <div v-else-if="data.adventureType === 'purgeDebuff'" class="field-note">
        清除所有减益效果，无需额外配置。
      </div>
      <!-- fullRestore -->
      <div v-else-if="data.adventureType === 'fullRestore'" class="field-note">
        HP和MP完全恢复，无需额外配置。
      </div>
      <!-- risk -->
      <div v-else-if="data.adventureType === 'risk'" class="editor-grid editor-grid--three">
        <label class="field"><span>HP消耗</span><input v-model.trim="data.hpCost" type="text" placeholder="30%" /></label>
        <label class="field"><span>胜率(%)</span><input v-model.number="data.winRate" type="number" min="0" max="100" /></label>
      </div>
      <!-- gamble -->
      <div v-else-if="data.adventureType === 'gamble'" class="editor-grid editor-grid--three">
        <label class="field"><span>成本类型</span><select v-model="data.costType"><option value="Gold">金币</option></select></label>
        <label class="field"><span>成本数量</span><input v-model.number="data.costAmount" type="number" min="0" /></label>
        <label class="field"><span>胜率(%)</span><input v-model.number="data.winRate" type="number" min="0" max="100" /></label>
      </div>
      <!-- wheel -->
      <div v-else-if="data.adventureType === 'wheel'" class="editor-section">
        <h4 class="editor-section__title">转盘奖池</h4>
        <div v-for="(r, i) in wheelRewards" :key="i" style="display:flex;gap:8px;align-items:center;margin-bottom:6px;flex-wrap:wrap;">
          <select v-model="r.type" style="width:100px">
            <option value="Gold">金币</option><option value="SpiritStone">灵石</option>
            <option value="Item">道具</option><option value="Equipment">装备</option>
          </select>
          <input v-if="r.type === 'Gold' || r.type === 'SpiritStone'" v-model.trim="r.amount" type="text" placeholder="数量" style="width:100px" />
          <select v-if="r.type === 'Item'" v-model="r.itemId" style="width:160px">
            <option value="">未选择</option>
            <option v-for="it in itemOptions" :key="it.value" :value="it.value">{{ it.label }}</option>
          </select>
          <select v-if="r.type === 'Equipment'" v-model="r.equipmentId" style="width:160px">
            <option value="">未选择</option>
            <option v-for="eq in equipmentOptions" :key="eq.value" :value="eq.value">{{ eq.label }}</option>
          </select>
          <input v-model.number="r.weight" type="number" min="1" placeholder="权重" style="width:80px" />
          <button class="danger-button" style="padding:2px 8px;font-size:12px" type="button" @click="wheelRewards.splice(i, 1)">删</button>
        </div>
        <button class="secondary-button" style="font-size:12px" type="button" @click="wheelRewards.push({ type: 'Gold', amount: '1000', weight: 50 })">+ 添加奖励</button>
      </div>
      <!-- rest -->
      <div v-else-if="data.adventureType === 'rest'" class="editor-grid editor-grid--three">
        <label class="field"><span>HP恢复%</span><input v-model.number="data.healHpPercent" type="number" min="0" max="100" /></label>
        <label class="field"><span>MP恢复%</span><input v-model.number="data.healMpPercent" type="number" min="0" max="100" /></label>
      </div>
      <!-- fake -->
      <div v-else-if="data.adventureType === 'fake'" class="editor-grid editor-grid--three">
        <label class="field"><span>真实事件ID</span><input v-model.trim="data.realEvent" type="text" /></label>
        <div class="field-note">玩家看到事件描述，实际触发配置的真实事件。</div>
      </div>
      <!-- treasureChest -->
      <div v-else-if="data.adventureType === 'treasureChest'" class="editor-grid editor-grid--three">
        <label class="field"><span>数量</span><input v-model.trim="quantityStr" type="text" placeholder="1" /></label>
        <div class="field-note">从道具箱类型中随机选择一个宝箱道具。</div>
      </div>
      <!-- lore -->
      <div v-else-if="data.adventureType === 'lore'" class="field-note">
        纯叙事事件，只显示事件描述文本。
      </div>
    </template>

    <!-- Shop (统一神秘商人) -->
    <template v-else-if="eventType === 8">
      <!-- 商品列表 -->
      <div class="editor-section">
        <h4 class="editor-section__title">商品列表</h4>
        <div v-for="(item, i) in shopItems" :key="i" style="display:flex;gap:8px;align-items:center;margin-bottom:6px;flex-wrap:wrap;">
          <select v-model="item.itemType" style="width:90px">
            <option value="item">道具</option>
            <option value="equip">装备</option>
            <option value="collection">图鉴</option>
          </select>
          <select v-if="item.itemType === 'item'" v-model="item.itemId" style="width:160px">
            <option value="">未选择</option>
            <option v-for="it in itemOptions" :key="it.value" :value="it.value">{{ it.label }}</option>
          </select>
          <select v-if="item.itemType === 'equip'" v-model="item.equipId" style="width:160px">
            <option value="">未选择</option>
            <option v-for="eq in equipmentOptions" :key="eq.value" :value="eq.value">{{ eq.label }}</option>
          </select>
          <select v-if="item.itemType === 'collection'" v-model="item.seriesId" style="width:160px">
            <option value="">未选择</option>
            <option v-for="s in collectionSeriesOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
          </select>
          <input v-model.number="item.price" type="number" min="0" placeholder="价格" style="width:100px" />
          <input v-model.number="item.weight" type="number" min="1" placeholder="权重" style="width:80px" />
          <button class="danger-button" style="padding:2px 8px;font-size:12px" type="button" @click="shopItems.splice(i, 1)">删</button>
        </div>
        <button class="secondary-button" style="font-size:12px" type="button" @click="shopItems.push({ itemType: 'item', itemId: '', equipId: '', seriesId: '', price: 500, weight: 30 })">+ 添加商品</button>
      </div>
      <!-- 治疗服务 -->
      <div class="editor-section">
        <h4 class="editor-section__title">治疗服务</h4>
        <div class="editor-grid editor-grid--three">
          <label class="field"><span>HP恢复/金币</span><input v-model.number="shopHealing.hpPerGold" type="number" min="0" /></label>
          <label class="field"><span>MP恢复/金币</span><input v-model.number="shopHealing.mpPerGold" type="number" min="0" /></label>
          <label class="field"><span>权重</span><input v-model.number="shopHealing.weight" type="number" min="1" /></label>
        </div>
      </div>
      <!-- 强化服务 -->
      <div class="editor-section">
        <h4 class="editor-section__title">强化服务</h4>
        <div class="editor-grid editor-grid--three">
          <label class="field">
            <span>强化类型</span>
            <select v-model="shopEnhance.type">
              <option value="attack">攻击</option>
              <option value="defense">防御</option>
              <option value="speed">速度</option>
              <option value="crit">暴击</option>
            </select>
          </label>
          <label class="field"><span>强化数值</span><input v-model.number="shopEnhance.value" type="number" min="1" /></label>
          <label class="field"><span>费用</span><input v-model.number="shopEnhance.cost" type="number" min="0" /></label>
          <label class="field"><span>权重</span><input v-model.number="shopEnhance.weight" type="number" min="1" /></label>
        </div>
      </div>
    </template>

    <!-- Event (Special) -->
    <template v-else-if="eventType === 9">
      <div class="editor-grid editor-grid--three">
        <label class="field">
          <span>效果类型</span>
          <select v-model="data.effectType">
            <option v-for="t in EFFECT_TYPES" :key="t.value" :value="t.value">{{ t.label }}</option>
          </select>
        </label>
      </div>
      <!-- No-field effects -->
      <div v-if="['skipTick','purgeDebuffs','hpMpSwap'].includes(data.effectType)" class="field-note">
        此效果无需额外参数。
      </div>
      <!-- value + duration effects -->
      <div v-if="['rewardMultiplier','monsterBuff','rareBoost','trapBoost'].includes(data.effectType)" class="editor-grid editor-grid--three">
        <label class="field"><span>数值</span><input v-model.number="data.value" type="number" /></label>
        <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
        <label class="field"><span>持续类型</span><select v-model="data.durationType"><option value="tick">探索步数</option><option value="battle">战斗(场次)</option></select></label>
      </div>
      <!-- regenTick -->
      <div v-if="data.effectType === 'regenTick'" class="editor-grid editor-grid--three">
        <label class="field"><span>HP恢复%</span><input v-model.number="data.healHpPercent" type="number" /></label>
        <label class="field"><span>MP恢复%</span><input v-model.number="data.healMpPercent" type="number" /></label>
        <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
        <label class="field"><span>持续类型</span><select v-model="data.durationType"><option value="tick">探索步数</option><option value="battle">战斗(场次)</option></select></label>
      </div>
      <!-- buffMultiplier -->
      <div v-if="data.effectType === 'buffMultiplier'" class="editor-grid editor-grid--three">
        <label class="field"><span>Buff分类</span><input v-model.trim="data.buffCategory" type="text" placeholder="attack" /></label>
        <label class="field"><span>倍率</span><input v-model.number="data.value" type="number" /></label>
        <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
        <label class="field"><span>持续类型</span><select v-model="data.durationType"><option value="tick">探索步数</option><option value="battle">战斗(场次)</option></select></label>
      </div>
      <!-- weightModifier -->
      <div v-if="data.effectType === 'weightModifier'" class="editor-grid editor-grid--three">
        <label class="field"><span>倍率</span><input v-model.number="data.multiplier" type="number" /></label>
        <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
        <label class="field"><span>持续类型</span><select v-model="data.durationType"><option value="tick">探索步数</option><option value="battle">战斗(场次)</option></select></label>
      </div>
      <div v-if="data.effectType === 'weightModifier'" class="editor-section">
        <h4 class="editor-section__title">提升的事件类型ID</h4>
        <div v-for="(bt, i) in boostTypesList" :key="i" style="display:flex;gap:8px;align-items:center;margin-bottom:6px;">
          <select v-model.number="boostTypesList[i]" style="width:160px">
            <option :value="1">战斗(1)</option>
            <option :value="2">回复(2)</option>
            <option :value="3">增益(3)</option>
            <option :value="4">减益(4)</option>
            <option :value="5">资源(5)</option>
            <option :value="6">宝箱(6)</option>
            <option :value="7">奇遇(7)</option>
          </select>
          <button class="danger-button" style="padding:2px 8px;font-size:12px" type="button" @click="boostTypesList.splice(i, 1)">删</button>
        </div>
        <button class="secondary-button" style="font-size:12px" type="button" @click="boostTypesList.push(1)">+ 添加类型</button>
      </div>
      <!-- instantHeal -->
      <div v-if="data.effectType === 'instantHeal'" class="editor-grid editor-grid--three">
        <label class="field"><span>HP恢复%</span><input v-model.number="data.healHpPercent" type="number" /></label>
        <label class="field"><span>MP恢复%</span><input v-model.number="data.healMpPercent" type="number" /></label>
      </div>
      <!-- instantBuff -->
      <div v-if="data.effectType === 'instantBuff'" class="editor-grid editor-grid--three">
        <label class="field"><span>Buff类型</span><select v-model="data.buffType"><option v-for="t in BUFF_TYPES" :key="t.value" :value="t.value">{{ t.label }}</option></select></label>
        <label class="field"><span>数值</span><input v-model.number="data.value" type="number" /></label>
        <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
        <label class="field"><span>持续类型</span><select v-model="data.durationType"><option value="tick">探索步数</option><option value="battle">战斗(场次)</option></select></label>
      </div>
      <!-- mystery -->
      <div v-if="data.effectType === 'mystery'" class="editor-section">
        <h4 class="editor-section__title">随机选项池</h4>
        <div v-for="(opt, i) in mysteryOptionsList" :key="i" style="display:flex;gap:8px;align-items:center;margin-bottom:6px;">
          <select v-model="mysteryOptionsList[i]" style="width:160px">
            <option value="fullRestore">完全恢复</option>
            <option value="allStatsBuff">全属性增益</option>
            <option value="forceBattle">强制战斗</option>
            <option value="noEffect">无效果</option>
          </select>
          <input v-model.number="mysteryWeightsList[i]" type="number" min="1" placeholder="权重" style="width:80px" />
          <button class="danger-button" style="padding:2px 8px;font-size:12px" type="button" @click="mysteryOptionsList.splice(i, 1); mysteryWeightsList.splice(i, 1)">删</button>
        </div>
        <button class="secondary-button" style="font-size:12px" type="button" @click="mysteryOptionsList.push('fullRestore'); mysteryWeightsList.push(1)">+ 添加选项</button>
      </div>
      <!-- randomEvent -->
      <div v-if="data.effectType === 'randomEvent'" class="editor-section">
        <h4 class="editor-section__title">随机事件类型</h4>
        <div v-for="(et, i) in randomEventTypesList" :key="i" style="display:flex;gap:8px;align-items:center;margin-bottom:6px;">
          <select v-model.number="randomEventTypesList[i]" style="width:160px">
            <option :value="1">战斗(1)</option>
            <option :value="2">回复(2)</option>
            <option :value="3">增益(3)</option>
            <option :value="4">减益(4)</option>
            <option :value="5">资源(5)</option>
            <option :value="6">宝箱(6)</option>
            <option :value="7">奇遇(7)</option>
          </select>
          <input v-model.number="randomEventWeightsList[i]" type="number" min="1" placeholder="权重" style="width:80px" />
          <button class="danger-button" style="padding:2px 8px;font-size:12px" type="button" @click="randomEventTypesList.splice(i, 1); randomEventWeightsList.splice(i, 1)">删</button>
        </div>
        <button class="secondary-button" style="font-size:12px" type="button" @click="randomEventTypesList.push(1); randomEventWeightsList.push(1)">+ 添加类型</button>
      </div>
      <!-- extendBuffs -->
      <div v-if="data.effectType === 'extendBuffs'" class="editor-grid editor-grid--three">
        <label class="field"><span>延长Tick数</span><input v-model.number="data.duration" type="number" min="1" /></label>
      </div>
      <!-- summonClone -->
      <div v-if="data.effectType === 'summonClone'" class="editor-grid editor-grid--three">
        <label class="field"><span>攻击%</span><input v-model.number="data.attackPercent" type="number" /></label>
        <label class="field"><span>HP%</span><input v-model.number="data.hpPercent" type="number" /></label>
        <label class="field"><span>持续时间</span><input v-model.number="data.duration" type="number" min="1" /></label>
        <label class="field"><span>持续类型</span><select v-model="data.durationType"><option value="tick">探索步数</option><option value="battle">战斗(场次)</option></select></label>
      </div>
      <!-- reviveOnce -->
      <div v-if="data.effectType === 'reviveOnce'" class="editor-grid editor-grid--three">
        <label class="field"><span>复活HP%</span><input v-model.number="data.healPercent" type="number" min="1" max="100" /></label>
      </div>
    </template>

    <!-- Empty -->
    <template v-else-if="eventType === 10">
      <label class="field checkbox-field"><input v-model="hasHiddenBuff" type="checkbox" /><span>隐藏增益</span></label>
      <div v-if="hasHiddenBuff" class="editor-grid editor-grid--three">
        <label class="field"><span>Buff类型</span><select v-model="hiddenBuff.buffType"><option v-for="t in BUFF_TYPES" :key="t.value" :value="t.value">{{ t.label }}</option></select></label>
        <label class="field"><span>数值</span><input v-model.number="hiddenBuff.value" type="number" /></label>
        <label class="field"><span>持续时间</span><input v-model.number="hiddenBuff.duration" type="number" min="1" /></label>
        <label class="field"><span>持续类型</span><select v-model="hiddenBuff.durationType"><option value="battle">战斗(场次)</option><option value="dungeon">整个秘境</option></select></label>
      </div>
    </template>

    <!-- PlayerEncounter -->
    <template v-else-if="eventType === 11">
      <div class="field-note">
        随机匹配同秘境中的其他玩家，根据好感度判定组队或PvP。无需额外配置。
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'

const props = defineProps<{
  modelValue?: string | null
  eventType: number
  monsterOptions?: { value: string; label: string }[]
  itemOptions?: { value: string; label: string }[]
  chestItemOptions?: { value: string; label: string }[]
  equipmentOptions?: { value: string; label: string }[]
  collectionSeriesOptions?: { value: string; label: string }[]
}>()

const emit = defineEmits<{ 'update:modelValue': [value: string | null | undefined] }>()

// ---- Constants ----
const BUFF_TYPES = [
  { value: 'AttackUp', label: '攻击力提升' },
  { value: 'DefenseUp', label: '防御力提升' },
  { value: 'CritRateUp', label: '暴击率提升' },
  { value: 'SpeedUp', label: '速度提升' },
  { value: 'ComboRateUp', label: '连击率提升' },
  { value: 'CounterRateUp', label: '反击率提升' },
  { value: 'DodgeRateUp', label: '闪避率提升' },
  { value: 'HitRateUp', label: '命中率提升' },
  { value: 'Lifesteal', label: '吸血' },
  { value: 'Shield', label: '护盾' },
  { value: 'HealOverTime', label: '持续回复' },
  { value: 'ManaRegen', label: '灵力回复' },
  { value: 'Undying', label: '不屈' },
  { value: 'ElementBonus', label: '五行克制增伤' },
  { value: 'AllStatsUp', label: '全属性提升' }
]

const DEBUFF_TYPES = [
  { value: 'DamagePercent', label: '百分比伤害' },
  { value: 'DamageOverTime', label: '持续伤害' },
  { value: 'AttackDown', label: '攻击力降低' },
  { value: 'DefenseDown', label: '防御力降低' },
  { value: 'SpeedDown', label: '速度降低' },
  { value: 'CritRateDown', label: '暴击率降低' },
  { value: 'Silence', label: '沉默' },
  { value: 'Stun', label: '眩晕' },
  { value: 'HealBlock', label: '禁疗' },
  { value: 'AllStatsDown', label: '全属性降低' },
  { value: 'WeightModifier', label: '权重修改' }
]

const RESOURCE_TYPES = [
  { value: 'Gold', label: '金币' }, { value: 'SpiritStone', label: '灵石' },
  { value: 'Exp', label: '经验' }, { value: 'Item', label: '道具' },
  { value: 'Equipment', label: '装备' }, { value: 'Collection', label: '图鉴' }
]

const ADVENTURE_TYPES = [
  { value: 'randomBuff', label: '随机增益' },
  { value: 'grantExp', label: '给经验' }, { value: 'grantItem', label: '给道具' },
  { value: 'permanentBuff', label: '永久增益' }, { value: 'riddle', label: '谜题' },
  { value: 'choice', label: '选择' }, { value: 'sacrifice', label: '献祭' },
  { value: 'purgeDebuff', label: '净化' },
  { value: 'fullRestore', label: '完全恢复' }, { value: 'risk', label: '风险' },
  { value: 'gamble', label: '赌博' }, { value: 'wheel', label: '转盘' },
  { value: 'rest', label: '休息' }, { value: 'fake', label: '假事件' },
  { value: 'lore', label: '传说' }, { value: 'treasureChest', label: '宝箱' }
]

const EFFECT_TYPES = [
  { value: 'rewardMultiplier', label: '奖励倍率' }, { value: 'monsterBuff', label: '怪物增益' },
  { value: 'rareBoost', label: '稀有提升' }, { value: 'trapBoost', label: '陷阱提升' },
  { value: 'regenTick', label: '持续恢复' }, { value: 'buffMultiplier', label: '增益倍率' },
  { value: 'skipTick', label: '跳过Tick' }, { value: 'weightModifier', label: '权重修改' },
  { value: 'instantHeal', label: '即时恢复' }, { value: 'instantBuff', label: '即时增益' },
  { value: 'mystery', label: '神秘事件' }, { value: 'randomEvent', label: '随机事件' },
  { value: 'extendBuffs', label: '延长增益' }, { value: 'purgeDebuffs', label: '净化减益' },
  { value: 'hpMpSwap', label: 'HP/MP互换' }, { value: 'summonClone', label: '召唤分身' },
  { value: 'reviveOnce', label: '复活一次' }
]

// ---- Local reactive data ----
const data = ref<Record<string, any>>({})
const battleQuantity = ref(1)
const healMode = ref<'percent' | 'item'>('percent')
const treasureMode = ref<'normal' | 'trap' | 'locked'>('normal')
const rewardsList = ref<any[]>([])
const boostTypesList = ref<number[]>([])
const mysteryOptionsList = ref<string[]>([])
const mysteryWeightsList = ref<number[]>([])
const randomEventTypesList = ref<number[]>([])
const randomEventWeightsList = ref<number[]>([])
const hasHiddenBuff = ref(false)
const hiddenBuff = ref({ buffType: 'AttackUp', value: 1, duration: 1, durationType: 'battle' })
const hasBonusBuff = ref(false)
const bonusBuff = ref({ buffType: 'AttackUp', value: 10, duration: 1 })
const shopItems = ref<any[]>([])
const shopHealing = ref({ hpPerGold: 10, mpPerGold: 10, weight: 30 })
const shopEnhance = ref({ type: 'attack', value: 10, cost: 500, weight: 15 })
const wheelRewards = ref<any[]>([])

// ---- Computed string wrappers for range fields ----
const startDamageStr = computed({
  get: () => data.value.startDamage ?? '',
  set: (v: string) => { data.value.startDamage = v || undefined }
})
const amountStr = computed({
  get: () => data.value.amount ?? '',
  set: (v: string) => { data.value.amount = v || undefined }
})
const quantityStr = computed({
  get: () => data.value.quantity != null ? String(data.value.quantity) : '',
  set: (v: string) => { data.value.quantity = v ? (v.includes('-') || isNaN(Number(v)) ? v : Number(v)) : undefined }
})
const qualityStr = computed({
  get: () => data.value.quality ?? '',
  set: (v: string) => { data.value.quality = v || undefined }
})

// ---- Computed helpers ----
const treasureItemOptions = computed(() => props.chestItemOptions ?? props.itemOptions ?? [])

const isStatDebuff = computed(() => {
  const t = data.value.debuffType
  return ['AttackDown', 'DefenseDown', 'SpeedDown', 'CritRateDown', 'AllStatsDown'].includes(t)
})

// ---- Parse / Serialize ----
function parseJson(json?: string | null): Record<string, any> {
  if (!json) return {}
  try { return JSON.parse(json) } catch { return {} }
}

function parseAndSync() {
  const parsed = parseJson(props.modelValue)
  data.value = { ...parsed }
  // Battle quantity
  battleQuantity.value = parsed.quantity ?? 1
  // Heal mode
  healMode.value = parsed.healPercent != null ? 'percent' : 'item'
  // Treasure mode
  if (parsed.trapBattle) { treasureMode.value = 'trap' }
  else if (parsed.requireItem) { treasureMode.value = 'locked' }
  else { treasureMode.value = 'normal' }
  // Treasure rewards
  rewardsList.value = Array.isArray(parsed.rewards) ? parsed.rewards.map((r: any) => ({ ...r })) : []
  // Event weightModifier boostTypes
  boostTypesList.value = Array.isArray(parsed.boostTypes) ? [...parsed.boostTypes] : []
  // Event mystery
  mysteryOptionsList.value = Array.isArray(parsed.options) ? [...parsed.options] : []
  mysteryWeightsList.value = Array.isArray(parsed.weights) ? [...parsed.weights] : []
  // Event randomEvent
  randomEventTypesList.value = Array.isArray(parsed.eventTypes) ? [...parsed.eventTypes] : []
  randomEventWeightsList.value = Array.isArray(parsed.weights) ? [...parsed.weights] : []
  // Empty hiddenBuff
  hasHiddenBuff.value = !!parsed.hiddenBuff
  if (parsed.hiddenBuff) { hiddenBuff.value = { ...parsed.hiddenBuff } }
  // Debuff bonusBuff
  hasBonusBuff.value = !!parsed.bonusBuff
  if (parsed.bonusBuff) { bonusBuff.value = { ...parsed.bonusBuff } }
  // Shop items
  shopItems.value = Array.isArray(parsed.items) ? parsed.items.map((it: any) => ({
    itemType: it.itemId ? 'item' : it.equipmentId ? 'equip' : 'collection',
    itemId: it.itemId || '', equipId: it.equipmentId ? String(it.equipmentId) : '', seriesId: it.seriesId || '',
    price: it.price || 0, weight: it.weight || 10
  })) : []
  // Shop healing
  if (parsed.healing) { shopHealing.value = { ...shopHealing.value, ...parsed.healing } }
  // Shop enhance
  if (parsed.enhance) { shopEnhance.value = { ...shopEnhance.value, ...parsed.enhance } }
  // Wheel rewards (only for adventure wheel type)
  if (props.eventType === 7 && parsed.adventureType === 'wheel') {
    wheelRewards.value = Array.isArray(parsed.rewards) ? parsed.rewards.map((r: any) => ({ ...r })) : []
  } else {
    wheelRewards.value = []
  }
}

function buildJson(): string | null {
  const d = { ...data.value }
  // Clean undefined/empty
  Object.keys(d).forEach(k => { if (d[k] === undefined || d[k] === '') delete d[k] })

  // Battle: handle quantity
  if (props.eventType === 1) {
    delete d.monsterIds
    if (battleQuantity.value > 1) {
      d.quantity = battleQuantity.value
    } else {
      delete d.quantity
    }
    if (!d.ambush) { delete d.ambush; delete d.startDamage }
    if (!d.extraReward) delete d.extraReward
  }

  // Heal: clean based on mode
  if (props.eventType === 2) {
    if (healMode.value === 'percent') { delete d.itemId; delete d.quantity }
    else { delete d.healPercent; delete d.target }
  }

  // Treasure
  if (props.eventType === 6) {
    if (treasureMode.value === 'trap') {
      d.trapBattle = true
      delete d.rewards; delete d.requireItem
    } else if (treasureMode.value === 'locked') {
      d.requireItem = d.requireItem || ''
      d.rewards = rewardsList.value.filter(r => r.type)
      delete d.trapBattle; delete d.monsterId
    } else {
      d.rewards = rewardsList.value.filter(r => r.type)
      delete d.trapBattle; delete d.monsterId; delete d.requireItem
    }
  }

  // Adventure
  if (props.eventType === 7) {
    // Clean fields based on adventureType
    const at = d.adventureType
    if (at === 'randomBuff') { delete d.amount; delete d.itemId; delete d.quantity; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
    else if (at === 'grantExp') { delete d.duration; delete d.durationType; delete d.itemId; delete d.quantity; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
    else if (at === 'grantItem') { delete d.amount; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
    else if (at === 'permanentBuff') { delete d.amount; delete d.itemId; delete d.quantity; delete d.duration; delete d.durationType; delete d.hpCost; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
    else if (at === 'riddle') { delete d.amount; delete d.itemId; delete d.quantity; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
    else if (at === 'sacrifice') { delete d.amount; delete d.itemId; delete d.quantity; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
    else if (at === 'risk') { delete d.amount; delete d.itemId; delete d.quantity; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.successRate; delete d.costType; delete d.costAmount; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
    else if (at === 'gamble') { delete d.amount; delete d.itemId; delete d.quantity; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.successRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent; delete d.winRate }
    else if (at === 'wheel') { d.rewards = wheelRewards.value.filter(r => r.type); delete d.amount; delete d.itemId; delete d.quantity; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
    else if (at === 'rest') { delete d.amount; delete d.itemId; delete d.quantity; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.realEvent }
    else if (at === 'fake') { delete d.amount; delete d.itemId; delete d.quantity; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent }
    else if (at === 'treasureChest') { delete d.amount; delete d.itemId; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
    else if (at === 'lore' || at === 'purgeDebuff' || at === 'fullRestore' || at === 'choice') { delete d.amount; delete d.itemId; delete d.quantity; delete d.duration; delete d.durationType; delete d.buffType; delete d.value; delete d.isPercent; delete d.hpCost; delete d.successRate; delete d.costType; delete d.costAmount; delete d.winRate; delete d.healHpPercent; delete d.healMpPercent; delete d.realEvent }
  }

  // Shop
  if (props.eventType === 8) {
    d.items = shopItems.value.filter(it => it.itemType && (
      (it.itemType === 'item' && it.itemId) || (it.itemType === 'equip' && it.equipId) || (it.itemType === 'collection' && it.seriesId)
    )).map(it => {
      const entry: any = { price: it.price, weight: it.weight }
      if (it.itemType === 'item') entry.itemId = it.itemId
      else if (it.itemType === 'equip') entry.equipmentId = Number(it.equipId)
      else if (it.itemType === 'collection') entry.seriesId = it.seriesId
      return entry
    })
    d.healing = { ...shopHealing.value }
    d.enhance = { ...shopEnhance.value }
  }

  // Event (Special)
  if (props.eventType === 9) {
    const et = d.effectType
    // No-field effects
    if (['skipTick', 'purgeDebuffs', 'hpMpSwap'].includes(et)) {
      delete d.value; delete d.duration; delete d.durationType; delete d.healHpPercent; delete d.healMpPercent
      delete d.buffCategory; delete d.multiplier; delete d.buffType; delete d.isPercent
      delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    // value+duration effects
    if (['rewardMultiplier', 'monsterBuff', 'rareBoost', 'trapBoost'].includes(et)) {
      delete d.healHpPercent; delete d.healMpPercent; delete d.buffCategory; delete d.multiplier
      delete d.buffType; delete d.isPercent; delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    if (et === 'weightModifier') {
      d.boostTypes = boostTypesList.value
      d.durationType = d.durationType || 'tick'
      delete d.value; delete d.healHpPercent; delete d.healMpPercent; delete d.buffCategory
      delete d.buffType; delete d.isPercent; delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    if (et === 'mystery') {
      d.options = mysteryOptionsList.value.filter(o => o.trim())
      d.weights = mysteryWeightsList.value.slice(0, d.options.length)
      delete d.value; delete d.duration; delete d.durationType; delete d.healHpPercent; delete d.healMpPercent
      delete d.buffCategory; delete d.multiplier; delete d.buffType; delete d.isPercent
      delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    if (et === 'randomEvent') {
      d.eventTypes = randomEventTypesList.value
      d.weights = randomEventWeightsList.value.slice(0, d.eventTypes.length)
      delete d.value; delete d.duration; delete d.durationType; delete d.healHpPercent; delete d.healMpPercent
      delete d.buffCategory; delete d.multiplier; delete d.buffType; delete d.isPercent
      delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    // regenTick
    if (et === 'regenTick') {
      delete d.value; delete d.buffCategory; delete d.multiplier; delete d.buffType; delete d.isPercent
      delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    // buffMultiplier
    if (et === 'buffMultiplier') {
      delete d.healHpPercent; delete d.healMpPercent; delete d.multiplier
      delete d.buffType; delete d.isPercent; delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    // instantHeal
    if (et === 'instantHeal') {
      delete d.value; delete d.duration; delete d.durationType; delete d.buffCategory; delete d.multiplier
      delete d.buffType; delete d.isPercent; delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    // instantBuff
    if (et === 'instantBuff') {
      delete d.healHpPercent; delete d.healMpPercent; delete d.buffCategory; delete d.multiplier
      delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    // extendBuffs
    if (et === 'extendBuffs') {
      delete d.value; delete d.durationType; delete d.healHpPercent; delete d.healMpPercent
      delete d.buffCategory; delete d.multiplier; delete d.buffType; delete d.isPercent
      delete d.attackPercent; delete d.hpPercent; delete d.healPercent
    }
    // summonClone
    if (et === 'summonClone') {
      delete d.value; delete d.healHpPercent; delete d.healMpPercent; delete d.buffCategory; delete d.multiplier
      delete d.buffType; delete d.isPercent; delete d.healPercent
    }
    // reviveOnce
    if (et === 'reviveOnce') {
      delete d.value; delete d.duration; delete d.durationType; delete d.healHpPercent; delete d.healMpPercent
      delete d.buffCategory; delete d.multiplier; delete d.buffType; delete d.isPercent
      delete d.attackPercent; delete d.hpPercent
    }
  }

  // PlayerEncounter - clean all fields
  if (props.eventType === 11) {
    delete d.encounterType
    delete d.matchScope
  }

  // Debuff bonusBuff
  if (props.eventType === 4 && hasBonusBuff.value && data.value.debuffType === 'DamagePercent') {
    d.bonusBuff = { ...bonusBuff.value }
  } else {
    delete d.bonusBuff
  }

  // Empty hiddenBuff
  if (props.eventType === 10) {
    if (hasHiddenBuff.value) {
      d.hiddenBuff = { ...hiddenBuff.value }
    } else {
      delete d.hiddenBuff
    }
  }

  if (Object.keys(d).length === 0) return null
  return JSON.stringify(d)
}

// ---- Watchers ----
watch(() => props.modelValue, () => { parseAndSync() }, { immediate: true })
watch(() => props.eventType, () => { parseAndSync() })

// Emit on any change
watch([data, battleQuantity, healMode, treasureMode, rewardsList, boostTypesList,
  mysteryOptionsList, mysteryWeightsList, randomEventTypesList, randomEventWeightsList,
  hasHiddenBuff, hiddenBuff, hasBonusBuff, bonusBuff,
  shopItems, shopHealing, shopEnhance, wheelRewards], () => {
  emit('update:modelValue', buildJson())
}, { deep: true })

</script>

<style scoped>
.event-data-editor {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.editor-grid {
  display: grid;
  gap: 12px;
}
.editor-grid--three {
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
}
.editor-section {
  margin-top: 12px;
  padding-top: 10px;
  border-top: 1px solid var(--border-color, #eee);
}
.editor-section__title {
  font-size: 13px;
  font-weight: 600;
  margin: 0 0 8px;
  color: var(--text-primary);
}
.field-note {
  font-size: 13px;
  color: var(--text-secondary);
  padding: 8px 0;
}
</style>

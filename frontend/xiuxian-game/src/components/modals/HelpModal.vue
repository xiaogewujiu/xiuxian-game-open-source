<template>
  <XiuXianModal
    :model-value="modelValue"
    title="游戏帮助"
    :width="1120"
    @update:model-value="$emit('update:modelValue', $event)"
  >
    <div class="help-modal">
      <section class="help-hero">
        <div class="hero-mark">☯</div>
        <div>
          <div class="hero-kicker">修仙问道 · 入门指引</div>
          <h2>从凡人起步，走出自己的修行路</h2>
          <p>这是一个以回合战斗、地图探索、装备养成和生产玩法为核心的文字修仙游戏。先选择地图战斗，再用获得的资源提升角色，逐步挑战更高阶段。</p>
        </div>
      </section>

      <div class="help-layout">
        <nav class="help-nav" aria-label="帮助目录">
          <button
            v-for="section in sections"
            :key="section.key"
            class="help-nav-item"
            :class="{ active: activeSection === section.key }"
            @click="selectSection(section.key)"
          >
            <span class="nav-icon">{{ section.icon }}</span>
            <span>{{ section.title }}</span>
          </button>
        </nav>

        <div ref="contentRef" class="help-content">
          <section
            v-for="section in sections"
            :id="`help-${section.key}`"
            :key="section.key"
            class="help-section"
          >
            <div class="section-heading">
              <span class="section-icon">{{ section.icon }}</span>
              <div>
                <h3>{{ section.title }}</h3>
                <p>{{ section.summary }}</p>
              </div>
            </div>

            <div class="topic-grid">
              <article v-for="topic in section.topics" :key="topic.title" class="topic-card" :class="{ 'topic-card--emphasis': topic.emphasis }">
                <div v-if="topic.emphasis" class="topic-important-badge">重中之重</div>
                <h4>{{ topic.title }}</h4>
                <p v-for="(paragraph, index) in topic.paragraphs" :key="index">{{ paragraph }}</p>
                <ul v-if="topic.points?.length">
                  <li v-for="point in topic.points" :key="point">{{ point }}</li>
                </ul>
              </article>
            </div>
          </section>
        </div>
      </div>

      <div class="help-footer">
        <span>遇到显示异常或规则疑问，可以通过“建议反馈”提交问题。</span>
        <button class="back-top-btn" @click="scrollToTop">返回目录顶部</button>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { nextTick, ref, watch } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'

const sections = [
  {
    key: 'start', icon: '✦', title: '初入仙途', summary: '第一次进入游戏时，建议按照“角色—地图—战斗—养成”的顺序熟悉系统。',
    topics: [
      { title: '核心循环', paragraphs: ['选择适合当前实力的地图，开始单场战斗或离线挂机，获得经验、金币、灵石、材料和装备，再回到角色面板强化自己。'], points: ['先看人物属性和装备栏，确认当前战力。', '普通地图适合稳定升级和收集装备；秘境适合组队与阶段性奖励。', '卡关时优先检查装备、技能、属性加点和药品。'] },
      { title: '资源用途', paragraphs: ['金币主要用于系统内的养成、制作和部分功能消耗；灵石、荣誉、宗门贡献等资源分别服务于不同玩法。背包中的物品请先查看描述再决定使用、出售或分解。'] },
      { title: '界面操作', paragraphs: ['左侧导航按功能分组。点击“☰”可以展开或收起导航栏；所有功能弹窗都可以点击右上角关闭，遮罩区域也可以关闭。'] },
      { title: '问题反馈', emphasis: true, paragraphs: ['如果出现数据没有刷新、奖励未显示或界面提示异常，先关闭并重新打开对应弹窗；仍未解决时，打开“建议反馈”，写明功能、操作步骤和大致时间。'] }
    ]
  },
  {
    key: 'character', icon: '♙', title: '角色与战力', summary: '职业、主属性、境界、装备、技能和各种加成共同决定战斗表现。',
    topics: [
      { title: '人物属性', paragraphs: ['人物属性面板展示基础属性、当前战斗属性、灵根和五行相克关系。升级后获得潜能属性点，可以按职业定位加点；已投入的点数也可以按规则返还。'], points: ['物攻职业优先关注物理攻击、生命和物防。', '法攻职业优先关注法术攻击、灵力和法防。', '肉身职业更重视生命、双防和持续作战能力。', '速度、命中、闪避、暴击、连击、反击和破甲等高级属性会影响实际战斗结果。'] },
      { title: '装备与品质', paragraphs: ['装备分为武器、防具、法宝和首饰等部位，并带有品质、基础属性、洗练词条和宝石孔位。装备品质越高，通常基础属性预算和可成长空间越高。'], points: ['从背包点击装备可以穿戴、比较、出售或丢弃。', '已穿戴装备需要先卸下，才能出售或进行部分处理。', '装备详情中的洗练词条和宝石属性属于额外战力来源。'] },
      { title: '装备分解', paragraphs: ['背包中的未穿戴、未锁定装备可以分解，分解产出由装备品质对应的规则决定。分解产出的道具和数量以当前配置为准。'], points: ['已锁定或已穿戴装备不会被分解。', '支持单件分解和批量分解，批量操作会一次性结算。', '分解材料需要占用道具背包格；背包已满时请先整理空间。'] },
      { title: '技能与增益', paragraphs: ['技能面板用于查看、升级和装备技能。战斗会根据当前装备的技能配置和系统规则释放技能，技能效果可能造成伤害、治疗、护盾、控制或施加 Buff/Debuff。'], points: ['技能等级和技能配置会直接改变战斗节奏。', '技能书可以解锁或成长技能；技能书碎片用于相关升级或分解流程。', '更换技能后，建议重新观察战斗日志和战斗回放。'] },
      { title: '称号与灵宠', paragraphs: ['称号可以装备并提供展示或属性效果；灵宠可以设置出战、喂养、进化和放生。不同灵宠和称号适合不同的养成方向。'] }
    ]
  },
  {
    key: 'battle', icon: '⚔', title: '战斗与地图', summary: '战斗是回合制流程，地图选择决定敌人、掉落和挑战门槛。',
    topics: [
      { title: '开始战斗', paragraphs: ['点击“选择地图”进入地图列表，选择地图后点击“开始战斗”。战斗区域会显示敌我单位、气血/灵力、回合数和战斗日志。'], points: ['普通地图支持单人战斗和离线挂机。', '战斗冷却结束后才能开始下一场；循环战斗会自动重复当前挑战。', '战斗日志适合排查技能释放、属性克制和伤害来源。'] },
      { title: '地图选择', paragraphs: ['地图按阶段和用途组织。通常可以从普通地图开始，再尝试材料图、精英图、首领图和秘境。地图名称、推荐阶段、敌人配置和掉落说明以当前服务端返回为准。'] },
      { title: '离线挂机', paragraphs: ['选择地图后点击“开始离线”，角色会在服务端持续结算战斗。每次离线挂机固定运行最多48小时，到期后自动完成结算并停止；也可以随时点击“停止离线”提前结束。挂机期间仍应选择角色能够稳定获胜的地图。'] },
      { title: '五行与战斗信息', paragraphs: ['灵根和五行相克会影响战斗中的伤害或承伤表现。鼠标悬停敌人可以查看基础属性和高级属性，战斗回放则可以复盘每一回合的行动。'] }
    ]
  },
  {
    key: 'cultivation', icon: '☯', title: '修炼与生产', summary: '修炼系统负责长期成长，生产系统把采集到的材料转化为丹药和装备。',
    topics: [
      { title: '境界与升级', paragraphs: ['战斗、任务和其他玩法会提供经验。经验达到要求后提升等级；部分境界节点还需要满足突破材料、成功率或其他条件。'] },
      { title: '灵田', paragraphs: ['灵田可以种植作物。选择空闲地块和作物模板后开始生长，成熟后收获。地块可以升级，以获得更好的生产能力。'], points: ['作物生长期间可以查看剩余时间。', '催熟只能使用当前配置允许的材料，消耗材料后缩短生长时间。', '收获的作物可用于炼丹、任务或其他系统，请留意道具描述。'] },
      { title: '炼丹', paragraphs: ['炼丹配方通过对应丹方卷轴解锁；解锁后才会进入可炼制范围。炼丹需要满足炼丹师等级、丹炉条件、金币和材料要求，并且同时只能处理一个制作任务。'], points: ['配方解锁和实际炼制是两件事：低等级可以解锁高阶配方，但未必能立即制作。', '炼丹完成后需要在炼丹弹窗中领取结果。', '炼丹经验和熟练度用于提升炼丹能力。'] },
      { title: '锻造', paragraphs: ['锻造图纸通过对应图纸道具解锁；锻造面板只展示已经解锁的图纸。实际锻造还要检查锻造师等级、金币、材料和当前是否已有制作任务。'], points: ['锻造完成后需要领取装备。', '装备获得后可以在背包穿戴、比较、洗练和镶嵌宝石。', '图纸解锁不等于立即具备制作资格，等级条件在实际锻造时检查。'] },
      { title: '装备强化', paragraphs: ['装备详情支持洗练词条和宝石镶嵌。洗练通常需要洗练材料；宝石可以镶入有孔位的装备，也可以取下。'] }
    ]
  },
  {
    key: 'adventure', icon: '🗺', title: '秘境与多人历练', summary: '秘境、组队、世界 Boss、竞技场和通天塔提供不同的挑战目标。',
    topics: [
      { title: '持续探索秘境', paragraphs: ['秘境以阶段事件组持续推进。进入后按事件顺序探索，可能遭遇战斗、奖励、资源、选择或阶段事件；过程中可以查看当前状态和历史结算。'] },
      { title: '组队副本', paragraphs: ['组队后由队长选择副本地图并发起战斗，队伍成员共同参与结算。副本战斗与普通地图的敌人和奖励配置相互独立，适合多人协作挑战。'], points: ['组队状态下必须选择副本地图。', '进入副本前确认队伍成员、角色定位和技能配置。', '副本令牌等入口资源请根据当前副本提示准备。'] },
      { title: '世界 Boss', paragraphs: ['世界 Boss 是多人共同参与的限时挑战。进入后可以进行普通行动或技能行动，系统会记录伤害排名和奖励状态。'] },
      { title: '竞技场与通天塔', paragraphs: ['竞技场用于挑战其他道友并记录赛季战绩；通天塔按层挑战并记录最高进度。挑战次数、购买次数和奖励以对应面板的当前状态为准。'] }
    ]
  },
  {
    key: 'social', icon: '☷', title: '社交与交易', summary: '与其他道友交流、加入宗门、交易物品，并通过邮件接收奖励。',
    topics: [
      { title: '宗门', paragraphs: ['可以浏览宗门、加入宗门并参与宗门任务、捐献、商店、弟子切磋和宗门活动。宗门贡献通常用于宗门相关成长和兑换。'] },
      { title: '商店与寄售行', paragraphs: ['商店提供系统固定商品；寄售行允许玩家浏览、上架和购买其他玩家出售的物品或装备。购买前请核对价格、品质和物品数量。'] },
      { title: '好感度与邮件', paragraphs: ['好感度功能用于与目标角色互动和赠送礼物；邮件用于接收系统通知、奖励和附件。领取邮件附件后再删除邮件，避免遗漏奖励。'] },
      { title: '世界聊天', paragraphs: ['首页下方聊天区包含世界、宗门和系统频道。宗门频道需要加入宗门，系统公告频道只读；实时连接异常时仍会尝试使用普通接口。'] }
    ]
  },
  {
    key: 'welfare', icon: '🎁', title: '福利与系统', summary: '签到、任务、成就、抽奖和兑换是日常资源的重要补充。',
    topics: [
      { title: '签到', paragraphs: ['每天打开签到面板查看当月签到状态并领取当日奖励，连续签到和月度进度可能带来额外奖励。'] },
      { title: '任务与成就', paragraphs: ['任务通常需要先接受，再完成目标并提交领取奖励；成就会持续记录角色进度，达成后在可领取页领取奖励。任务和成就的具体目标以面板实时数据为准。'] },
      { title: '抽奖与兑换', paragraphs: ['抽奖消耗对应抽奖资源并从当前奖池获得奖励；兑换功能用于输入有效兑换码。请确认资源和兑换码没有多余空格。'] },
      { title: '装备自动出售', paragraphs: ['打开“福利与系统 → 设置”，可以配置最低保留装备等级和最低保留装备品质。保存后，普通地图、离线战斗、副本和秘境结算获得的装备会自动处理。'], points: ['装备等级和品质都达到设定值时保留；未配置的条件不限制。', '两个条件都选择“不限制”时，不会自动出售装备。', '已穿戴或已绑定装备不会被自动出售。', '被出售的装备会按现有出售规则转换为金币。'] },
      { title: '排行、主题和更新', paragraphs: ['排行榜展示不同维度的角色排名；主题按钮可以切换界面风格；更新日志用于查看版本功能和修复记录。帮助页入口始终保留在“福利与系统”分组。'] }
    ]
  },
  {
    key: 'tips', icon: '❖', title: '实用建议', summary: '按下面的顺序排查问题，通常可以更快找到提升方向。',
    topics: [
      { title: '遇到打不过的怪', paragraphs: ['先确认地图是否超出当前阶段，再检查装备是否穿戴、技能是否装备、属性点是否分配、灵宠是否出战，以及是否拥有合适的恢复或增益道具。'], points: ['观察战斗日志：伤害不足看攻击和技能，生存不足看生命与双防，出手慢看速度。', '尝试更换五行匹配或技能组合。', '稳定挂机优先于挑战高难度地图。'] },
      { title: '生产玩法顺序', paragraphs: ['先通过地图获取材料，再用灵田补充作物，使用丹方和图纸解锁生产内容，最后根据等级和材料储备安排制作。制作完成后记得领取。'] },
    ]
  }
]

export default {
  name: 'HelpModal',
  components: { XiuXianModal },
  props: { modelValue: Boolean },
  emits: ['update:modelValue'],
  setup(props) {
    const activeSection = ref('start')
    const contentRef = ref(null)
    const selectSection = (key) => {
      activeSection.value = key
      nextTick(() => document.getElementById(`help-${key}`)?.scrollIntoView({ behavior: 'smooth', block: 'start' }))
    }
    const scrollToTop = () => {
      contentRef.value?.scrollTo({ top: 0, behavior: 'smooth' })
      activeSection.value = 'start'
    }
    watch(() => props.modelValue, (visible) => {
      if (visible) {
        activeSection.value = 'start'
        nextTick(() => contentRef.value?.scrollTo({ top: 0 }))
      }
    })
    return { sections, activeSection, contentRef, selectSection, scrollToTop }
  }
}
</script>

<style scoped>
.help-modal { display: flex; flex-direction: column; gap: var(--spacing-md); color: var(--text-primary); }
.help-hero { display: flex; align-items: center; gap: var(--spacing-md); padding: var(--spacing-lg); background: linear-gradient(135deg, rgba(124, 58, 237, .16), rgba(227, 179, 92, .10)); border: 1px solid var(--border-color); border-radius: var(--radius-lg); }
.hero-mark { width: 58px; height: 58px; flex: 0 0 auto; display: grid; place-items: center; border-radius: 50%; background: var(--accent-soft-bg); color: var(--highlight-text); font-size: 34px; }
.hero-kicker { color: var(--accent-text); font-size: var(--font-size-sm); letter-spacing: .12em; }
.help-hero h2 { margin: 4px 0; color: var(--text-primary); font-size: 22px; }
.help-hero p { margin: 0; color: var(--text-secondary); line-height: 1.7; }
.help-layout { display: grid; grid-template-columns: 176px minmax(0, 1fr); gap: var(--spacing-md); min-height: 0; height: min(600px, 64vh); }
.help-nav { display: flex; flex-direction: column; gap: 5px; padding: var(--spacing-sm); overflow-y: auto; background: var(--xiuxian-bg-secondary); border: 1px solid var(--border-color); border-radius: var(--radius-md); }
.help-nav-item { display: flex; align-items: center; gap: 8px; padding: 10px 11px; border: 1px solid transparent; border-radius: var(--radius-sm); background: transparent; color: var(--text-secondary); cursor: pointer; text-align: left; font-family: inherit; }
.help-nav-item:hover, .help-nav-item.active { background: var(--accent-soft-bg); border-color: var(--control-border); color: var(--text-primary); }
.nav-icon, .section-icon { color: var(--highlight-text); }
.help-content { overflow-y: auto; padding-right: 4px; scroll-behavior: smooth; }
.help-section { scroll-margin-top: 8px; padding: 2px 2px 22px; }
.help-section + .help-section { padding-top: 18px; border-top: 1px solid var(--border-color); }
.section-heading { display: flex; gap: 10px; align-items: flex-start; margin-bottom: var(--spacing-sm); }
.section-icon { font-size: 24px; line-height: 1.2; }
.section-heading h3 { margin: 0; font-size: 18px; color: var(--text-primary); }
.section-heading p { margin: 3px 0 0; color: var(--text-secondary); }
.topic-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: var(--spacing-sm); }
.topic-card { padding: var(--spacing-md); background: rgba(255, 255, 255, .025); border: 1px solid var(--border-color); border-radius: var(--radius-md); }
.topic-card--emphasis { border: 2px solid var(--status-warning-text); background: linear-gradient(135deg, rgba(245, 158, 11, .16), rgba(245, 158, 11, .05)); box-shadow: 0 0 18px rgba(245, 158, 11, .14); }
.topic-important-badge { display: inline-flex; margin-bottom: 7px; padding: 3px 9px; border-radius: 999px; background: var(--status-warning-text); color: #241500; font-size: 12px; font-weight: 700; letter-spacing: .08em; }
.topic-card h4 { margin: 0 0 6px; color: var(--highlight-text); font-size: 15px; }
.topic-card p { margin: 5px 0; color: var(--text-secondary); line-height: 1.65; }
.topic-card ul { margin: 8px 0 0; padding-left: 20px; color: var(--text-secondary); line-height: 1.7; }
.help-footer { display: flex; justify-content: space-between; align-items: center; gap: var(--spacing-md); padding-top: var(--spacing-sm); border-top: 1px solid var(--border-color); color: var(--text-muted); font-size: var(--font-size-sm); }
.back-top-btn { padding: 6px 10px; border: 1px solid var(--control-border); border-radius: var(--radius-sm); background: var(--control-bg); color: var(--control-text); cursor: pointer; }
.back-top-btn:hover { background: var(--control-bg-hover); color: var(--text-primary); }
@media (max-width: 760px) { .help-layout { grid-template-columns: 1fr; height: min(660px, 68vh); } .help-nav { flex-direction: row; overflow-x: auto; overflow-y: hidden; } .help-nav-item { white-space: nowrap; } .topic-grid { grid-template-columns: 1fr; } .help-footer { align-items: flex-start; flex-direction: column; } }
</style>

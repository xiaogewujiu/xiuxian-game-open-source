<template>
  <!-- 更新日志 Modal -->
  <XiuXianModal :model-value="modelValue" title="更新日志" :width="550" @update:model-value="$emit('update:modelValue', $event)">
    <div class="changelog-modal">
      <!-- 版本列表 -->
      <div class="version-list">
        <div v-for="version in changelogs" :key="version.version" class="version-item">
          <div class="version-header">
            <div class="version-title">
              <span class="version-number">v{{ version.version }}</span>
              <span v-if="version.isNew" class="new-badge">NEW</span>
            </div>
            <div class="version-date">{{ version.date }}</div>
          </div>

          <div class="version-content">
            <div v-if="version.features.length" class="change-section">
              <div class="section-title feature"><AssetIcon :source="ICON.misc_sparkles" size="20" /> 新增功能</div>
              <ul class="change-list">
                <li v-for="(item, index) in version.features" :key="index">{{ item }}</li>
              </ul>
            </div>

            <div v-if="version.optimizations.length" class="change-section">
              <div class="section-title optimization"><AssetIcon :source="ICON.misc_lightning" size="20" /> 优化改进</div>
              <ul class="change-list">
                <li v-for="(item, index) in version.optimizations" :key="index">{{ item }}</li>
              </ul>
            </div>

            <div v-if="version.fixes.length" class="change-section">
              <div class="section-title fix"><AssetIcon :source="ICON.ui_gear" size="20" /> 问题修复</div>
              <ul class="change-list">
                <li v-for="(item, index) in version.fixes" :key="index">{{ item }}</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  </XiuXianModal>
</template>

<script>
import { reactive } from 'vue'
import XiuXianModal from '../common/XiuXianModal.vue'
import { ICON } from '../../icons'
import AssetIcon from '../common/AssetIcon.vue'

/**
 * ChangelogModal - 更新日志弹窗
 *
 * 功能：
 * - 显示更新公告内容
 * - 按版本分类展示
 */
export default {
  name: 'ChangelogModal',

  components: {
    XiuXianModal,
    AssetIcon
  },

  props: {
    modelValue: Boolean
  },

  emits: ['update:modelValue'],

  setup() {
    const changelogs = reactive([
      {
        version: '1.0.0',
        date: '2024-02-02',
        isNew: true,
        features: [
          '全新修仙世界开放，支持角色修炼升级',
          '新增战斗系统，支持自动战斗',
          '新增地图探索功能',
          '新增宠物系统，可捕获和培养宠物',
          '新增装备锻造和炼丹系统',
          '新增组队功能，可与其他道友组队探险'
        ],
        optimizations: [
          '优化UI界面，提升用户体验',
          '优化战斗流畅度'
        ],
        fixes: []
      },
      {
        version: '0.9.0',
        date: '2024-01-15',
        isNew: false,
        features: [
          '新增签到系统',
          '新增排行榜功能',
          '新增商店系统'
        ],
        optimizations: [
          '优化资源加载速度'
        ],
        fixes: [
          '修复部分界面显示异常的问题'
        ]
      },
      {
        version: '0.8.0',
        date: '2024-01-01',
        isNew: false,
        features: [
          '游戏基础框架搭建完成',
          '新增登录注册系统',
          '新增角色属性面板'
        ],
        optimizations: [],
        fixes: []
      }
    ])

    return {
      ICON,
      changelogs
    }
  }
}
</script>

<style scoped>
.changelog-modal {
  max-height: 500px;
  overflow-y: auto;
  padding-right: var(--spacing-sm);
}

.version-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.version-item {
  background: var(--xiuxian-bg-secondary);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.version-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-md);
  background: rgba(124, 58, 237, 0.1);
  border-bottom: 1px solid var(--border-color);
}

.version-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.version-number {
  font-size: 16px;
  font-weight: 600;
  color: var(--highlight-text);
}

.new-badge {
  padding: 2px 8px;
  background: #f44336;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  color: white;
  font-weight: 600;
}

.version-date {
  font-size: var(--font-size-base);
  color: var(--text-muted);
}

.version-content {
  padding: var(--spacing-md);
}

.change-section {
  margin-bottom: var(--spacing-md);
}

.change-section:last-child {
  margin-bottom: 0;
}

.section-title {
  font-size: var(--font-size-base);
  font-weight: 600;
  margin-bottom: var(--spacing-sm);
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
}

.section-title.feature {
  color: var(--quality-uncommon);
}

.section-title.optimization {
  color: var(--status-warning-text);
}

.section-title.fix {
  color: var(--status-info-text);
}

.change-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.change-list li {
  padding: var(--spacing-xs) 0;
  padding-left: var(--spacing-md);
  font-size: var(--font-size-base);
  color: var(--text-secondary);
  position: relative;
}

.change-list li::before {
  content: '•';
  position: absolute;
  left: 0;
  color: var(--accent-text);
}

.change-list li:hover {
  color: var(--text-primary);
}
</style>

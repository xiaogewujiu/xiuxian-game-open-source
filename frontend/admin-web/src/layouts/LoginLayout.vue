<template>
  <div class="login-shell">
    <section class="login-shell__intro">
      <div>
        <p class="section-kicker">后台入口</p>
        <h1 class="login-shell__title">修仙游戏管理后台</h1>
        <p class="login-shell__copy">
          用于管理玩家、配置、运营工具和审计日志的统一后台入口。
        </p>
      </div>

      <div class="login-shell__highlights login-shell__highlights--stack">
        <article class="login-highlight">
          <span class="login-highlight__tag">System</span>
          <strong>统一管理</strong>
          <p>地图、怪物、装备、任务、活动奖励等页面统一维护。</p>
        </article>
        <article class="login-highlight">
          <span class="login-highlight__tag">Ops</span>
          <strong>运营处理</strong>
          <p>支持玩家资料维护、GM 发放、封禁和后台账号管理。</p>
        </article>
        <article class="login-highlight">
          <span class="login-highlight__tag">Audit</span>
          <strong>操作留痕</strong>
          <p>审计日志记录后台写操作，便于排查与回溯。</p>
        </article>
      </div>
    </section>

    <div class="login-shell__panel">
      <RouterView />
    </div>
  </div>
</template>

<script setup lang="ts">
</script>

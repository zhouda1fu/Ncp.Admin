<template>
  <div class="sc-workflow-design sc-workflow-design--v2">
    <div class="box-scale">
      <node-wrap
        v-if="nodeConfig"
        v-model="nodeConfig"
        :category="category"
        :view-only="viewOnly"></node-wrap>
      <div class="end-node">
        <div class="end-node-circle"></div>
        <div class="end-node-text">流程结束</div>
      </div>
    </div>
    <use-select
      v-if="selectVisible"
      ref="useselect"
      @closed="selectVisible = false"></use-select>
  </div>
</template>

<script>
import nodeWrap from './nodeWrap.vue'
import useSelect from './select.vue'

export default {
  provide() {
    return {
      select: this.selectHandle,
      readonly: this.viewOnly,
    }
  },
  props: {
    modelValue: { type: Object, default: () => {} },
    category: { type: String, default: '' },
    viewOnly: { type: Boolean, default: false },
  },
  components: {
    nodeWrap,
    useSelect
  },
  data() {
    return {
      nodeConfig: this.modelValue,
      selectVisible: false
    }
  },
  watch: {
    modelValue(val) {
      this.nodeConfig = val
    },
    nodeConfig(val) {
      this.$emit('update:modelValue', val)
    }
  },
  mounted() {},
  methods: {
    selectHandle(type, data) {
      if (this.viewOnly) return
      this.selectVisible = true
      this.$nextTick(() => {
        this.$refs.useselect.open(type, data)
      })
    }
  }
}
</script>

<style lang="scss">
.sc-workflow-design {
  width: 100%;
}
.sc-workflow-design--v2 .box-scale {
  padding: 4px 0 8px;
}
.sc-workflow-design .box-scale {
  display: inline-block;
  position: relative;
  width: 100%;
  align-items: flex-start;
  justify-content: center;
  flex-wrap: wrap;
  min-width: min-content;
}

.sc-workflow-design {
  .node-wrap {
    display: inline-flex;
    width: 100%;
    flex-flow: column wrap;
    justify-content: flex-start;
    align-items: center;
    padding: 0px 0px;
    position: relative;
    z-index: 1;
  }
  .node-wrap-box {
    display: inline-flex;
    flex-direction: column;
    position: relative;
    width: 228px;
    min-height: 72px;
    flex-shrink: 0;
    background: hsl(var(--card));
    border-radius: 10px;
    border: 1px solid hsl(var(--border) / 0.55);
    cursor: pointer;
    box-shadow:
      0 1px 2px hsl(var(--foreground) / 0.04),
      0 4px 12px hsl(var(--foreground) / 0.06);
    transition:
      box-shadow 0.2s ease,
      border-color 0.2s ease;
  }
  .node-wrap-box::before {
    content: '';
    position: absolute;
    top: -12px;
    left: 50%;
    transform: translateX(-50%);
    width: 0px;
    border-style: solid;
    border-width: 8px 6px 4px;
    border-color: hsl(var(--border)) transparent transparent;
  }
  .node-wrap-box.start-node:before {
    content: none;
  }
  .node-wrap-box .title {
    height: 28px;
    line-height: 28px;
    color: #fff;
    padding-left: 14px;
    padding-right: 30px;
    border-radius: 9px 9px 0 0;
    position: relative;
    display: flex;
    align-items: center;
  }
  .node-wrap-box .title.title--primary {
    background: hsl(var(--primary)) !important;
  }
  .node-wrap-box .title .icon {
    margin-right: 5px;
  }
  .node-wrap-box .title .close {
    font-size: 15px;
    position: absolute;
    top: 50%;
    transform: translateY(-50%);
    right: 10px;
    display: none;
  }
  .node-wrap-box .content {
    position: relative;
    padding: 12px 14px 14px;
    font-size: 13px;
  }
  .node-wrap-box .content .placeholder {
    color: #999;
  }
  .node-wrap-box:hover .close {
    display: block;
  }
  .add-node-btn-box {
    width: 240px;
    display: inline-flex;
    flex-shrink: 0;
    position: relative;
    z-index: 1;
  }
  .add-node-btn-box:before {
    content: '';
    position: absolute;
    top: 0px;
    left: 0px;
    right: 0px;
    bottom: 0px;
    z-index: -1;
    margin: auto;
    width: 2px;
    height: 100%;
    background: linear-gradient(
      180deg,
      hsl(var(--border)) 0%,
      hsl(var(--primary) / 0.35) 50%,
      hsl(var(--border)) 100%
    );
    border-radius: 2px;
  }
  .add-node-btn {
    user-select: none;
    width: 240px;
    padding: 20px 0px 32px;
    display: flex;
    justify-content: center;
    flex-shrink: 0;
    flex-grow: 1;
  }
  .add-branch {
    justify-content: center;
    padding: 0px 10px;
    position: absolute;
    top: -16px;
    left: 50%;
    transform: translateX(-50%);
    transform-origin: center center;
    z-index: 1;
    display: inline-flex;
    align-items: center;
  }
  .branch-wrap {
    display: inline-flex;
    width: 100%;
  }
  .branch-box-wrap {
    display: flex;
    flex-flow: column wrap;
    align-items: center;
    min-height: 270px;
    width: 100%;
    flex-shrink: 0;
  }
  .col-box {
    display: inline-flex;
    flex-direction: column;
    align-items: center;
    position: relative;
  }
  .branch-box {
    display: flex;
    overflow: visible;
    min-height: 180px;
    height: auto;
    border-bottom: 2px solid hsl(var(--border));
    border-top: 2px solid hsl(var(--border));
    border-radius: 12px;
    position: relative;
    margin-top: 15px;
    background: hsl(var(--muted) / 0.2);
  }
  .branch-box .col-box::before {
    content: '';
    position: absolute;
    top: 0px;
    left: 0px;
    right: 0px;
    bottom: 0px;
    z-index: 0;
    margin: auto;
    width: 2px;
    height: 100%;
    background-color: rgb(202, 202, 202);
  }
  .condition-node {
    display: inline-flex;
    flex-direction: column;
    min-height: 220px;
  }
  .condition-node-box {
    padding-top: 30px;
    padding-right: 50px;
    padding-left: 50px;
    justify-content: center;
    align-items: center;
    flex-grow: 1;
    position: relative;
    display: inline-flex;
    flex-direction: column;
  }
  .condition-node-box::before {
    content: '';
    position: absolute;
    top: 0px;
    left: 0px;
    right: 0px;
    bottom: 0px;
    margin: auto;
    width: 2px;
    height: 100%;
    background-color: rgb(202, 202, 202);
  }
  .auto-judge {
    position: relative;
    width: 228px;
    min-height: 72px;
    background: hsl(var(--card));
    border-radius: 10px;
    border: 1px solid hsl(var(--border) / 0.55);
    padding: 14px 14px;
    cursor: pointer;
    box-shadow:
      0 1px 2px hsl(var(--foreground) / 0.04),
      0 4px 12px hsl(var(--foreground) / 0.06);
  }
  .auto-judge::before {
    content: '';
    position: absolute;
    top: -12px;
    left: 50%;
    transform: translateX(-50%);
    width: 0px;
    border-style: solid;
    border-width: 8px 6px 4px;
    border-color: hsl(var(--border)) transparent transparent;
    background: transparent;
  }
  .auto-judge .title {
    line-height: 16px;
  }
  .auto-judge .title .node-title {
    color: #15bc83;
  }
  .auto-judge .title .close {
    font-size: 15px;
    position: absolute;
    top: 15px;
    right: 15px;
    color: #999;
    display: none;
  }
  .auto-judge .title .priority-title {
    position: absolute;
    top: 15px;
    right: 15px;
    color: #999;
  }
  .auto-judge .content {
    position: relative;
    padding-top: 15px;
  }
  .auto-judge .content .placeholder {
    color: #999;
  }
  .auto-judge:hover {
    .close {
      display: block;
    }
    .priority-title {
      display: none;
    }
  }
  .top-left-cover-line,
  .top-right-cover-line {
    position: absolute;
    height: 3px;
    width: 50%;
    background-color: #efefef;
    top: -2px;
  }
  .bottom-left-cover-line,
  .bottom-right-cover-line {
    position: absolute;
    height: 3px;
    width: 50%;
    background-color: #efefef;
    bottom: -2px;
  }
  .top-left-cover-line {
    left: -1px;
  }
  .top-right-cover-line {
    right: -1px;
  }
  .bottom-left-cover-line {
    left: -1px;
  }
  .bottom-right-cover-line {
    right: -1px;
  }
  .end-node {
    border-radius: 50%;
    font-size: 13px;
    color: hsl(var(--muted-foreground));
    text-align: center;
    padding: 8px 0 4px;
  }
  .end-node-circle {
    width: 12px;
    height: 12px;
    margin: auto;
    border-radius: 50%;
    background: linear-gradient(145deg, hsl(var(--muted-foreground) / 0.5), hsl(var(--border)));
    box-shadow: 0 0 0 3px hsl(var(--muted) / 0.5);
  }
  .end-node-text {
    margin-top: 8px;
    text-align: center;
    font-weight: 500;
    letter-spacing: 0.02em;
  }
  .auto-judge:hover {
    .sort-left {
      display: flex;
    }
    .sort-right {
      display: flex;
    }
  }
  .auto-judge .sort-left {
    position: absolute;
    top: 0;
    bottom: 0;
    z-index: 1;
    left: 0;
    display: none;
    justify-content: center;
    align-items: center;
    flex-direction: column;
  }
  .auto-judge .sort-right {
    position: absolute;
    top: 0;
    bottom: 0;
    z-index: 1;
    right: 0;
    display: none;
    justify-content: center;
    align-items: center;
    flex-direction: column;
  }
  .auto-judge .sort-left:hover,
  .auto-judge .sort-right:hover {
    background: #eee;
  }
  .auto-judge:after {
    pointer-events: none;
    content: '';
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    z-index: 2;
    border-radius: 4px;
    transition: all 0.1s;
  }
  .auto-judge:hover:after {
    border: 1px solid hsl(var(--primary));
    box-shadow: 0 0 6px 0 hsl(var(--primary) / 0.3);
  }
  .node-wrap-box:after {
    pointer-events: none;
    content: '';
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    z-index: 2;
    border-radius: 4px;
    transition: all 0.1s;
  }
  .node-wrap-box:hover:after {
    border: 1px solid hsl(var(--primary));
    box-shadow: 0 0 6px 0 hsl(var(--primary) / 0.3);
  }
}

.tags-list {
  margin-top: 15px;
  width: 100%;
}
.add-node-popover-body li {
  display: inline-block;
  width: 80px;
  text-align: center;
  padding: 10px 0;
}
.add-node-popover-body li i {
  border: 1px solid hsl(var(--border));
  width: 40px;
  height: 40px;
  border-radius: 50%;
  text-align: center;
  line-height: 38px;
  font-size: 18px;
  cursor: pointer;
}
.add-node-popover-body li i:hover {
  border: 1px solid hsl(var(--primary));
  background: hsl(var(--primary));
  color: #fff !important;
}
.add-node-popover-body li p {
  font-size: 12px;
  margin-top: 5px;
}
.node-wrap-drawer__title {
  padding-right: 40px;
}
.node-wrap-drawer__title label {
  cursor: pointer;
}
.node-wrap-drawer__title label:hover {
  border-bottom: 1px dashed hsl(var(--primary));
}
.node-wrap-drawer__title .node-wrap-drawer__title-edit {
  color: hsl(var(--primary));
  margin-left: 10px;
  vertical-align: middle;
}

.dark .sc-workflow-design {
  .node-wrap-box,
  .auto-judge {
    background: hsl(var(--card));
  }
  .col-box {
    background: hsl(var(--card));
  }
  .top-left-cover-line,
  .top-right-cover-line,
  .bottom-left-cover-line,
  .bottom-right-cover-line {
    background-color: hsl(var(--card));
  }
  .node-wrap-box::before,
  .auto-judge::before {
    background-color: hsl(var(--card));
  }
  .branch-box .add-branch {
    background: hsl(var(--card));
  }
  .end-node .end-node-text {
    color: hsl(var(--foreground) / 0.6);
  }
  .auto-judge .sort-left:hover,
  .auto-judge .sort-right:hover {
    background: hsl(var(--card));
  }
}
</style>

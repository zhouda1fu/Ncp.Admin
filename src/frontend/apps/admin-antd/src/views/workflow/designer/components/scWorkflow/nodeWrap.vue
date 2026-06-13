<template>
	<promoter
		v-if="nodeConfig.type==0"
		:model-value="nodeConfig"
		:view-only="viewOnly"
		@update:model-value="updateNodeConfig"></promoter>

	<approver
		v-if="nodeConfig.type==1"
		:model-value="nodeConfig"
		:view-only="viewOnly"
		@update:model-value="updateNodeConfig"></approver>

	<send
		v-if="nodeConfig.type==2"
		:model-value="nodeConfig"
		:view-only="viewOnly"
		@update:model-value="updateNodeConfig"></send>

	<branch
		v-if="nodeConfig.type==4"
		:model-value="nodeConfig"
		:category="category"
		:view-only="viewOnly"
		@update:model-value="updateNodeConfig">
		<template v-slot="slot">
			<node-wrap
				v-if="slot.node"
				:model-value="slot.node.childNode"
				:category="category"
				:view-only="viewOnly"
				@update:model-value="updateBranchChildNode(slot.node, $event)"></node-wrap>
		</template>
	</branch>

	<node-wrap
		v-if="nodeConfig.childNode"
		:model-value="nodeConfig.childNode"
		:category="category"
		:view-only="viewOnly"
		@update:model-value="updateChildNode"></node-wrap>


</template>

<script>
	import approver from './nodes/approver.vue'
	import promoter from './nodes/promoter.vue'
	import branch from './nodes/branch.vue'
	import send from './nodes/send.vue'

	export default {
		props: {
			modelValue: { type: Object, default: () => {} },
			category: { type: String, default: '' },
			viewOnly: { type: Boolean, default: false }
		},
		components: {
			approver,
			promoter,
			branch,
			send
		},
		data() {
			return {
				nodeConfig: {},
			}
		},
		watch:{
			modelValue(val){
				this.nodeConfig = val
			},
			nodeConfig(val){
				this.$emit("update:modelValue", val)
			}
		},
		mounted() {
			this.nodeConfig = this.modelValue
		},
		methods: {
			updateNodeConfig(val) {
				this.nodeConfig = val || {}
				this.$emit("update:modelValue", this.nodeConfig)
			},
			updateChildNode(val) {
				this.nodeConfig.childNode = val
				this.$emit("update:modelValue", this.nodeConfig)
			},
			updateBranchChildNode(branchNode, val) {
				if (!branchNode) return
				branchNode.childNode = val
				this.$emit("update:modelValue", this.nodeConfig)
			},
		}
	}
</script>

<style>
</style>

<template>
	<promoter v-if="nodeConfig.type==0" v-model="nodeConfig" :view-only="viewOnly"></promoter>

	<approver v-if="nodeConfig.type==1" v-model="nodeConfig" :view-only="viewOnly"></approver>

	<send v-if="nodeConfig.type==2" v-model="nodeConfig" :view-only="viewOnly"></send>

	<branch v-if="nodeConfig.type==4" v-model="nodeConfig" :category="category" :view-only="viewOnly">
		<template v-slot="slot">
			<node-wrap v-if="slot.node" v-model="slot.node.childNode" :category="category" :view-only="viewOnly"></node-wrap>
		</template>
	</branch>

	<node-wrap v-if="nodeConfig.childNode" v-model="nodeConfig.childNode" :category="category" :view-only="viewOnly"></node-wrap>


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

		}
	}
</script>

<style>
</style>

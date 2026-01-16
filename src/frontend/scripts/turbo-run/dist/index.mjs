import { createJiti } from "../../../node_modules/.pnpm/jiti@2.6.1/node_modules/jiti/lib/jiti.mjs";

const jiti = createJiti(import.meta.url, {
  "interopDefault": true,
  "alias": {
    "@vben/turbo-run": "D:/github/Ncp.Admin/frontend/scripts/turbo-run"
  },
  "transformOptions": {
    "babel": {
      "plugins": []
    }
  }
})

/** @type {import("D:/github/Ncp.Admin/frontend/scripts/turbo-run/src/index.js")} */
const _module = await jiti.import("D:/github/Ncp.Admin/frontend/scripts/turbo-run/src/index.ts");

export default _module?.default ?? _module;
import { createJiti } from "../../../node_modules/.pnpm/jiti@2.6.1/node_modules/jiti/lib/jiti.mjs";

const jiti = createJiti(import.meta.url, {
  "interopDefault": true,
  "alias": {
    "@vben/vsh": "D:/github/Ncp.Admin/frontend/scripts/vsh"
  },
  "transformOptions": {
    "babel": {
      "plugins": []
    }
  }
})

/** @type {import("D:/github/Ncp.Admin/frontend/scripts/vsh/src/index.js")} */
const _module = await jiti.import("D:/github/Ncp.Admin/frontend/scripts/vsh/src/index.ts");

export default _module?.default ?? _module;
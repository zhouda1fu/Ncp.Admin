const { createJiti } = require("../../../node_modules/.pnpm/jiti@2.6.1/node_modules/jiti/lib/jiti.cjs")

const jiti = createJiti(__filename, {
  "interopDefault": true,
  "alias": {
    "@vben/tailwind-config": "D:/github/Ncp.Admin/src/frontend/internal/tailwind-config"
  },
  "transformOptions": {
    "babel": {
      "plugins": []
    }
  }
})

/** @type {import("D:/github/Ncp.Admin/src/frontend/internal/tailwind-config/src/index.js")} */
module.exports = jiti("D:/github/Ncp.Admin/src/frontend/internal/tailwind-config/src/index.ts")
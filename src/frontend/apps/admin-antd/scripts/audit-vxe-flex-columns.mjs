import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const viewsDir = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../src/views');

function walk(dir, files = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) walk(full, files);
    else if (/\.(ts|vue)$/.test(entry.name)) files.push(full);
  }
  return files;
}

function hasOp(content) {
  return (
    /field:\s*['"]operation['"][\s\S]*?fixed:\s*['"]right['"]/m.test(content)
    || /fixed:\s*['"]right['"][\s\S]*?field:\s*['"]operation['"]/m.test(content)
  );
}

const missing = [];
const wrongOrder = [];
const noShowOverflow = [];

for (const file of walk(viewsDir)) {
  const content = fs.readFileSync(file, 'utf8');
  if (!hasOp(content)) continue;
  const rel = path.relative(viewsDir, file);
  if (!content.includes('_flex')) missing.push(rel);
  else if (/field:\s*['"]operation['"][\s\S]*?fixed:\s*['"]right['"][\s\S]*?_flex/m.test(content)) {
    wrongOrder.push(rel);
  }
  if (!/field:\s*['"]operation['"][\s\S]*?fixed:\s*['"]right['"][\s\S]*?showOverflow:\s*false/m.test(content)
    && !/fixed:\s*['"]right['"][\s\S]*?field:\s*['"]operation['"][\s\S]*?showOverflow:\s*false/m.test(content)) {
    noShowOverflow.push(rel);
  }
}

console.log('Missing _flex:', missing.length);
missing.forEach((f) => console.log(`  ${f}`));
console.log('Wrong order:', wrongOrder.length);
wrongOrder.forEach((f) => console.log(`  ${f}`));
console.log('Missing showOverflow:false:', noShowOverflow.length);
noShowOverflow.forEach((f) => console.log(`  ${f}`));

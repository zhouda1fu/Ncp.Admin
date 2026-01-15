#!/usr/bin/env node
import { createJiti } from 'jiti';

const jiti = createJiti(import.meta.url);

jiti('../src/index.ts');

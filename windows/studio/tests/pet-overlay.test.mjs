import assert from "node:assert/strict";
import fs from "node:fs/promises";
import path from "node:path";
import vm from "node:vm";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const studioRoot = path.resolve(here, "..");
const template = await fs.readFile(path.join(studioRoot, "engine", "assets", "renderer-inject.js"), "utf8");
const payload = template
  .replace("__DREAM_CSS_JSON__", JSON.stringify(".fixture { color: aqua; }"))
  .replace("__DREAM_ART_JSON__", JSON.stringify("data:image/png;base64,AA=="));

const nodes = new Map();
const rootClasses = new Set(["codex-dream-skin"]);
const rootStyles = new Map([["--dream-art", 'url("blob:stale")']]);

const makeClassList = (classes = new Set()) => ({
  add(...values) { values.forEach((value) => classes.add(value)); },
  remove(...values) { values.forEach((value) => classes.delete(value)); },
  contains(value) { return classes.has(value); },
  toggle(value, enabled) {
    if (enabled) classes.add(value);
    else classes.delete(value);
  },
});

const createNode = (id) => ({
  id,
  classList: makeClassList(),
  remove() { nodes.delete(id); },
});
nodes.set("codex-dream-skin-style", createNode("codex-dream-skin-style"));
nodes.set("codex-dream-skin-chrome", createNode("codex-dream-skin-chrome"));

const root = {
  classList: makeClassList(rootClasses),
  style: {
    setProperty(name, value) { rootStyles.set(name, value); },
    removeProperty(name) { rootStyles.delete(name); },
  },
};

const document = {
  documentElement: root,
  body: {},
  head: root,
  querySelector() { return null; },
  querySelectorAll() { return []; },
  getElementById(id) { return nodes.get(id) ?? null; },
};

const context = {
  window: {},
  document,
  MutationObserver: class {
    observe() {}
    disconnect() {}
  },
  URL: {
    createObjectURL() { return "blob:fixture"; },
    revokeObjectURL() {},
  },
  Blob,
  Uint8Array,
  atob,
  setInterval: () => 1,
  clearInterval() {},
  setTimeout: () => 2,
  clearTimeout() {},
};

const result = vm.runInNewContext(payload, context);
assert.equal(result.installed, true);
assert.equal(rootClasses.has("codex-dream-skin"), false);
assert.equal(rootStyles.has("--dream-art"), false);
assert.equal(nodes.has("codex-dream-skin-style"), false);
assert.equal(nodes.has("codex-dream-skin-chrome"), false);

console.log("PASS: Codex pet and auxiliary renderers stay transparent.");

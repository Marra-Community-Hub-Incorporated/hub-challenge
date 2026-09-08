// First-time local setup, cross-platform. Creates api/local.settings.json from the
// example if it does not exist yet. Safe to run any number of times.
import { copyFileSync, existsSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const repoRoot = join(dirname(fileURLToPath(import.meta.url)), "..");
const example = join(repoRoot, "api", "local.settings.json.example");
const target = join(repoRoot, "api", "local.settings.json");

if (existsSync(target)) {
  console.log("[setup] api/local.settings.json already exists, left untouched.");
} else {
  copyFileSync(example, target);
  console.log("[setup] Created api/local.settings.json. Nothing else to configure.");
}

const MAX_ROWS = 24;

const plannerForm = document.querySelector("#planner-form");
const floorForm = document.querySelector("#floor-form");
const solidRows = document.querySelector("#solid-rows");
const layerRows = document.querySelector("#layer-rows");
const solidTemplate = document.querySelector("#solid-row-template");
const layerTemplate = document.querySelector("#layer-row-template");
const addSolidButton = document.querySelector("#add-solid");
const addLayerButton = document.querySelector("#add-layer");
const planButton = document.querySelector("#plan-button");
const floorButton = document.querySelector("#floor-button");
const runtimeStatus = document.querySelector("#runtime-status");
const results = document.querySelector("#results");
const planSummary = document.querySelector("#plan-summary");
const changeRows = document.querySelector("#change-rows");
const newLayerList = document.querySelector("#new-layer-list");
const newLayerCount = document.querySelector("#new-layer-count");
const floorResult = document.querySelector("#floor-result");

let browserApi;
let hasPlanResult = false;

const outcomeDetails = {
  Moved: { label: "Move", kind: "moved", detail: "" },
  SkippedLockedLayer: { label: "Skip", kind: "skipped", detail: "Current layer is locked." },
  SkippedInheritedMaterial: { label: "Skip", kind: "skipped", detail: "Material inherits from ByLayer or ByBlock." },
  SkippedInvalidName: { label: "Skip", kind: "skipped", detail: "Material name is not valid as a layer name." },
};

const initialSolids = [
  { materialName: "Plywood", isOnLockedLayer: false, nameIsValid: true },
  { materialName: "Steel", isOnLockedLayer: false, nameIsValid: true },
  { materialName: "Birch veneer", isOnLockedLayer: false, nameIsValid: true },
  { materialName: "ByLayer", isOnLockedLayer: false, nameIsValid: true },
  { materialName: "Walnut", isOnLockedLayer: true, nameIsValid: true },
  { materialName: "Aluminum <raw>", isOnLockedLayer: false, nameIsValid: false },
];

const initialLayers = [
  { name: "Plywood", materialName: "Plywood" },
  { name: "Steel", materialName: "Shop primer" },
];

function addSolid(values = { materialName: "", isOnLockedLayer: false, nameIsValid: true }, focusNewRow = false) {
  if (solidRows.children.length >= MAX_ROWS) return;

  const row = solidTemplate.content.firstElementChild.cloneNode(true);
  const materialInput = row.querySelector('[data-field="materialName"]');
  materialInput.value = values.materialName;
  row.querySelector('[data-field="isOnLockedLayer"]').checked = values.isOnLockedLayer;
  row.querySelector('[data-field="nameIsValid"]').checked = values.nameIsValid;
  solidRows.append(row);
  invalidatePlan();
  renumberSolids();
  updateAddButtons();
  if (focusNewRow) materialInput.focus();
}

function addLayer(values = { name: "", materialName: "" }, focusNewRow = false) {
  if (layerRows.children.length >= MAX_ROWS) return;

  const row = layerTemplate.content.firstElementChild.cloneNode(true);
  const nameInput = row.querySelector('[data-field="name"]');
  nameInput.value = values.name;
  row.querySelector('[data-field="materialName"]').value = values.materialName;
  layerRows.append(row);
  invalidatePlan();
  renumberLayers();
  updateAddButtons();
  if (focusNewRow) nameInput.focus();
}

function renumberSolids() {
  [...solidRows.children].forEach((row, index) => {
    const number = index + 1;
    row.querySelector(".row-number").textContent = `Solid ${number}`;
    row.querySelectorAll("input").forEach((input) => {
      const field = input.dataset.field.replace(/[A-Z]/g, (letter) => `-${letter.toLowerCase()}`);
      input.id = `solid-${number}-${field}`;
      input.setAttribute("aria-label", `Solid ${number} ${field.replaceAll("-", " ")}`);
    });
    row.querySelector('[data-action="remove"]').setAttribute("aria-label", `Remove solid ${number}`);
  });
}

function renumberLayers() {
  [...layerRows.children].forEach((row, index) => {
    const number = index + 1;
    row.querySelectorAll("input").forEach((input) => {
      const field = input.dataset.field.replace(/[A-Z]/g, (letter) => `-${letter.toLowerCase()}`);
      input.id = `layer-${number}-${field}`;
      input.setAttribute("aria-label", `Existing layer ${number} ${field.replaceAll("-", " ")}`);
    });
    row.querySelector('[data-action="remove"]').setAttribute("aria-label", `Remove existing layer ${number}`);
  });
}

function updateAddButtons() {
  addSolidButton.disabled = solidRows.children.length >= MAX_ROWS;
  addLayerButton.disabled = layerRows.children.length >= MAX_ROWS;
}

function clearPlanResult() {
  results.hidden = true;
  changeRows.replaceChildren();
  newLayerList.replaceChildren();
  newLayerCount.textContent = "0";
  planSummary.textContent = "";
}

function invalidatePlan() {
  if (!hasPlanResult) return;

  hasPlanResult = false;
  clearPlanResult();
  runtimeStatus.removeAttribute("data-state");
  runtimeStatus.textContent = "Inputs changed. Run the planner again.";
}

function removeRow(event) {
  const button = event.target.closest('[data-action="remove"]');
  if (!button) return;

  const row = button.closest("tr");
  const body = row.parentElement;
  const index = [...body.children].indexOf(row);
  row.remove();
  invalidatePlan();
  if (body === solidRows) renumberSolids();
  else renumberLayers();
  updateAddButtons();

  const nearestRow = body.children[Math.min(index, body.children.length - 1)];
  if (nearestRow) nearestRow.querySelector("input").focus();
  else (body === solidRows ? addSolidButton : addLayerButton).focus();
}

function readSolids() {
  return [...solidRows.children].map((row) => ({
    materialName: row.querySelector('[data-field="materialName"]').value,
    isOnLockedLayer: row.querySelector('[data-field="isOnLockedLayer"]').checked,
    nameIsValid: row.querySelector('[data-field="nameIsValid"]').checked,
  }));
}

function readLayers() {
  return [...layerRows.children].map((row) => ({
    name: row.querySelector('[data-field="name"]').value,
    materialName: row.querySelector('[data-field="materialName"]').value,
  }));
}

function appendCell(row, headerId, label, content, className = "") {
  const cell = document.createElement("td");
  cell.setAttribute("role", "cell");
  cell.setAttribute("headers", headerId);
  cell.dataset.label = label;
  if (className) cell.className = className;
  if (content instanceof Node) cell.append(content);
  else cell.textContent = content;
  row.append(cell);
  return cell;
}

function renderPlan(plan, solids) {
  changeRows.replaceChildren();

  plan.changes.forEach((change, index) => {
    const row = document.createElement("tr");
    row.setAttribute("role", "row");
    const solidHeader = document.createElement("th");
    solidHeader.scope = "row";
    solidHeader.setAttribute("role", "rowheader");
    solidHeader.setAttribute("headers", "change-solid-column");
    solidHeader.dataset.label = "Solid";
    solidHeader.textContent = `Solid ${index + 1}`;
    row.append(solidHeader);

    appendCell(row, "change-material-column", "Material", solids[index]?.materialName ?? "");

    const detail = outcomeDetails[change.outcome] ?? {
      label: change.outcome,
      kind: change.layer ? "moved" : "skipped",
      detail: "Planner returned this outcome.",
    };
    const outcome = document.createElement("span");
    outcome.className = "outcome-badge";
    outcome.dataset.kind = detail.kind;
    outcome.textContent = detail.label;
    appendCell(row, "change-outcome-column", "Outcome", outcome);

    const destination = document.createElement("div");
    destination.className = "destination";
    if (change.layer) {
      const name = document.createElement("strong");
      name.textContent = change.layer;
      destination.append(name);
      const kind = document.createElement("span");
      kind.className = "layer-kind";
      kind.textContent = "Target layer";
      destination.append(kind);
    } else {
      destination.textContent = "No change";
    }
    appendCell(row, "change-destination-column", "Destination", destination);

    let handling = detail.detail;
    if (change.layer) {
      handling = change.useLayerMaterial
        ? "Set solid material to ByLayer."
        : "Keep the solid's own material.";
    }
    appendCell(row, "change-handling-column", "Material handling", handling, "result-detail");
    changeRows.append(row);
  });

  newLayerList.replaceChildren();
  newLayerCount.textContent = plan.newLayers.length;
  if (plan.newLayers.length === 0) {
    const empty = document.createElement("li");
    empty.className = "empty-note";
    empty.textContent = "No layers need to be created.";
    newLayerList.append(empty);
  } else {
    plan.newLayers.forEach((layer) => {
      const item = document.createElement("li");
      const name = document.createElement("strong");
      name.textContent = layer.name;
      const source = document.createElement("span");
      source.textContent = `Colour and material from Solid ${layer.sourceSolid + 1}`;
      item.append(name, source);
      newLayerList.append(item);
    });
  }

  planSummary.textContent = plan.summary;
  results.hidden = false;
  hasPlanResult = true;
  results.focus({ preventScroll: true });
  const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  results.scrollIntoView({ behavior: reduceMotion ? "auto" : "smooth", block: "start" });
}

function showPlannerError(message) {
  hasPlanResult = false;
  clearPlanResult();
  runtimeStatus.dataset.state = "error";
  runtimeStatus.textContent = `Planner error: ${message}`;
}

async function submitPlan(event) {
  event.preventDefault();
  if (!browserApi || !plannerForm.reportValidity()) return;

  planButton.disabled = true;
  planButton.textContent = "Planning…";
  runtimeStatus.dataset.state = "ready";
  runtimeStatus.textContent = "Running Cork.Core…";

  try {
    const solids = readSolids();
    const response = await browserApi.Plan(JSON.stringify({ solids, existingLayers: readLayers() }));
    const plan = JSON.parse(response);
    if (plan.error) throw new Error(plan.error);
    if (!Array.isArray(plan.newLayers) || !Array.isArray(plan.changes)) throw new Error("The planner returned an incomplete result.");

    renderPlan(plan, solids);
    runtimeStatus.dataset.state = "ready";
    runtimeStatus.textContent = "Plan ready · real Cork.Core result";
  } catch (error) {
    showPlannerError(error instanceof Error ? error.message : String(error));
  } finally {
    planButton.disabled = false;
    planButton.textContent = "Plan material layers";
  }
}

function formatOffset(value) {
  if (value === 0) return "0";
  const magnitude = Math.abs(value).toLocaleString(undefined, { maximumFractionDigits: 8 });
  return `${value > 0 ? "+" : "−"}${magnitude}`;
}

async function submitFloor(event) {
  event.preventDefault();
  if (!browserApi || !floorForm.reportValidity()) return;

  floorButton.disabled = true;
  try {
    const pickedZ = Number(document.querySelector("#picked-z").value);
    const offset = await browserApi.Floor(pickedZ);
    floorResult.textContent = `Move the selection by Z ${formatOffset(offset)}. The picked point lands at Z = 0.`;
  } catch (error) {
    floorResult.textContent = `Floor calculation failed: ${error instanceof Error ? error.message : String(error)}`;
  } finally {
    floorButton.disabled = false;
  }
}

async function initializeCore() {
  try {
    const { dotnet } = await import("./_framework/dotnet.js");
    const runtime = await dotnet.create();
    const exports = await runtime.getAssemblyExports(runtime.getConfig().mainAssemblyName);
    browserApi = exports?.Cork?.Demo?.Browser;
    if (typeof browserApi?.Plan !== "function" || typeof browserApi?.Floor !== "function") {
      throw new Error("The browser adapter is unavailable in this build.");
    }

    planButton.disabled = false;
    floorButton.disabled = false;
    runtimeStatus.dataset.state = "ready";
    runtimeStatus.textContent = "Planner core ready";
    floorResult.textContent = "Enter a picked Z coordinate to calculate its move.";
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    runtimeStatus.dataset.state = "error";
    runtimeStatus.textContent = `Planner failed to load: ${message}`;
    floorResult.textContent = "Floor calculation is unavailable because the planner core did not load.";
  }
}

initialSolids.forEach((values) => addSolid(values));
initialLayers.forEach((values) => addLayer(values));
addSolidButton.addEventListener("click", () => addSolid(undefined, true));
addLayerButton.addEventListener("click", () => addLayer(undefined, true));
solidRows.addEventListener("click", removeRow);
layerRows.addEventListener("click", removeRow);
plannerForm.addEventListener("input", invalidatePlan);
plannerForm.addEventListener("submit", submitPlan);
floorForm.addEventListener("submit", submitFloor);
initializeCore();

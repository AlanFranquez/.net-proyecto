export const BROWSER_ID_KEY = "e_browser_id";

export function getBrowserId() {
  let id = localStorage.getItem(BROWSER_ID_KEY);
  if (!id) {
    id = crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`;
    localStorage.setItem(BROWSER_ID_KEY, id);
  }
  return id;
}

export function clearBrowserId() {
  localStorage.removeItem(BROWSER_ID_KEY);
}

/** DOM-building helpers that keep untrusted data out of the HTML parser.
 *  Prefer these over `innerHTML`: text goes through `textContent` and
 *  URLs/attributes through property assignment, so markup injection is
 *  impossible by construction. */

const template = document.createElement('template');

/** Parses a trusted, developer-authored SVG string once and returns a
 *  factory that stamps out clones. Only pass compile-time constants —
 *  never runtime or API data. */
export function staticIcon(svg: string): () => SVGElement {
  template.innerHTML = svg.trim();
  const node = template.content.firstElementChild as SVGElement | null;
  template.replaceChildren();
  if (!node) throw new Error('staticIcon: empty or invalid SVG markup');
  return () => node.cloneNode(true) as SVGElement;
}

/** Allows http(s) and relative URLs only; anything with another scheme
 *  (e.g. `javascript:`) falls back to `#`. Use for href/src values that
 *  come from an API. */
export function safeUrl(url: string): string {
  if (!url) return '#';
  if (/^[a-z][a-z0-9+.-]*:/i.test(url)) {
    return /^https?:/i.test(url) ? url : '#';
  }
  return url;
}

type MessageType = 'success' | 'error';

export function initLinkSender(container: HTMLElement, endpoint: string): void {
  const input = container.querySelector('#stringInput') as HTMLInputElement;
  const addBtn = container.querySelector('#addBtn') as HTMLButtonElement;
  const sendBtn = container.querySelector('#sendBtn') as HTMLButtonElement;
  const clearBtn = container.querySelector('#clearBtn') as HTMLButtonElement;
  const listEl = container.querySelector('#stringList') as HTMLUListElement;
  const messageEl = container.querySelector('#message') as HTMLParagraphElement;
  const countBadge = container.querySelector('#countBadge') as HTMLSpanElement;

  const links: string[] = [];

  function isValidUrl(raw: string): boolean {
    try {
      new URL(raw.startsWith('http') ? raw : `https://${raw}`);
      return true;
    } catch {
      return false;
    }
  }

  function extractDomain(url: string): string {
    try {
      return new URL(url.startsWith('http') ? url : `https://${url}`).hostname.replace(/^www\./, '');
    } catch {
      return url;
    }
  }

  function updateUI(): void {
    listEl.replaceChildren();
    links.forEach((link, index) => {
      const li = document.createElement('li');

      const domain = extractDomain(link);
      const favicon = document.createElement('img');
      favicon.className = 'link-icon';
      favicon.src = `https://www.google.com/s2/favicons?domain=${encodeURIComponent(domain)}&sz=32`;
      favicon.alt = '';
      favicon.onerror = () => { favicon.style.display = 'none'; };

      const textSpan = document.createElement('span');
      textSpan.className = 'link-text';
      textSpan.textContent = link;

      const domainBadge = document.createElement('span');
      domainBadge.className = 'link-domain';
      domainBadge.textContent = domain;

      const removeBtn = document.createElement('button');
      removeBtn.className = 'remove-btn';
      removeBtn.textContent = '×';
      removeBtn.setAttribute('aria-label', `Remove ${link}`);
      removeBtn.addEventListener('click', () => {
        links.splice(index, 1);
        updateUI();
      });

      li.append(favicon, textSpan, domainBadge, removeBtn);
      listEl.appendChild(li);
    });

    sendBtn.disabled = links.length === 0;
    countBadge.textContent = String(links.length);
    clearBtn.style.display = links.length ? 'inline-flex' : 'none';
  }

  let messageTimer: number | undefined;
  function setMessage(text: string, type: MessageType): void {
    if (messageTimer) clearTimeout(messageTimer);
    messageEl.textContent = text;
    messageEl.className = `message ${type}`;
    messageTimer = window.setTimeout(() => {
      messageEl.textContent = '';
      messageEl.className = 'message';
    }, 3500);
  }

  function processInput(raw: string): void {
    const items = raw.split(/[,\n]+/).map(s => s.trim()).filter(Boolean);
    const added: string[] = [];
    const invalid: string[] = [];

    for (const item of items) {
      if (isValidUrl(item)) {
        if (!links.includes(item)) {
          links.push(item);
          added.push(item);
        }
      } else {
        invalid.push(item);
      }
    }

    if (added.length) {
      updateUI();
      setMessage(`Added ${added.length} link(s).`, 'success');
    }
    if (invalid.length) {
      setMessage(`Skipped: ${invalid.join(', ')}`, 'error');
    }
    if (!added.length && invalid.length === 0 && items.length) {
      setMessage('No valid URLs found.', 'error');
    }
  }

  addBtn.addEventListener('click', () => {
    const value = input.value.trim();
    if (!value) return;
    processInput(value);
    input.value = '';
    input.focus();
  });

  input.addEventListener('keypress', (e: KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      addBtn.click();
    }
  });

  input.addEventListener('paste', (e: ClipboardEvent) => {
    const pasted = e.clipboardData?.getData('text/plain') || '';
    if (pasted.includes(',') || pasted.includes('\n')) {
      e.preventDefault();
      processInput(pasted);
      input.value = '';
    }
  });

  clearBtn.addEventListener('click', () => {
    links.length = 0;
    updateUI();
    setMessage('List cleared.', 'success');
  });

  sendBtn.addEventListener('click', async () => {
    if (links.length === 0) return;

    sendBtn.disabled = true;
    const originalText = sendBtn.textContent;
    sendBtn.textContent = 'Sending...';

    try {
      const response = await fetch(endpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify([...links]),
      });
      if (!response.ok) {
        const errorText = await response.text().catch(() => 'Unknown error');
        throw new Error(errorText || `HTTP ${response.status}`);
      }
      links.length = 0;
      updateUI();
      setMessage('List sent successfully!', 'success');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Failed to send list.';
      setMessage(message, 'error');
    } finally {
      sendBtn.textContent = originalText;
      sendBtn.disabled = links.length === 0;
    }
  });
}
import { staticIcon } from '@/lib/dom';

function initials(name: string): string {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0].toUpperCase())
    .join('');
}

const deleteIcon = staticIcon(
  '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0h10"/><path d="M10 11v6"/><path d="M14 11v6"/></svg>',
);

const STYLE = `
  :host {
    display: block;
    font-family: inherit;
  }

  .user {
    display: grid;
    grid-template-columns: auto 1fr auto auto;
    align-items: center;
    gap: 1rem;
    padding: 1rem 1.25rem;
    background: var(--surface);
    border: 1px solid var(--border-soft);
    border-radius: 14px;
    transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease;
  }

  .user:hover {
    transform: translateY(-2px);
    box-shadow: 0 16px 40px -24px var(--shadow);
    border-color: var(--border);
  }

  .avatar {
    width: 46px;
    height: 46px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--ink);
    color: var(--on-ink);
    font-size: 0.95rem;
    font-weight: 700;
    letter-spacing: 0.02em;
    flex-shrink: 0;
  }

  .details {
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: 0.2rem;
  }

  .name {
    font-family: 'Spectral', serif;
    font-size: 1.15rem;
    line-height: 1.25;
    color: var(--text);
    margin: 0;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .email {
    font-size: 0.85rem;
    color: var(--muted);
    margin: 0;
  }

  .role {
    font-size: 0.7rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.07em;
    padding: 0.35rem 0.75rem;
    border-radius: 999px;
    white-space: nowrap;
  }

  .role--Admin {
    background: var(--accent);
    color: var(--on-ink);
  }

  .role--User {
    background: var(--chip-bg);
    color: var(--text-2);
  }

  .role--PremiumUser {
    background: var(--ink);
    color: var(--on-ink);
  }

  .delete {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border: none;
    border-radius: 10px;
    background: transparent;
    color: var(--faint);
    cursor: pointer;
    transition: color 0.15s, background 0.15s;
    line-height: 0;
  }

  .delete:hover {
    color: var(--accent);
    background: var(--chip-bg);
  }

  @media (max-width: 520px) {
    .user {
      grid-template-columns: auto 1fr auto;
      gap: 0.75rem;
    }
    .role {
      grid-column: 2 / 3;
      justify-self: start;
    }
    .delete {
      grid-row: 1 / 3;
      grid-column: 3 / 4;
      align-self: center;
    }
  }
`;

export class UserCard extends HTMLElement {
  constructor() {
    super();
    this.attachShadow({ mode: 'open' });
  }

  connectedCallback(): void {
    this.render();
  }

  private render(): void {
    const id = this.getAttribute('data-id') || '';
    const name = this.getAttribute('data-name') || '';
    const email = this.getAttribute('data-email') || '';
    const role = (this.getAttribute('data-role') || 'User') as import('@/api').UserRole;
    const created = this.getAttribute('data-created') || '';

    const dateText = created ? new Date(created).toLocaleDateString() : '';

    const avatar = document.createElement('div');
    avatar.className = 'avatar';
    avatar.setAttribute('aria-hidden', 'true');
    avatar.textContent = initials(name);

    const nameEl = document.createElement('p');
    nameEl.className = 'name';
    nameEl.title = name;
    nameEl.textContent = name;

    const emailEl = document.createElement('p');
    emailEl.className = 'email';
    emailEl.textContent = email + (dateText ? ` · joined ${dateText}` : '');

    const details = document.createElement('div');
    details.className = 'details';
    details.append(nameEl, emailEl);

    const roleEl = document.createElement('span');
    roleEl.className = `role role--${role}`;
    roleEl.title = `Role: ${role}`;
    roleEl.textContent = role;

    const deleteBtn = document.createElement('button');
    deleteBtn.type = 'button';
    deleteBtn.className = 'delete';
    deleteBtn.title = 'Delete user';
    deleteBtn.setAttribute('aria-label', `Delete ${name}`);
    deleteBtn.appendChild(deleteIcon());

    const article = document.createElement('article');
    article.className = 'user';
    article.append(avatar, details, roleEl, deleteBtn);

    const style = document.createElement('style');
    style.textContent = STYLE;

    this.shadowRoot!.replaceChildren(style, article);

    deleteBtn.addEventListener('click', () => {
      this.dispatchEvent(
        new CustomEvent('user-delete', {
          bubbles: true,
          composed: true,
          detail: { id, name },
        }),
      );
    });
  }
}

if (!customElements.get('user-card')) {
  customElements.define('user-card', UserCard);
}

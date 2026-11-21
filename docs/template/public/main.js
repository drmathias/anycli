// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

export default {
  start: () => {
    const navbar = document.getElementById('navbar');
    if (!navbar) {
      return;
    }

    const addApiReference = () => {
      const menu = navbar.querySelector('.navbar-nav');
      if (!menu || menu.querySelector('[data-api-reference]')) {
        return;
      }

      const rel = document.querySelector('meta[name="docfx:rel"]')?.content ?? '';
      const isApiPage = window.location.pathname.includes('/api/');
      const item = document.createElement('li');
      item.className = 'nav-item';

      const link = document.createElement('a');
      link.className = `nav-link${isApiPage ? ' active' : ''}`;
      link.href = `${rel}api/AnyCli.html`;
      link.textContent = 'API Reference';
      link.dataset.apiReference = 'true';

      if (isApiPage) {
        link.setAttribute('aria-current', 'page');
      }

      item.append(link);
      menu.append(item);
    };

    new MutationObserver(addApiReference).observe(
      navbar,
      { childList: true, subtree: true });
  }
};

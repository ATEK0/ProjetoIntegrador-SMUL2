/**
 * Dropdown customizado — mantém <select> nativo para formulários e validação.
 */
(function (global) {
    'use strict';

    const ENHANCED = 'data-ds-enhanced';
    const SKIP = 'data-ds-dropdown-skip';

    function getLabel(option) {
        return (option && option.textContent.trim()) || '';
    }

    function isPlaceholderOption(option) {
        return !option || option.value === '' || option.disabled;
    }

    function buildMenu(wrap, select, triggerLabel) {
        const menu = wrap.querySelector('.ds-dropdown-menu');
        menu.innerHTML = '';
        let focusedIndex = -1;

        Array.from(select.options).forEach(function (option, index) {
            if (option.disabled && option.value === '') return;

            const li = document.createElement('li');
            li.className = 'ds-dropdown-option';
            li.setAttribute('role', 'option');
            li.setAttribute('data-value', option.value);
            li.textContent = getLabel(option);

            if (option.disabled) li.classList.add('is-disabled');
            if (option.selected) {
                li.classList.add('is-selected');
                focusedIndex = index;
            }

            li.addEventListener('click', function (e) {
                e.preventDefault();
                if (option.disabled) return;
                select.value = option.value;
                select.dispatchEvent(new Event('change', { bubbles: true }));
                closeDropdown(wrap);
                syncUI(wrap, select, triggerLabel);
            });

            menu.appendChild(li);
        });

        wrap._focusedIndex = focusedIndex;
    }

    function syncUI(wrap, select, triggerLabel) {
        const selected = select.options[select.selectedIndex];
        const label = getLabel(selected);
        const isPlaceholder = isPlaceholderOption(selected);

        triggerLabel.textContent = label || 'Selecionar…';
        triggerLabel.classList.toggle('is-placeholder', isPlaceholder);

        wrap.querySelectorAll('.ds-dropdown-option').forEach(function (opt) {
            const match = opt.getAttribute('data-value') === select.value;
            opt.classList.toggle('is-selected', match);
            opt.classList.toggle('is-focused', false);
        });
    }

    function closeDropdown(wrap) {
        wrap.classList.remove('is-open');
        const trigger = wrap.querySelector('.ds-dropdown-trigger');
        if (trigger) trigger.setAttribute('aria-expanded', 'false');
    }

    function closeAllExcept(exceptWrap) {
        document.querySelectorAll('.ds-dropdown-wrap.is-open').forEach(function (w) {
            if (w !== exceptWrap) closeDropdown(w);
        });
    }

    function openDropdown(wrap) {
        closeAllExcept(wrap);
        wrap.classList.add('is-open');
        const trigger = wrap.querySelector('.ds-dropdown-trigger');
        trigger.setAttribute('aria-expanded', 'true');
        const selected = wrap.querySelector('.ds-dropdown-option.is-selected');
        if (selected) selected.scrollIntoView({ block: 'nearest' });
    }

    function focusOptionByIndex(wrap, index) {
        const options = Array.from(wrap.querySelectorAll('.ds-dropdown-option:not(.is-disabled)'));
        if (!options.length) return;
        index = Math.max(0, Math.min(index, options.length - 1));
        options.forEach(function (o, i) {
            o.classList.toggle('is-focused', i === index);
        });
        wrap._focusedIndex = index;
        options[index].scrollIntoView({ block: 'nearest' });
    }

    function enhance(select) {
        if (!select || select.tagName !== 'SELECT') return null;
        if (select.getAttribute(ENHANCED) === 'true') return select.closest('.ds-dropdown-wrap');
        if (select.hasAttribute(SKIP)) return null;

        const parent = select.parentElement;
        if (parent && parent.classList.contains('ds-dropdown-wrap')) return parent;

        const wrap = document.createElement('div');
        wrap.className = 'ds-dropdown-wrap';

        if (select.classList.contains('ds-dropdown-compact') || select.dataset.dsSize === 'compact') {
            wrap.classList.add('ds-dropdown--compact');
        }
        if (select.dataset.dsInline === 'true') {
            wrap.classList.add('ds-dropdown--inline');
        }
        if (select.classList.contains('role-professor')) wrap.classList.add('ds-dropdown--role-professor');
        if (select.classList.contains('role-aluno')) wrap.classList.add('ds-dropdown--role-aluno');
        if (select.dataset.dsAccent === 'true') wrap.classList.add('ds-dropdown--accent');

        const trigger = document.createElement('button');
        trigger.type = 'button';
        trigger.className = 'ds-dropdown-trigger';
        trigger.setAttribute('aria-haspopup', 'listbox');
        trigger.setAttribute('aria-expanded', 'false');

        const triggerLabel = document.createElement('span');
        triggerLabel.className = 'ds-dropdown-label';
        trigger.appendChild(triggerLabel);

        const chevron = document.createElement('i');
        chevron.className = 'fas fa-chevron-down ds-dropdown-chevron';
        chevron.setAttribute('aria-hidden', 'true');

        const menu = document.createElement('ul');
        menu.className = 'ds-dropdown-menu';
        menu.setAttribute('role', 'listbox');

        select.classList.add('ds-dropdown-source');
        select.setAttribute(ENHANCED, 'true');

        const id = select.id || 'ds-select-' + Math.random().toString(36).slice(2, 9);
        if (!select.id) select.id = id;
        trigger.setAttribute('aria-labelledby', id + '-label');
        menu.id = id + '-listbox';

        select.parentNode.insertBefore(wrap, select);
        wrap.appendChild(select);
        wrap.appendChild(trigger);
        wrap.appendChild(chevron);
        wrap.appendChild(menu);

        if (select.disabled) wrap.classList.add('is-disabled');

        buildMenu(wrap, select, triggerLabel);
        syncUI(wrap, select, triggerLabel);

        trigger.addEventListener('click', function () {
            if (select.disabled) return;
            if (wrap.classList.contains('is-open')) closeDropdown(wrap);
            else openDropdown(wrap);
        });

        trigger.addEventListener('keydown', function (e) {
            if (select.disabled) return;
            const options = wrap.querySelectorAll('.ds-dropdown-option:not(.is-disabled)');
            let idx = wrap._focusedIndex >= 0 ? wrap._focusedIndex : 0;

            if (e.key === 'ArrowDown' || e.key === 'ArrowUp' || e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
            }

            if (e.key === 'Escape') {
                closeDropdown(wrap);
                return;
            }

            if (!wrap.classList.contains('is-open')) {
                if (e.key === 'ArrowDown' || e.key === 'Enter' || e.key === ' ') {
                    openDropdown(wrap);
                    focusOptionByIndex(wrap, idx);
                }
                return;
            }

            if (e.key === 'ArrowDown') {
                idx = Math.min(idx + 1, options.length - 1);
                focusOptionByIndex(wrap, idx);
            } else if (e.key === 'ArrowUp') {
                idx = Math.max(idx - 1, 0);
                focusOptionByIndex(wrap, idx);
            } else if (e.key === 'Enter' || e.key === ' ') {
                const focused = wrap.querySelector('.ds-dropdown-option.is-focused') ||
                    wrap.querySelector('.ds-dropdown-option.is-selected');
                if (focused) focused.click();
            }
        });

        select.addEventListener('change', function () {
            syncUI(wrap, select, triggerLabel);
        });

        const observer = new MutationObserver(function () {
            buildMenu(wrap, select, triggerLabel);
            syncUI(wrap, select, triggerLabel);
        });
        observer.observe(select, { childList: true, subtree: true, attributes: true, attributeFilter: ['selected'] });
        wrap._dsObserver = observer;

        return wrap;
    }

    function refresh(select) {
        if (!select) return;
        const wrap = select.closest('.ds-dropdown-wrap');
        if (!wrap) {
            enhance(select);
            return;
        }
        const triggerLabel = wrap.querySelector('.ds-dropdown-label');
        buildMenu(wrap, select, triggerLabel);
        syncUI(wrap, select, triggerLabel);
    }

    function enhanceAll(root) {
        const scope = root || document;
        scope.querySelectorAll('select.form-control, select.ds-dropdown, select.ds-dropdown-compact, select[data-ds-dropdown]').forEach(function (sel) {
            if (!sel.hasAttribute(SKIP)) enhance(sel);
        });
    }

    document.addEventListener('click', function (e) {
        if (!e.target.closest('.ds-dropdown-wrap')) {
            document.querySelectorAll('.ds-dropdown-wrap.is-open').forEach(closeDropdown);
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            document.querySelectorAll('.ds-dropdown-wrap.is-open').forEach(closeDropdown);
        }
    });

    function init() {
        enhanceAll();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    global.DsDropdown = {
        enhance: enhance,
        refresh: refresh,
        enhanceAll: enhanceAll
    };
})(window);

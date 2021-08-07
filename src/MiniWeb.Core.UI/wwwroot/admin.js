let options;
const log = function (...args) {
    if (localStorage.getItem("showLog") === "true") {
        console.log(...args);
    }
};
const extend = function (defaults, options) {
    const extended = {};
    let prop;
    for (prop in defaults) {
        if (Object.prototype.hasOwnProperty.call(defaults, prop)) {
            extended[prop] = defaults[prop];
        }
    }
    for (prop in options) {
        if (Object.prototype.hasOwnProperty.call(options, prop)) {
            extended[prop] = options[prop];
        }
    }
    return extended;
};
const hotKeys = [
    ['b', false, 'bold'],
    ['i', false, 'italic'],
    ['u', false, 'underline'],
    ['z', false, 'undo'],
    ['z', true, 'redo']
];
const executeHotkey = function (e) {
    log('executeHotkey', e);
    if (e.ctrlKey || e.metaKey) {
        for (let i = 0; i < hotKeys.length; i++) {
            const hotkeySetting = hotKeys[i];
            if (e.key === hotkeySetting[0] && e.shiftKey === hotkeySetting[1]) {
                e.preventDefault();
                e.stopPropagation();
                document.execCommand(hotkeySetting[2]);
            }
        }
    }
};
const bindHotKeys = function (element) {
    element.addEventListener('keydown', executeHotkey);
};
const unbindHotKeys = function (element) {
    element.removeEventListener('keydown', executeHotkey);
};
let selectedRange;
const getCurrentRange = function () {
    const sel = window.getSelection();
    if (sel.getRangeAt && sel.rangeCount) {
        return sel.getRangeAt(0);
    }
};
const saveSelection = function () {
    selectedRange = getCurrentRange();
};
const restoreSelection = function () {
    const selection = window.getSelection();
    if (selectedRange) {
        try {
            selection.removeAllRanges();
        }
        catch (ex) {
            document.createRange().select();
            document.selection.empty();
        }
        selection.addRange(selectedRange);
        selection.anchorNode.parentElement.focus();
    }
};
const htmlEscape = function (str) {
    return str
        .replace(/&/g, '&')
        .replace(/'/g, "'")
        .replace(/"/g, '"')
        .replace(/>/g, '>')
        .replace(/</g, '<');
};
const closeModals = function () {
    document.querySelectorAll('.miniweb-modal.show').forEach(el => {
        el.classList.remove('show');
    });
};
const txtMessage = document.querySelector("miniwebadmin .alert");
let messageTimeout;
const showMessage = function (success, message, isHtml = false) {
    log('showMessage', ...arguments);
    const className = success ? "miniweb-alert-success" : "miniweb-alert-danger";
    const timeout = success ? 4000 : 8000;
    if (messageTimeout) {
        clearTimeout(messageTimeout);
    }
    txtMessage.classList.remove(...txtMessage.classList);
    txtMessage.classList.add('alert');
    txtMessage.classList.add(className);
    if (isHtml)
        txtMessage.innerHTML = message;
    else
        txtMessage.innerHTML = htmlEscape(message);
    txtMessage.parentElement.classList.remove("is-hidden");
    messageTimeout = setTimeout(function () {
        txtMessage.classList.remove(className);
        txtMessage.parentElement.classList.add("is-hidden");
    }, timeout);
};
const checkAssetPagerVisibility = function (isFirst, lastPage) {
    if (lastPage) {
        document.getElementById('miniweb-asset-page-right').classList.add('is-hidden');
    }
    else {
        document.getElementById('miniweb-asset-page-right').classList.remove('is-hidden');
    }
    if (isFirst) {
        document.getElementById('miniweb-asset-page-left').classList.add('is-hidden');
    }
    else {
        document.getElementById('miniweb-asset-page-left').classList.remove('is-hidden');
    }
};
const assetPageList = document.querySelector('.miniweb-assetlist');
const showAssetPage = function (page) {
    const pageSize = 15;
    const folder = (document.querySelector('[name="miniwebAssetFolder"]')).value;
    const assetList = document.querySelector('.miniweb-assetlist');
    assetPageList.dataset.miniwebPage = page.toString();
    assetList.innerHTML = '';
    const addAsset = function (asset) {
        const li = document.createElement('li');
        li.dataset.miniwebPath = asset.folder;
        if (asset.type === 0) {
            li.innerHTML = '<img data-miniweb-src="' + asset.virtualPath + '" src="' + asset.virtualPath + '" data-miniweb-relpath="' + asset.virtualPath + '" class="miniweb-asset-pick" >';
        }
        else {
            li.innerHTML = '<span data-miniweb-relpath="' + asset.virtualPath + '" class="miniweb-asset-pick"  >' + asset.fileName + '</span>';
        }
        assetList.appendChild(li);
    };
    const url = new URL(options.apiEndpoint + 'allassets', document.location.origin);
    url.searchParams.set('page', page.toString());
    url.searchParams.set('folder', folder);
    url.searchParams.set('take', pageSize.toString());
    console.log('fetching url', url.toString(), url);
    let isLast = false;
    fetch(url.toString(), { headers: { "RequestVerificationToken": options.afToken } })
        .then(res => res.json())
        .then(data => {
        if (data.assets !== null && data.assets.length > 0) {
            for (let i = 0; i < data.assets.length; i++) {
                addAsset(data.assets[i]);
            }
        }
        if (data.totalAssets <= (page + 1) * pageSize)
            isLast = true;
        checkAssetPagerVisibility(page === 0, isLast);
    });
};
const toggleHiddenMenuItems = function (on) {
    const items = document.querySelectorAll('.miniweb-hidden-menu');
    items.forEach((item) => {
        if (on) {
            item.classList.add('show');
        }
        else {
            item.classList.remove('show');
        }
    });
};
const toggleContentInserts = function (on) {
    if (on) {
        document.querySelectorAll('[data-miniwebsection]').forEach((el) => {
            const section = el.dataset.miniwebsection;
            el.insertAdjacentHTML('beforeend', '<button class="miniweb-button miniweb-insertcontent" data-miniweb-add-content-to="' + section + '">add content</button>');
        });
        document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate] .miniweb-template-actions').forEach(el => el.remove());
        document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate]').forEach(el => el.insertAdjacentHTML('beforeend', '<div class="pull-right miniweb-template-actions"><button tabindex="-1" class="miniweb-button" data-miniweb-content-move="up" title="Move up">&#11014;</button><button tabindex="-1" class="miniweb-button" data-miniweb-content-move="down" title="Move down">&#11015;</button>	<button tabindex="-1" class="miniweb-button miniweb-danger" data-miniweb-content-move="delete" title="Delete item">&#11199;</button></div>'));
    }
    else {
        document.querySelectorAll('.miniweb-insertcontent, .miniweb-template-actions').forEach(el => el.remove());
    }
};
const editContent = function () {
    closeModals();
    document.querySelector('body').classList.add('miniweb-editing');
    const contentEditables = document.querySelectorAll('[data-miniwebprop]');
    contentEditables.forEach((el) => {
        if (el.classList.contains('miniweb-input-value')) {
            el.value = el.dataset.miniwebinputvalue;
            return;
        }
        el.setAttribute('contentEditable', "true");
    });
    for (let i = 0; i < options.editTypes.length; i++) {
        const editType = options.editTypes[i];
        contentEditables.forEach((ce, ix) => {
            if (ce.dataset.miniwebedittype === editType.key) {
                editType.editStart(ce, ix);
            }
        });
    }
    const btnPageProperties = document.getElementById("miniweb-button-pageprops");
    const btnNew = document.getElementById("miniweb-button-newpage");
    const btnEdit = document.getElementById("miniweb-button-edit");
    const btnSave = document.getElementById("miniweb-button-save");
    const btnCancel = document.getElementById("miniweb-button-cancel");
    btnPageProperties.setAttribute("disabled", "true");
    btnNew.setAttribute("disabled", "true");
    btnEdit.setAttribute("disabled", "true");
    btnSave.removeAttribute("disabled");
    btnCancel.removeAttribute("disabled");
    toggleContentInserts(true);
};
const cancelEdit = function () {
    document.querySelector('body').classList.remove('miniweb-editing');
    const contentEditables = document.querySelectorAll('[data-miniwebprop]');
    contentEditables.forEach(el => el.removeAttribute('contentEditable'));
    for (let i = 0; i < options.editTypes.length; i++) {
        const editType = options.editTypes[i];
        contentEditables.forEach((ce, ix) => {
            if (ce.dataset.miniwebedittype === editType.key) {
                editType.editEnd(ce, ix);
                return;
            }
        });
    }
    const btnPageProperties = document.getElementById("miniweb-button-pageprops");
    const btnNew = document.getElementById("miniweb-button-newpage");
    const btnEdit = document.getElementById("miniweb-button-edit");
    const btnSave = document.getElementById("miniweb-button-save");
    const btnCancel = document.getElementById("miniweb-button-cancel");
    btnPageProperties.removeAttribute("disabled");
    btnNew.removeAttribute("disabled");
    btnEdit.removeAttribute("disabled");
    btnSave.setAttribute("disabled", "true");
    btnCancel.setAttribute("disabled", "true");
    toggleContentInserts(false);
    closeModals();
};
const getItemValue = function (source) {
    var _a;
    log('getItemValue', source.contentEditable);
    if (source.contentEditable === "true") {
        let parsedDOM;
        parsedDOM = new DOMParser().parseFromString(source.innerHTML, 'text/html');
        parsedDOM = new XMLSerializer().serializeToString(parsedDOM);
        const result = /<body>([\s\S]*?)(<br \/>)?<\/body>/im.exec(parsedDOM);
        parsedDOM = result[1];
        return parsedDOM;
    }
    return (_a = source) === null || _a === void 0 ? void 0 : _a.value;
};
const saveContent = function () {
    if (!document.querySelector('body').classList.contains('miniweb-editing'))
        return;
    document.querySelectorAll('.miniweb-editing-source').forEach((content) => {
        content.innerHTML = content.firstElementChild.innerText;
    });
    let valid = true;
    const items = [];
    document.querySelectorAll('[data-miniwebsection]').forEach((section, index) => {
        const sectionid = section.dataset.miniwebsection;
        section.querySelectorAll('[data-miniwebtemplate]').forEach((tmpl) => {
            log('item', tmpl);
            if (!items[index]) {
                items[index] = {};
                items[index].Key = sectionid;
                items[index].Items = [];
            }
            const item = {
                Template: tmpl.dataset.miniwebtemplate,
                Values: {}
            };
            tmpl.querySelectorAll('[data-miniwebprop]').forEach((prop) => {
                const key = prop.dataset.miniwebprop;
                const value = getItemValue(prop);
                const validation = prop.dataset.miniwebValidation;
                prop.classList.remove('miniweb-invalid-item');
                if (validation === 'required') {
                    if (!value || value === '<br />') {
                        valid = false;
                        prop.classList.add('miniweb-invalid-item');
                    }
                }
                else if (validation) {
                    const reg = new RegExp(validation);
                    if (!reg.test(value)) {
                        valid = false;
                        prop.classList.add('miniweb-invalid-item');
                    }
                }
                if (prop.contentEditable == "true") {
                    prop.innerHTML = value;
                }
                else if (prop != null) {
                    const ht = prop;
                    ht.value = value;
                }
                log('itemfound', key, '[' + value + ']', validation, valid);
                item.Values[key] = value;
                if (key.indexOf(':') > 0) {
                    const orig = key.split(':')[0];
                    const attrib = key.split(':')[1];
                    tmpl.querySelector('[data-miniwebprop="' + orig + '"]').setAttribute(attrib, value);
                }
            });
            items[index].Items.push(item);
        });
    });
    if (!valid) {
        showMessage(false, "Check item validity");
        return;
    }
    const data = new FormData();
    data.append('url', document.getElementById("miniweb-admin-nav").dataset.miniwebPath);
    data.append('items', JSON.stringify(items));
    data.append('__RequestVerificationToken', options.afToken);
    fetch(options.apiEndpoint + "savecontent", {
        method: "POST",
        body: data
    }).then(res => res.json())
        .then(data => {
        log(data);
        if (data.result)
            showMessage(true, "The page was saved successfully");
        else
            showMessage(false, "Save page failed");
        cancelEdit();
    });
};
const savePage = function () {
    const form = document.querySelector('.show.miniweb-pageproperties form');
    form.classList.remove('miniweb-invalid');
    if (!form.checkValidity()) {
        showMessage(false, "Check form validity");
        form.classList.add('miniweb-invalid');
        return;
    }
    const formData = new FormData(form);
    formData.append('__RequestVerificationToken', options.afToken);
    fetch(options.apiEndpoint + "savepage", {
        method: "POST",
        body: formData
    }).then(res => res.json())
        .then(data => {
        if (data.result) {
            showMessage(true, "saved page successfully, reloading...");
            localStorage.setItem('miniweb-reloadedmessage', 'page saved successfully');
            window.setTimeout(() => document.location.reload(), 1000);
        }
        else {
            showMessage(false, data.message);
        }
    }).catch(err => {
        console.error(err);
        showMessage(false, 'failed to post');
    });
    form.querySelector('[name="NewPage"]').value = "false";
};
const confirmDialog = function (message, action) {
    const modal = document.querySelector('#miniweb-confirm');
    modal.querySelector('h4').innerHTML = message;
    const oldButton = modal.querySelector('[data-miniweb-confirm]');
    var newButton = oldButton.cloneNode(true);
    oldButton.parentNode.replaceChild(newButton, oldButton);
    newButton.addEventListener('click', () => {
        action();
        closeModals();
    });
    modal.classList.add('show');
    modal.scrollIntoView(true);
};
const removePage = function () {
    confirmDialog('Do you want to remove the page?', () => {
        const adminNav = document.getElementById('miniweb-admin-nav');
        const formData = new FormData();
        formData.append('__RequestVerificationToken', options.afToken);
        formData.append('url', adminNav.dataset.miniwebPath);
        fetch(options.apiEndpoint + "removepage", {
            method: "POST",
            body: formData
        }).then(res => res.json())
            .then(data => {
            if (data.result) {
                showMessage(true, "removed page successfully");
                setTimeout(function () {
                    document.location.href = data.url;
                }, 1000);
            }
            else {
                showMessage(false, data.message);
            }
        }).catch(err => {
            console.error(err);
            showMessage(false, 'failed to delete');
        });
    });
};
const addNewPageModal = function () {
    const modal = document.querySelector('.miniweb-pageproperties');
    modal.classList.add("miniweb-modal-right");
    const form = modal.querySelector('form');
    form.classList.remove('miniweb-invalid');
    const elems = form.querySelectorAll('input,textarea');
    for (let i = 0; i < elems.length; i++) {
        const elem = elems[i];
        switch (elem.name) {
            case 'NewPage':
                elem.value = "true";
                break;
            case 'Date':
                elem.value = elem.dataset.miniwebNewdate;
                break;
            case 'Layout': break;
            default:
                elem.value = null;
                break;
        }
    }
    modal.classList.add('show');
    modal.scrollIntoView(true);
};
const ctrlSsave = function (event) {
    if (document.querySelector('body').classList.contains('miniweb-editing')) {
        if (event.ctrlKey && event.code === 'KeyS') {
            event.preventDefault();
            saveContent();
        }
        ;
    }
    else {
        const modal = document.querySelector('.miniweb-pageproperties');
        if (modal.classList.contains('show')) {
            if (event.ctrlKey && event.code === 'KeyS') {
                event.preventDefault();
                savePage();
            }
        }
    }
};
const addLink = function () {
    const modal = document.getElementById('miniweb-addHyperLink');
    const contentEditables = document.querySelectorAll('[data-miniwebprop]');
    let href = modal.querySelector('[name="InternalUrl"]').value;
    if (!href)
        href = modal.querySelector('[name="Url"]').value;
    if (modal.dataset.miniwebLinkType === 'HTML') {
        restoreSelection();
        document.execCommand("unlink", false, null);
        document.execCommand("createLink", false, href);
    }
    else if (modal.dataset.miniwebLinkType === "URL") {
        const index = modal.dataset.miniwebLinkIndex;
        log('add link to', index);
        const el = contentEditables[index];
        el.innerText = href;
    }
    delete modal.dataset.miniwebLinkIndex;
    delete modal.dataset.miniwebLinkType;
    closeModals();
    modal.querySelector('[name="InternalUrl"]').value = null;
    modal.querySelector('[name="Url"]').value = null;
};
document.addEventListener('click', (e) => {
    const target = e.target;
    log('documentclick', e, target);
    if (!target)
        return;
    if (target.dataset.miniwebDismiss) {
        e.preventDefault();
        closeModals();
    }
    else if (target.dataset.miniwebAddContentTo) {
        e.preventDefault();
        const contentTarget = target.dataset.miniwebAddContentTo;
        const modal = document.querySelector('#miniweb-content-add');
        modal.dataset.miniwebTargetsection = contentTarget;
        modal.classList.add('show');
        modal.scrollIntoView(true);
    }
    else if (target.dataset.miniwebAddContentView) {
        e.preventDefault();
        const contentId = target.dataset.miniwebAddContentView;
        const url = new URL(options.apiEndpoint + 'getitem', document.location.origin);
        url.searchParams.set('viewPath', contentId);
        console.log('getitem', url.toString(), url);
        fetch(url.toString(), { headers: { "RequestVerificationToken": options.afToken } })
            .then(res => res.text())
            .then(data => {
            const targetSection = target.closest('.miniweb-modal').dataset.miniwebTargetsection;
            const el = document.createElement('div');
            el.innerHTML = data;
            const section = document.querySelector('[data-miniwebsection=' + targetSection + ']');
            log(target, contentId, targetSection, section, el);
            section.append(el.firstChild);
            cancelEdit();
            editContent();
            const newEl = section.querySelector('[data-miniwebtemplate]:last-of-type');
            newEl.scrollIntoView(true);
            const firstInput = newEl.querySelector('[contenteditable]');
            if (firstInput) {
                firstInput.focus();
                document.execCommand('selectAll', false, null);
            }
            closeModals();
        });
    }
    else if (target.dataset.miniwebContentMove) {
        const move = target.dataset.miniwebContentMove;
        const item = target.closest('[data-miniwebtemplate]');
        log('move', move, item, target);
        if (move === "up") {
            item.parentNode.insertBefore(item, item.previousElementSibling);
        }
        else if (move === "down") {
            item.parentNode.insertBefore(item, item.nextElementSibling.nextElementSibling);
        }
        else if (move === "delete") {
            confirmDialog('are you sure?', () => {
                item.remove();
            });
        }
        else {
            console.error("unknown move", move, target);
        }
    }
    else if (target.classList.contains('miniweb-asset-pick')) {
        const contentEditables = document.querySelectorAll('[data-miniwebprop]');
        const modal = document.getElementById('miniweb-addAsset');
        const index = modal.dataset.miniwebAssetIndex;
        log('add asset to', index);
        const el = contentEditables[index];
        if (modal.dataset.miniwebAssetType === 'ASSET') {
            el.innerText = target.dataset.miniwebRelpath;
        }
        else if (modal.dataset.miniwebAssetType === 'HTML') {
            document.execCommand('inserthtml', false, `<img src="${target.dataset.miniwebRelpath}"/>`);
        }
        delete modal.dataset.miniwebAssetIndex;
        delete modal.dataset.miniwebAssetType;
        closeModals();
    }
});
const miniwebAdminDefaults = {
    apiEndpoint: '/miniweb-api/',
    afToken: '',
    editTypes: [
        {
            key: 'html',
            editStart: function (element, index) {
                const thisTools = document.getElementById('miniweb-html-tools').cloneNode(true);
                thisTools.removeAttribute("id");
                thisTools.classList.add('miniweb-editor-toolbar');
                element.parentNode.insertBefore(thisTools, element);
                thisTools.querySelectorAll('button').forEach((b) => {
                    b.addEventListener('click', (e) => {
                        const commandWithArgs = b.dataset.miniwebEdit;
                        if (commandWithArgs) {
                            e.preventDefault();
                            e.stopPropagation();
                            const commandArr = commandWithArgs.split(' '), command = commandArr.shift(), args = commandArr.join(' ');
                            document.execCommand(command, false, args);
                        }
                        else if (b.dataset.miniwebCustom) {
                            e.preventDefault();
                            e.stopPropagation();
                            log('do custom task', b);
                            if (b.dataset.miniwebCustom === "createLink") {
                                saveSelection();
                                const modal = document.getElementById('miniweb-addHyperLink');
                                if (selectedRange.commonAncestorContainer.parentNode.tagName === 'A') {
                                    const curHref = selectedRange.commonAncestorContainer.parentNode.getAttribute('href');
                                    if (curHref.indexOf('http') === 0) {
                                        (modal.querySelector('[name="Url"]')).value = curHref;
                                    }
                                    else {
                                        (modal.querySelector('[name="InternalUrl"]')).value = curHref;
                                    }
                                }
                                modal.dataset.miniwebLinkType = 'HTML';
                                modal.classList.add("show");
                            }
                            else if (b.dataset.miniwebCustom === "showSource") {
                                const content = b.closest('.miniweb-editor-toolbar').nextElementSibling;
                                if (b.dataset.miniwebShowSource) {
                                    delete b.dataset.miniwebShowSource;
                                    content.innerHTML = content.firstElementChild.innerText;
                                    content.classList.remove('miniweb-editing-source');
                                }
                                else {
                                    content.classList.add('miniweb-editing-source');
                                    b.dataset.miniwebShowSource = "true";
                                    let html = content.innerHTML;
                                    html = html.replace(/\t/gi, '');
                                    const pre = document.createElement('pre');
                                    pre.innerText = html;
                                    content.innerHTML = pre.outerHTML;
                                }
                            }
                            else if (b.dataset.miniwebCustom === "insertAsset") {
                                const modal = document.getElementById('miniweb-addAsset');
                                const currentAsset = element.innerText;
                                modal.dataset.miniwebAssetType = 'HTML';
                                modal.dataset.miniwebAssetIndex = index;
                                if (currentAsset.lastIndexOf('/') > 0) {
                                    const folder = currentAsset.substr(0, currentAsset.lastIndexOf('/'));
                                    modal.querySelector('.select-asset-folder').value = folder;
                                }
                                modal.classList.add("show");
                                e.stopPropagation();
                                e.preventDefault();
                                showAssetPage(0);
                            }
                        }
                    });
                });
                bindHotKeys(element);
            },
            editEnd: function (element, index) {
                document.querySelectorAll(".miniweb-editor-toolbar").forEach(tb => tb.remove());
                unbindHotKeys(element);
            }
        },
        {
            key: 'asset',
            editStart: function (element, index) {
                element.addEventListener('click', (e) => {
                    log('assetclick', e, element, index);
                    if (e.offsetX > element.offsetWidth) {
                        const modal = document.getElementById('miniweb-addAsset');
                        const currentAsset = element.innerText;
                        modal.dataset.miniwebAssetType = 'ASSET';
                        modal.dataset.miniwebAssetIndex = index;
                        if (currentAsset.lastIndexOf('/') > 0) {
                            const folder = currentAsset.substr(0, currentAsset.lastIndexOf('/'));
                            modal.querySelector('.select-asset-folder').value = folder;
                        }
                        modal.classList.add("show");
                        e.stopPropagation();
                        e.preventDefault();
                        showAssetPage(0);
                    }
                });
            },
            editEnd: function (element, index) {
                element.parentNode.replaceChild(element.cloneNode(true), element);
            }
        },
        {
            key: 'url',
            editStart: function (element, index) {
                element.addEventListener('click', (e) => {
                    log('urlclick', e, element, index);
                    if (e.offsetX > element.offsetWidth) {
                        const modal = document.getElementById('miniweb-addHyperLink');
                        const curHref = element.innerText;
                        if (curHref.indexOf('http') === 0) {
                            modal.querySelector('[name="Url"]').value = curHref;
                        }
                        else {
                            modal.querySelector('[name="InternalUrl"]').value = curHref;
                        }
                        modal.dataset.miniwebLinkType = 'URL';
                        modal.dataset.miniwebLinkIndex = index;
                        modal.classList.add("show");
                        e.stopPropagation();
                        e.preventDefault();
                    }
                });
            },
            editEnd: function (element, index) {
                element.parentNode.replaceChild(element.cloneNode(true), element);
            }
        }
    ]
};
const miniwebAdminInit = function (userOptions) {
    options = extend(miniwebAdminDefaults, userOptions);
    log('initiated miniweb with', userOptions, 'fulloptions', options);
    document.querySelector('body').classList.add('miniweb');
    const btnEdit = document.getElementById("miniweb-button-edit");
    const btnSave = document.getElementById("miniweb-button-save");
    const btnCancel = document.getElementById("miniweb-button-cancel");
    const btnSavePage = document.getElementById("miniweb-button-savepage");
    const btnNewPage = document.getElementById("miniweb-button-newpage");
    const btnPageProperties = document.getElementById("miniweb-button-pageprops");
    const btnDeletePage = document.getElementById("miniweb-button-deletepage");
    const btnAddLink = document.getElementById("miniweb-button-addlink");
    const btnAddAsset = document.getElementById('miniweb-button-addasset');
    const btnAddMultiplePages = document.getElementById('miniweb-button-addmultiplepages');
    const btnDownloadPages = document.getElementById('miniweb-button-downloadpagejson');
    const btnReload = document.getElementById('miniweb-button-reloadcache');
    btnSavePage.addEventListener('click', savePage);
    btnDeletePage.addEventListener('click', removePage);
    btnAddLink.addEventListener('click', addLink);
    btnNewPage.addEventListener('click', addNewPageModal);
    btnEdit.addEventListener('click', editContent);
    btnSave.addEventListener('click', saveContent);
    btnCancel.addEventListener('click', cancelEdit);
    btnPageProperties.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        const modal = document.querySelector('.miniweb-pageproperties');
        const form = modal.querySelector('form');
        form.reset();
        modal.classList.remove("miniweb-modal-right");
        modal.classList.add("show");
    });
    document.getElementById('miniweb-datalist-navigateonenter').addEventListener('keypress', (e) => {
        if (e.code === "Enter") {
            document.location.href = e.target.value;
        }
    });
    document.getElementById('miniweb-datalist-navigateonenter').addEventListener('input', (e) => {
        const input = e.target;
        const listId = input.getAttribute('list');
        const list = document.getElementById(listId);
        for (let i = 0; i < list.options.length; i++) {
            if (input.value === list.options[i].value) {
                document.location.href = input.value;
                return;
            }
        }
    });
    btnAddAsset.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        const button = e.target;
        const form = button.closest('form');
        const fileUpload = button.nextElementSibling;
        fileUpload.onchange = (e) => {
            log('selected asset', e);
            const formData = new FormData(form);
            formData.append('__RequestVerificationToken', options.afToken);
            log('formdata', formData);
            fetch(options.apiEndpoint + "saveassets", {
                method: "POST",
                body: formData
            }).then(res => res.json())
                .then(data => {
                if (data && data.result) {
                    showAssetPage(0);
                    showMessage(true, "The assets were saved successfully");
                }
                else {
                    showMessage(false, "Save assets failed");
                }
            });
        };
        fileUpload.click();
    });
    btnAddMultiplePages.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        const button = e.target;
        const form = button.closest('form');
        const fileUpload = button.nextElementSibling;
        fileUpload.onchange = (e) => {
            log('selected asset', e);
            const formData = new FormData(form);
            formData.append('__RequestVerificationToken', options.afToken);
            log('formdata', formData);
            fetch(options.apiEndpoint + "multiplepages", {
                method: "POST",
                body: formData
            })
                .then(res => res.json())
                .then(data => {
                if (data && data.result) {
                    showMessage(true, "Pages saved successfully");
                }
                else {
                    showMessage(false, "Save pages failed");
                }
            });
        };
        fileUpload.click();
    });
    btnDownloadPages.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        const button = e.target;
        const form = button.closest('form');
        const formData = new FormData(form);
        formData.append('__RequestVerificationToken', options.afToken);
        fetch(options.apiEndpoint + "downloadpages", {
            method: "POST",
            body: formData
        })
            .then(res => res.text())
            .then(content => {
            const a = document.createElement('a');
            const mimeType = 'application/json';
            const fileName = 'page.json';
            if (navigator.msSaveBlob) {
                navigator.msSaveBlob(new Blob([content], {
                    type: mimeType
                }), fileName);
            }
            else if (URL && 'download' in a) {
                a.href = URL.createObjectURL(new Blob([content], {
                    type: mimeType
                }));
                a.setAttribute('download', fileName);
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
            }
            else {
                location.href = 'data:application/octet-stream,' + encodeURIComponent(content);
            }
        });
    });
    btnReload.addEventListener('click', (e) => {
        e.preventDefault();
        if (localStorage.getItem('showLog') === 'true') {
            console.log('turned logging off');
            localStorage.removeItem('showLog');
        }
        else {
            console.log('turned logging on');
            localStorage.setItem('showLog', 'true');
        }
        const data = new FormData();
        data.append('__RequestVerificationToken', options.afToken);
        fetch(options.apiEndpoint + "reloadcaches", {
            method: "POST",
            body: data
        }).then(res => res.json())
            .then(data => {
            log(data);
            if (data === null || data === void 0 ? void 0 : data.result)
                showMessage(true, "Caches were cleared");
            else
                showMessage(false, "Failed to clear cache");
            cancelEdit();
        });
    });
    window.addEventListener('keydown', ctrlSsave, true);
    document.querySelector('[name="miniwebAssetFolder"]').addEventListener('input', () => {
        showAssetPage(0);
    });
    document.querySelectorAll('.miniweb-asset-pager').forEach((elem) => {
        elem.addEventListener('click', (e) => {
            const direction = Number(e.target.dataset.miniwebPageMove);
            let curPage = Number(assetPageList.dataset.miniwebPage);
            curPage += direction;
            if (curPage < 0)
                curPage = 0;
            showAssetPage(curPage);
        });
    });
    document.querySelector('#miniweb-li-showhiddenpages input').addEventListener('click', (e) => {
        const input = e.target;
        sessionStorage.setItem('miniweb-li-showhiddenpages', input.checked ? "true" : "false");
        toggleHiddenMenuItems(input.checked);
    });
    if (sessionStorage.getItem('miniweb-li-showhiddenpages') === "true") {
        document.querySelector('#miniweb-li-showhiddenpages input').checked = true;
        toggleHiddenMenuItems(true);
    }
    else {
        toggleHiddenMenuItems(false);
    }
    const message = localStorage.getItem('miniweb-reloadedmessage');
    if (message) {
        if (message.startsWith('!')) {
            showMessage(false, message.substring(1));
        }
        else {
            showMessage(true, message);
        }
        localStorage.removeItem('miniweb-reloadedmessage');
    }
    cancelEdit();
};
export { miniwebAdminInit };

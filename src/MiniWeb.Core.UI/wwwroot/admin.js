(function () {
    var readFileIntoDataUrl = function (fileInfo) {
        return new Promise((resolve, reject) => {
            const fReader = new FileReader();
            fReader.onload = function (e) {
                resolve(e.target.result);
            };
            fReader.onerror = reject;
            fReader.readAsDataURL(fileInfo);
        });
    };
    if (!document.querySelector("miniwebadmin")) {
        return;
    }
    var extend = function (defaults, options) {
        var extended = {};
        var prop;
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
    window.miniwebAdmin = function (userOptions) {
        var adminTag = this;
        var options = extend(miniwebAdminDefaults, userOptions);
        let contentEditables;
        let btnNew;
        let btnSavePage;
        let btnEdit;
        let btnSave;
        let btnCancel;
        const editContent = function () {
            document.querySelector('body').classList.add('miniweb-editing');
            contentEditables = document.querySelectorAll('[data-miniwebprop]');
            contentEditables.forEach(el => el.setAttribute('contentEditable', "true"));
            for (var i = 0; i < options.editTypes.length; i++) {
                var editType = options.editTypes[i];
                contentEditables.forEach((ce, ix) => {
                    if (ce.dataset.miniwebedittype == editType.key) {
                        editType.editStart(ce, ix);
                    }
                });
            }
            btnNew.setAttribute("disabled", "true");
            btnEdit.setAttribute("disabled", "true");
            btnSave.removeAttribute("disabled");
            btnCancel.removeAttribute("disabled");
            toggleContentInserts(true);
        };
        const cancelEdit = function () {
            document.querySelector('body').classList.remove('miniweb-editing');
            contentEditables.forEach(el => el.removeAttribute('contentEditable'));
            for (var i = 0; i < options.editTypes.length; i++) {
                var editType = options.editTypes[i];
                contentEditables.forEach((ce, ix) => {
                    if (ce.dataset.miniwebedittype == editType.key) {
                        editType.editEnd(ce, ix);
                        return;
                    }
                });
            }
            btnNew.removeAttribute("disabled");
            btnEdit.removeAttribute("disabled");
            btnSave.setAttribute("disabled", "true");
            btnCancel.setAttribute("disabled", "true");
            toggleContentInserts(false);
        };
        const toggleContentInserts = function (on) {
            if (on) {
                document.querySelectorAll('[data-miniwebsection]').forEach(el => {
                    const section = el.dataset.miniwebsection;
                    el.insertAdjacentHTML('beforeend', '<button class="miniweb-insertcontent" data-add-content-to="' + section + '">add content</button>');
                });
                document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate] .miniweb-template-actions').forEach(el => el.remove());
                document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate]').forEach(el => el.insertAdjacentHTML('beforeend', '<div class="pull-right miniweb-template-actions"><button data-content-move="up">&#11014;</button><button data-content-move="down" >&#11015;</button>	<button class="danger" data-content-move="delete">&#11199;</button></div>'));
            }
            else {
                document.querySelectorAll('.miniweb-insertcontent, .miniweb-template-actions').forEach(el => el.remove());
            }
        };
        const toggleSourceView = function () {
        };
        const getParsedHtml = function (source) {
            var parsedDOM;
            parsedDOM = new DOMParser().parseFromString(source.innerHTML, 'text/html');
            parsedDOM = new XMLSerializer().serializeToString(parsedDOM);
            /<body>([\s\S]*)<\/body>/im.exec(parsedDOM);
            parsedDOM = RegExp.$1;
            return parsedDOM;
        };
        const saveContent = function (e) {
            if (!document.querySelector('body').classList.contains('miniweb-editing'))
                return;
            var items = [];
            document.querySelectorAll('[data-miniwebsection]').forEach((section, index) => {
                var sectionid = section.dataset.miniwebsection;
                section.querySelectorAll('[data-miniwebtemplate]').forEach((tmpl, tindex) => {
                    console.log('item', tmpl);
                    if (items[index] == null) {
                        items[index] = {};
                        items[index].Key = sectionid;
                        items[index].Items = [];
                    }
                    var item = {
                        Template: tmpl.dataset.miniwebtemplate,
                        Values: {}
                    };
                    tmpl.querySelectorAll('[data-miniwebprop]').forEach((prop, pindex) => {
                        var key = prop.getAttribute('data-miniwebprop');
                        var value = getParsedHtml(prop);
                        item.Values[key] = value;
                        if (key.indexOf(':') > 0) {
                            var orig = key.split(':')[0];
                            var attrib = key.split(':')[1];
                            tmpl.querySelector('[data-miniwebprop="' + orig + '"]').setAttribute(attrib, value);
                        }
                    });
                    items[index].Items.push(item);
                });
            });
            const data = new FormData();
            data.append('url', document.getElementById("miniweb-admin-nav").dataset.miniwebPath);
            data.append('items', JSON.stringify(items));
            data.append('__RequestVerificationToken', getVerificationToken());
            fetch(options.apiEndpoint + "savecontent", {
                method: "POST",
                body: data
            }).then(res => res.json())
                .then(data => {
                console.log(data);
                if (data.result)
                    showMessage(true, "The page was saved successfully");
                else
                    showMessage(false, "Save page failed");
                cancelEdit();
            });
        };
        const getVerificationToken = function () {
            return document.querySelector('#miniweb-templates input[name=__RequestVerificationToken]').value;
        };
        const savePage = function () {
            const form = document.querySelector('#miniweb-pageProperties form');
            let formData = new FormData(form);
            formData.append('__RequestVerificationToken', getVerificationToken());
            fetch(options.apiEndpoint + "savepage", {
                method: "POST",
                body: formData
            }).then(res => res.json())
                .then(data => {
                if (data.result) {
                    showMessage(true, "saved page successfully");
                    document.querySelector('.mw-modal.show').classList.remove('show');
                }
                else {
                    showMessage(false, data.message);
                }
            }).catch(res => {
                showMessage(false, 'failed to post');
            });
        };
        const removePage = function () {
            if (confirm('are you sure?')) {
            }
        };
        const addNewPage = function () {
        };
        const ctrl_s_save = function (event) {
            if (document.querySelector('body').classList.contains('miniweb-editing')) {
                if (event.ctrlKey && event.keyCode == 83) {
                    event.preventDefault();
                    saveContent(event);
                }
                ;
            }
        };
        const addLink = function () {
            const modal = document.getElementById('miniweb-addHyperLink');
            let href = modal.querySelector('[name="InternalUrl"]').value;
            if (!href)
                href = modal.querySelector('[name="Url"]').value;
            if (modal.dataset.linkType == 'HTML') {
                restoreSelection();
                document.execCommand("unlink", false, null);
                document.execCommand("createLink", false, href);
            }
            else if (modal.dataset.linkType == "URL") {
                const index = modal.dataset.linkIndex;
                console.log('add link to', index);
                const el = contentEditables[index];
                el.innerText = href;
            }
            modal.classList.remove('show');
            delete modal.dataset.linkIndex;
            delete modal.dataset.linkType;
            modal.querySelector('[name="InternalUrl"]').value = null;
            modal.querySelector('[name="Url"]').value = null;
        };
        btnNew = document.getElementById("miniwebButtonNew");
        btnSavePage = document.getElementById("miniwebSavePage");
        btnEdit = document.getElementById("miniwebButtonEdit");
        btnSave = document.getElementById("miniwebButtonSave");
        btnCancel = document.getElementById("miniwebButtonCancel");
        contentEditables = document.querySelectorAll('[data-miniwebprop]');
        btnSavePage.addEventListener('click', savePage);
        btnEdit.addEventListener('click', editContent);
        btnSave.addEventListener('click', saveContent);
        btnCancel.addEventListener('click', cancelEdit);
        const btnAddLink = document.getElementById("miniwebAddLink");
        btnAddLink.addEventListener('click', addLink);
        document.getElementById('miniwebNavigateOnEnter').addEventListener('keypress', (e) => {
            if (e.code == "Enter") {
                document.location.href = e.target.value;
            }
        });
        document.getElementById('miniwebNavigateOnEnter').addEventListener('input', (e) => {
            const input = e.target;
            const listId = input.getAttribute('list');
            const list = document.getElementById(listId);
            for (let i = 0; i < list.options.length; i++) {
                if (input.value == list.options[i].value) {
                    document.location.href = input.value;
                    return;
                }
            }
        });
        document.addEventListener('click', (e) => {
            const target = e.target;
            console.log('documentclick', e, target);
            if (!target)
                return;
            if (target.dataset.showModal) {
                e.preventDefault();
                const modal = document.querySelector(target.dataset.showModal);
                if (modal) {
                    modal.classList.contains('show') ? modal.classList.remove('show') : modal.classList.add('show');
                }
            }
            else if (target.dataset.dismiss) {
                e.preventDefault();
                document.querySelectorAll('.mw-modal.show').forEach(el => {
                    el.classList.remove('show');
                });
            }
            else if (target.dataset.addContentTo) {
                e.preventDefault();
                const contentTarget = target.dataset.addContentTo;
                const modal = document.querySelector('#newContentAdd');
                modal.dataset.targetsection = contentTarget;
                modal.classList.add('show');
            }
            else if (target.dataset.addContentId) {
                e.preventDefault();
                const contentId = target.dataset.addContentId;
                const targetSection = target.closest('.mw-modal').dataset.targetsection;
                const el = (document.getElementById(contentId).firstElementChild.cloneNode(true));
                const section = document.querySelector('[data-miniwebsection=' + targetSection + ']');
                console.log(target, contentId, targetSection, section, el);
                section.append(el);
                cancelEdit();
                editContent();
                document.querySelectorAll('.mw-modal.show').forEach(el => {
                    el.classList.remove('show');
                });
            }
            else if (target.dataset.contentMove) {
                const move = target.dataset.contentMove;
                const item = target.closest('[data-miniwebtemplate]');
                console.log('move', move, item, target);
                if (move == "up") {
                    item.parentNode.insertBefore(item, item.previousElementSibling);
                }
                else if (move == "down") {
                    item.parentNode.insertBefore(item, item.nextElementSibling.nextElementSibling);
                }
                else if (move == "delete") {
                    if (confirm('are you sure?')) {
                        item.remove();
                    }
                }
                else {
                    console.error("unknown move", move, target);
                }
            }
        });
        window.addEventListener('keydown', ctrl_s_save, true);
        cancelEdit();
        return this;
    };
    let selectedRange;
    const getCurrentRange = function () {
        var sel = window.getSelection();
        if (sel.getRangeAt && sel.rangeCount) {
            return sel.getRangeAt(0);
        }
    };
    const saveSelection = function () {
        selectedRange = getCurrentRange();
    };
    const restoreSelection = function () {
        var selection = window.getSelection();
        if (selectedRange) {
            try {
                selection.removeAllRanges();
            }
            catch (ex) {
                document.body.createTextRange().select();
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
    var txtMessage = document.querySelector("miniwebadmin .alert");
    var showMessage = function (success, message, isHtml = false) {
        var className = success ? "alert-success" : "alert-danger";
        var timeout = success ? 4000 : 8000;
        txtMessage.classList.add(className);
        if (isHtml)
            txtMessage.innerHTML = message;
        else
            txtMessage.innerHTML = htmlEscape(message);
        txtMessage.parentElement.classList.remove("is-hidden");
        setTimeout(function () {
            txtMessage.classList.remove(className);
            txtMessage.parentElement.classList.add("is-hidden");
        }, timeout);
    };
    document.querySelector('[name="miniweb-asset-folder"]').addEventListener('input', (e) => {
        const input = e.target;
        const listId = input.getAttribute('list');
        const list = document.getElementById(listId);
        for (let i = 0; i < list.options.length; i++) {
            if (input.value == list.options[i].value) {
                showAssetPage(0);
                return;
            }
        }
    });
    const assetPageList = document.querySelector('.miniweb-assetlist');
    const showAssetPage = function (page) {
        assetPageList.dataset.page = page.toString();
        const assets = assetPageList.querySelectorAll('li');
        assets.forEach((li) => {
            li.classList.add('is-hidden');
            const img = li.querySelector('img');
            if (img)
                delete img.src;
        });
        const folder = document.querySelector('[name="miniweb-asset-folder"]').value;
        const curEls = assetPageList.querySelectorAll('li[data-path="' + folder + '"]');
        for (let i = curEls.length; i > 0; i--) {
            assetPageList.insertBefore(curEls[i - 1], assetPageList.childNodes[0]);
        }
        const newPage = (page * 16) + 1;
        const toShow = document.querySelectorAll('.miniweb-assetlist li[data-path="' + folder + '"]:nth-child(n+' + newPage + '):nth-child(-n+' + (newPage + 15) + ')');
        toShow.forEach((li) => {
            const img = li.querySelector('img');
            if (img && img.dataset.src) {
                img.src = img.dataset.src;
            }
            li.classList.remove('is-hidden');
        });
        checkAssetPagerVisibility(page, folder);
    };
    const checkAssetPagerVisibility = function (page, folder) {
        const all = assetPageList.querySelectorAll('li[data-path="' + folder + '"]');
        const last = all[all.length - 1];
        if (last.classList.contains('is-hidden')) {
            document.getElementById('miniweb-asset-page-right').classList.remove('is-hidden');
        }
        else {
            document.getElementById('miniweb-asset-page-right').classList.add('is-hidden');
        }
        if (page > 0) {
            document.getElementById('miniweb-asset-page-left').classList.remove('is-hidden');
        }
        else {
            document.getElementById('miniweb-asset-page-left').classList.add('is-hidden');
        }
    };
    document.querySelectorAll('.miniweb-asset-pager').forEach((elem, ix) => {
        elem.addEventListener('click', (e) => {
            const direction = Number(e.target.dataset.pageMove);
            let curPage = Number(assetPageList.dataset.page);
            curPage += direction;
            if (curPage < 0)
                curPage = 0;
            showAssetPage(curPage);
        });
    });
    const miniwebAdminDefaults = {
        apiEndpoint: '/miniweb-api/',
        editTypes: [
            {
                key: 'html',
                editStart: function (element, index) {
                    var thisTools = (document.getElementById('tools').cloneNode(true));
                    thisTools.removeAttribute("id");
                    thisTools.classList.add('miniweb-editor-toolbar');
                    element.parentNode.insertBefore(thisTools, element);
                    thisTools.querySelectorAll('button').forEach((b, i) => {
                        b.addEventListener('click', (e) => {
                            const commandWithArgs = b.dataset.edit;
                            if (commandWithArgs) {
                                e.preventDefault();
                                e.stopPropagation();
                                const commandArr = commandWithArgs.split(' '), command = commandArr.shift(), args = commandArr.join(' ');
                                document.execCommand(command, false, args);
                            }
                            else if (b.dataset.custom) {
                                e.preventDefault();
                                e.stopPropagation();
                                console.log('do custom task', b);
                                if (b.dataset.custom == "createLink") {
                                    saveSelection();
                                    const modal = document.getElementById('miniweb-addHyperLink');
                                    if (selectedRange.commonAncestorContainer.parentNode.tagName == 'A') {
                                        const curHref = selectedRange.commonAncestorContainer.parentNode.getAttribute('href');
                                        if (curHref.indexOf('http') == 0) {
                                            modal.querySelector('[name="Url"]').value = curHref;
                                        }
                                        else {
                                            modal.querySelector('[name="InternalUrl"]').value = curHref;
                                        }
                                    }
                                    modal.dataset.linkType = 'HTML';
                                    modal.classList.add("show");
                                }
                                else if (b.dataset.custom == "showSource") {
                                    const content = b.closest('.miniweb-editor-toolbar').nextElementSibling;
                                    if (b.dataset.showSource) {
                                        delete b.dataset.showSource;
                                        content.innerHTML = content.firstElementChild.innerText;
                                    }
                                    else {
                                        b.dataset.showSource = "true";
                                        let html = content.innerHTML;
                                        html = html.replace(/\t/gi, '');
                                        const pre = document.createElement('pre');
                                        pre.innerText = html;
                                        content.innerHTML = pre.outerHTML;
                                    }
                                }
                            }
                        });
                    });
                },
                editEnd: function (index) {
                    document.querySelectorAll(".miniweb-editor-toolbar").forEach(tb => tb.remove());
                }
            },
            {
                key: 'asset',
                editStart: function (element, index) {
                    element.addEventListener('click', (e) => {
                        console.log('assetclick', e, element, index);
                        if (e.offsetX > element.offsetWidth) {
                            const modal = document.getElementById('miniweb-addAsset');
                            const currentAsset = element.innerText;
                            modal.dataset.assetType = 'ASSET';
                            modal.dataset.assetIndex = index;
                            modal.dataset.currentAsset = currentAsset;
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
                        console.log('urlclick', e, element, index);
                        if (e.offsetX > element.offsetWidth) {
                            const modal = document.getElementById('miniweb-addHyperLink');
                            const curHref = element.innerText;
                            if (curHref.indexOf('http') == 0) {
                                modal.querySelector('[name="Url"]').value = curHref;
                            }
                            else {
                                modal.querySelector('[name="InternalUrl"]').value = curHref;
                            }
                            modal.dataset.linkType = 'URL';
                            modal.dataset.linkIndex = index;
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
    const toggleHiddenMenuItems = function (on) {
        const items = document.querySelectorAll('.miniweb-hidden-menu');
        items.forEach((item, ix) => {
            if (on) {
                item.classList.add('show');
            }
            else {
                item.classList.remove('show');
            }
        });
    };
    document.querySelector('#miniwebShowHiddenPages input').addEventListener('click', (e) => {
        sessionStorage.setItem('miniwebShowHiddenPages', (e.target).checked ? "true" : "false");
        toggleHiddenMenuItems((e.target).checked);
    });
    if (sessionStorage.getItem('miniwebShowHiddenPages') === "true") {
        document.querySelector('#miniwebShowHiddenPages input').checked = true;
        toggleHiddenMenuItems(true);
    }
    else {
        toggleHiddenMenuItems(false);
    }
})();

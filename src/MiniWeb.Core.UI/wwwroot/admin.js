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
            toggleSourceView();
        };
        const cancelEdit = function () {
            document.querySelector('body').classList.remove('miniweb-editing');
            contentEditables.forEach(el => el.removeAttribute('contentEditable'));
            for (var i = 0; i < options.editTypes.length; i++) {
                var editType = options.editTypes[i];
                contentEditables.forEach((ce, ix) => {
                    if (ce.dataset.miniwebedittype == editType.key) {
                        editType.editEnd();
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
            const form = document.querySelector('#pageProperties form');
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
            }
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
        window.addEventListener('keydown', ctrl_s_save, true);
        cancelEdit();
        return this;
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
                            console.log("clicked button", b, e);
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
                editStart: function (index) {
                },
                editEnd: function (index) {
                }
            },
            {
                key: 'url',
                editStart: function (index) {
                },
                editEnd: function (index) {
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

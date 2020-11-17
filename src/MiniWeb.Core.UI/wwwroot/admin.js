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
            btnNew.removeAttribute("disabled");
            btnEdit.removeAttribute("disabled");
            btnSave.setAttribute("disabled", "true");
            btnCancel.setAttribute("disabled", "true");
            toggleContentInserts(false);
        };
        const toggleContentInserts = function (on) {
            if (on) {
                document.querySelectorAll('[data-miniwebsection]').forEach(el => el.insertAdjacentHTML('beforeend', '<a href="#" class="miniweb-insertcontent btn btn-info" data-toggle="modal" data-target="#newContentAdd">add content</a>'));
                document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate] .miniweb-template-actions').forEach(el => el.remove());
                document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate]').forEach(el => el.insertAdjacentHTML('beforeend', '<div class="btn-group pull-right miniweb-template-actions"><a class="btn btn-mini articleUp" ><i class="glyphicon glyphicon-arrow-up" > </i> </a><a class="btn btn-mini articleDown" > <i class="glyphicon glyphicon-arrow-down" > </i> </a>	<a class="btn btn-mini remove" title= "Remove article" > <i class="glyphicon glyphicon-remove" > </i> remove article</a>	</div>'));
            }
            else {
                document.querySelectorAll('.miniweb-insertcontent, .miniweb-template-actions').forEach(el => el.remove());
            }
        };
        const toggleSourceView = function () {
        };
        const getParsedHtml = function (source) {
            var parsedDOM;
            parsedDOM = new DOMParser().parseFromString(source.cleanHtml(), 'text/html');
            parsedDOM = new XMLSerializer().serializeToString(parsedDOM);
            /<body>([\s\S]*)<\/body>/im.exec(parsedDOM);
            parsedDOM = RegExp.$1;
            return $.trim(parsedDOM);
        };
        const saveContent = function (e) {
            if (!document.querySelector('body').classList.contains('miniweb-editing'))
                return;
            var items = [];
            document.querySelectorAll('[data-miniwebsection]').forEach((section, index) => {
                var sectionid = section.getAttribute('data-miniwebsection');
                this.querySelectorAll('[data-miniwebtemplate]').forEach((tmpl, tindex) => {
                    if (items[index] == null) {
                        items[index] = {};
                        items[index].Key = sectionid;
                        items[index].Items = [];
                    }
                    var item = {
                        Template: tmpl.getAttribute('data-miniwebtemplate'),
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
            console.log(JSON.stringify(items));
            showMessage(true, "TEST SUCCESS");
        };
        const savePage = function () {
            const form = document.querySelector('#pageProperties form');
            const items = form.querySelectorAll('select,input,textarea');
            let formArr = new FormData();
            items.forEach((el, ix) => {
                console.log(el.getAttribute('name'), el.value);
                formArr.append(el.getAttribute('name'), el.value);
            });
            formArr.append('__RequestVerificationToken', document.querySelector('#miniweb-templates input[name=__RequestVerificationToken]').value);
            console.log('savePage', options.apiEndpoint, formArr);
            fetch(options.apiEndpoint + "savepage", {
                method: "POST",
                body: formArr
            }).then(res => res.json())
                .then(data => {
                if (data.result) {
                    showMessage(true, "saved page successfully");
                    document.querySelector('.modal.show').classList.remove('show');
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
        const addNewContent = function () {
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
        const modalTriggers = document.querySelectorAll('[data-show-modal]');
        modalTriggers.forEach((t, i) => {
            t.addEventListener('click', (e) => {
                if (e.target instanceof Element) {
                    const modalTargetSelector = e.target.dataset.showModal;
                    const modalTarget = document.querySelector(modalTargetSelector);
                    if (modalTarget) {
                        modalTarget.classList.contains('show') ? modalTarget.classList.remove('show') : modalTarget.classList.add('show');
                    }
                }
            });
        });
        const modalDismiss = document.querySelectorAll('[data-dismiss]');
        modalDismiss.forEach((t, i) => {
            t.addEventListener('click', (e) => {
                if (e.target instanceof Element) {
                    const modalTargetSelector = e.target.dataset.dismiss;
                    const modalTarget = document.querySelector(modalTargetSelector);
                    if (modalTarget) {
                        modalTarget.classList.remove('show');
                    }
                }
            });
        });
        btnNew = document.getElementById("btnNew");
        btnSavePage = document.getElementById("miniwebSavePage");
        btnEdit = document.getElementById("btnEdit");
        btnSave = document.getElementById("btnSave");
        btnCancel = document.getElementById("btnCancel");
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
                editStart: function (index) {
                    var thisTools = (document.getElementById('tools').cloneNode(true));
                    thisTools.setAttribute("id", "");
                    thisTools.setAttribute("data-role", "editor-toolbar" + index);
                    thisTools.classList.add("editor-toolbar");
                    this.before(thisTools);
                },
                editEnd: function (index) {
                    document.querySelectorAll(".editor-toolbar").forEach(tb => tb.remove());
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
    document.querySelector('#showHiddenPages input').addEventListener('click', (e) => {
        sessionStorage.setItem('showhiddenpages', (e.target).checked ? "true" : "false");
    });
})();

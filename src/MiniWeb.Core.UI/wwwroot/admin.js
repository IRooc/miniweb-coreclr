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
            $.post(options.apiEndpoint + 'savecontent', {
                url: $('#admin').attr('data-miniweb-path'),
                items: JSON.stringify(items),
                '__RequestVerificationToken': $('#miniweb-templates input[name=__RequestVerificationToken]').val()
            }).done(function (data) {
                if (data.result) {
                    showMessage(true, "The page was saved successfully");
                }
                else {
                    showMessage(false, "Save page failed");
                }
                cancelEdit();
            }).fail(function (data) {
                var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
                showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
            });
        }, savePage = function () {
            var formArr = $(this).closest('.modal-content').find('form').serializeArray();
            formArr.push({ name: '__RequestVerificationToken', value: $('#miniweb-templates input[name=__RequestVerificationToken]').val() });
            $.post(options.apiEndpoint + "savepage", formArr).done(function (data) {
                if (data && data.result) {
                    document.location.href = data.url;
                }
                else {
                    showMessage(false, data.message);
                }
            }).fail(function (data) {
                var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
                showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
            });
        }, removePage = function () {
            if (confirm('are you sure?')) {
                $.post(options.apiEndpoint + "removepage", {
                    '__RequestVerificationToken': $('#miniweb-templates input[name=__RequestVerificationToken]').val(),
                    url: $('#admin').attr('data-miniweb-path')
                }).done(function (data) {
                    showMessage(true, "The page was saved successfully");
                    setTimeout(function () {
                        document.location.href = data.url;
                    }, 1500);
                }).fail(function (data) {
                    var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
                    showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
                });
            }
        }, addNewContent = function () {
            var htmlid = $(this).data('contentid');
            var target = $(this).closest('#newContentAdd').attr('data-targetsection');
            $('[data-miniwebsection=' + target + ']').append($('#' + htmlid).html());
            cancelEdit();
            editContent();
            $('#newContentAdd').modal('hide');
        }, addNewPage = function () {
            if ($('#newPageProperties').length == 0) {
                var newP = $('#pageProperties').clone();
                newP.attr('id', 'newPageProperties');
                $('input[name=OldUrl]', newP).remove();
                $('input,textarea', newP).not('[type=hidden],[type=checkbox]').val('');
                var parentUrl = $('#pageProperties input[name=Url]').val();
                $('input[name=Url]', newP).val(parentUrl.substring(0, parentUrl.lastIndexOf('/') + 1));
                adminTag.append(newP);
                $('#newPageProperties .btn-primary').data('newpage', true).bind('click', savePage);
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
        btnNew = document.getElementById("btnNew");
        btnEdit = document.getElementById("btnEdit");
        btnSave = document.getElementById("btnSave");
        btnCancel = document.getElementById("btnCancel");
        contentEditables = document.querySelectorAll('[data-miniwebprop]');
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
    var txtMessage = document.querySelector("#admin .alert");
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

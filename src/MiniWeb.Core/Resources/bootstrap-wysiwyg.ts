/* http://github.com/mindmup/bootstrap-wysiwyg */
/*global jQuery, $, FileReader*/
/*jslint browser:true*/
/*

RC
- Extended userOptions to enable createLink override
- Changed the insertimage to inserthtml so you can set the imagename as attribute
- updated with closures so can be used in extended function
*/
/// <reference path="jquery.d.ts" />
/// <reference path="bootstrap.d.ts" />

(function ($) {
	'use strict';
	var readFileIntoDataUrl = function (fileInfo) {
		var loader = $.Deferred(),
		 fReader = new FileReader();
		fReader.onload = function (e) {
			loader.resolve((<any>e.target).result);
		};
		fReader.onerror = loader.reject;
		fReader.onprogress = loader.notify;
		fReader.readAsDataURL(fileInfo);
		return loader.promise();
	};
	$.fn.cleanHtml = function () {
		var html = $(this).html();
		return html && html.replace(/(<br>|\s|<div><br><\/div>|&nbsp;)*$/, '');
	};
	$.fn.wysiwyg = function (userOptions) {
		var editor = this;
		var editorWysiwyg = {
			me: editor,
			selectedRange: null,
			options: <any>{},
			toolbar: null,
			toolbarBtnSelector: '',
			updateToolbar: function () {
				if (this.options.activeToolbarClass) {
					var opts = this.options;
					$(opts.toolbarSelector).find(this.toolbarBtnSelector).each(function () {
						var command = <any>$(this).data(opts.commandRole);
						try {
							if (document.queryCommandState(command)) {
								$(this).addClass(opts.activeToolbarClass);
							} else {
								$(this).removeClass(opts.activeToolbarClass);
							}
						}
						catch (ex) { };
					});
				}
			},
			execCommand: function (commandWithArgs, valueArg) {
				var commandArr = commandWithArgs.split(' '),
				command = commandArr.shift(),
				args = commandArr.join(' ') + (valueArg || '');
				if (commandWithArgs === "createLink") {
					this.options.createLink(this);
				} else if (commandWithArgs === "insertAsset") {
					this.options.insertAsset(this);
				} else {
					document.execCommand(command, false, args);
					this.updateToolbar();
				}
			},
			bindHotkeys: function (hotKeys) {
				$.each(hotKeys, function (hotkey, command) {
					var meEdit = editor;
					meEdit.keydown(hotkey, function (e) {
						if (meEdit.attr('contenteditable') && meEdit.is(':visible')) {
							e.preventDefault();
							e.stopPropagation();
							this.execCommand(command);
						}
					}).keyup(hotkey, function (e) {
						if (meEdit.attr('contenteditable') && meEdit.is(':visible')) {
							e.preventDefault();
							e.stopPropagation();
						}
					});
				});
			},
			getCurrentRange: function () {
				var sel = window.getSelection();
				if (sel.getRangeAt && sel.rangeCount) {
					return sel.getRangeAt(0);
				}
			},
			saveSelection: function () {
				this.selectedRange = this.getCurrentRange();
			},
			restoreSelection: function () {
				var selection = window.getSelection();
				if (this.selectedRange) {
					try {
						selection.removeAllRanges();
					} catch (ex) {
						(<any>document.body).createTextRange().select();
						(<any>document).selection.empty();
					}

					selection.addRange(this.selectedRange);
				}
			},
			insertFiles: function (files) {
				var meObj = this;
				meObj.me.focus();
				$.each(files, function (idx, fileInfo) {
					if (/^image\//.test(fileInfo.type)) {
						$.when(meObj.options.readFileIntoUrl(fileInfo)).done(function (dataUrl) {
							var imageHtml = '<img src="' + dataUrl + '" data-filename="' + fileInfo.name + '"/>';
							meObj.execCommand('inserthtml', imageHtml);
						}).fail(function (e) {
							meObj.options.fileUploadError("file-reader", e);
						});
					} else {
						$.when(meObj.options.readFileIntoUrl(fileInfo)).done(function (dataUrl) {
							//execCommand('inserthtml', '<a href="' + dataUrl + '">Download</a>');
							var frag = document.createDocumentFragment();
							var node = document.createElement("a");
							node.innerText = fileInfo.name;
							node.href = dataUrl;
							var attrib = document.createAttribute("data-filename");
							attrib.value = fileInfo.name;
							node.setAttributeNode(attrib);

							frag.appendChild(node);
							window.getSelection().getRangeAt(0).insertNode(node);
						}).fail(function (e) {
							meObj.options.fileUploadError("file-reader", e);
						});
						//this.options.fileUploadError("unsupported-file-type", fileInfo.type);
					}
				});
			},
			markSelection: function (input, color) {
				this.restoreSelection();
				if (document.queryCommandSupported('hiliteColor')) {
					document.execCommand('hiliteColor', false, color || 'transparent');
				}
				this.saveSelection();
				input.data(this.options.selectionMarker, color);
			},
			bindToolbar: function (toolbar, options) {
				var meEdit = this;
				meEdit.toolbar = toolbar;

				toolbar.find(meEdit.toolbarBtnSelector).click(function () {
					meEdit.restoreSelection();
					meEdit.me.focus();
					meEdit.execCommand($(this).data(options.commandRole));
					meEdit.saveSelection();
				});
				toolbar.find('[data-toggle=dropdown]').click(this.restoreSelection);

				toolbar.find('input[type=text][data-' + options.commandRole + ']').on('webkitspeechchange change', function () {
					var newValue = this.value; /* ugly but prevents fake double-calls due to selection restoration */
					this.value = '';
					meEdit.restoreSelection();
					if (newValue) {
						this.me.focus();
						meEdit.execCommand($(this).data(options.commandRole), newValue);
					}
					meEdit.saveSelection();
				}).on('focus', function () {
					var input = $(this);
					if (!input.data(options.selectionMarker)) {
						meEdit.markSelection(input, options.selectionColor);
						input.focus();
					}
				}).on('blur', function () {
					var input = $(this);
					if (input.data(options.selectionMarker)) {
						meEdit.markSelection(input, false);
					}
				});
				toolbar.find('input[type=file][data-' + options.commandRole + ']').change(function () {
					meEdit.restoreSelection();
					if (this.type === 'file' && this.files && this.files.length > 0) {
						meEdit.insertFiles(this.files);
					}
					meEdit.saveSelection();
					this.value = '';
				});
			},
			initFileDrops: function () {
				var meEdit = this;
				this.me.on('dragenter dragover', false)
				  .on('drop', function (e) {
				  	var dataTransfer = e.originalEvent.dataTransfer;
				  	e.stopPropagation();
				  	e.preventDefault();
				  	if (dataTransfer && dataTransfer.files && dataTransfer.files.length > 0) {
				  		meEdit.insertFiles(dataTransfer.files);
				  	}
				  });
			}
		};
		editorWysiwyg.options = $.extend({}, $.fn.wysiwyg.defaults, userOptions);
		editorWysiwyg.toolbarBtnSelector = 'a[data-' + editorWysiwyg.options.commandRole + '],button[data-' + editorWysiwyg.options.commandRole + '],input[type=button][data-' + editorWysiwyg.options.commandRole + ']';
		editorWysiwyg.bindHotkeys(editorWysiwyg.options.hotKeys);
		if (editorWysiwyg.options.dragAndDropImages) {
			editorWysiwyg.initFileDrops();
		}
		editorWysiwyg.bindToolbar($(editorWysiwyg.options.toolbarSelector), editorWysiwyg.options);
		editor.attr('contenteditable', true)
		 .on('mouseup keyup mouseout', function () {
		 	editorWysiwyg.saveSelection();
		 	editorWysiwyg.updateToolbar();
		 });
		$(window).bind('touchend', function (e) {
			var isInside = (editor.is(e.target) || editor.has(e.target).length > 0),
			currentRange = editorWysiwyg.getCurrentRange(),
			clear = currentRange && (currentRange.startContainer === currentRange.endContainer && currentRange.startOffset === currentRange.endOffset);
			if (!clear || isInside) {
				editorWysiwyg.saveSelection();
				editorWysiwyg.updateToolbar();
			}
		});

		return editorWysiwyg;
	};
	$.fn.wysiwyg.defaults = {
		hotKeys: {
			'ctrl+b meta+b': 'bold',
			'ctrl+i meta+i': 'italic',
			'ctrl+u meta+u': 'underline',
			'ctrl+z meta+z': 'undo',
			'ctrl+y meta+y meta+shift+z': 'redo',
			'ctrl+l meta+l': 'justifyleft',
			'ctrl+r meta+r': 'justifyright',
			'ctrl+e meta+e': 'justifycenter',
			'ctrl+j meta+j': 'justifyfull',
			'shift+tab': 'outdent',
			'tab': 'indent'
		},
		toolbarSelector: '[data-role=editor-toolbar]',
		commandRole: 'edit',
		activeToolbarClass: 'btn-info',
		selectionMarker: 'edit-focus-marker',
		selectionColor: 'darkgrey',
		dragAndDropImages: true,
		fileUploadError: function (reason, detail) { console.log("File upload error", reason, detail); },
		readFileIntoUrl: readFileIntoDataUrl,
		createLink: function (wysiwygObj) {
			var args = prompt("Enter the URL for this link:", "http://");
			document.execCommand("createLink", false, args);
			wysiwygObj.updateToolbar();
		},
		insertAsset: function(wysiwygObj) {
			wysiwygObj.toolbar.find('input[type=file][data-' + wysiwygObj.options.commandRole + ']').click();
			wysiwygObj.updateToolbar();
			return false;
		}

	};
}(jQuery));



interface Window { miniwebAdmin: any; }

(function () {

	var readFileIntoDataUrl = function (fileInfo) {
		return new Promise((resolve, reject) => {
			const fReader = new FileReader();
			fReader.onload = function (e) {
				resolve((<any>e.target).result);
			};
			fReader.onerror = reject;
			fReader.readAsDataURL(fileInfo);
		})
	};

	if (!document.querySelector("miniwebadmin")) {
		return;
	}
	var extend: any = function (defaults, options) {
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

		let contentEditables: NodeListOf<Element>;
		let btnNew: Element;
		let btnSavePage: Element;
		let btnEdit: Element;
		let btnSave: Element;
		let btnCancel: Element;

		const editContent = function () {
			document.querySelector('body').classList.add('miniweb-editing');
			//reassign arrays so al new items are parsed
			contentEditables = document.querySelectorAll('[data-miniwebprop]');
			contentEditables.forEach(el => el.setAttribute('contentEditable', "true"));

			for (var i = 0; i < options.editTypes.length; i++) {
				var editType = options.editTypes[i];

				contentEditables.forEach((ce: HTMLElement, ix) => {
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
			//$(".editor-toolbar").fadeIn().css("display", "block");

			//setupAssetPager();
		};
		const cancelEdit = function () {
			document.querySelector('body').classList.remove('miniweb-editing');
			contentEditables.forEach(el => el.removeAttribute('contentEditable'));


			for (var i = 0; i < options.editTypes.length; i++) {
				var editType = options.editTypes[i];

				contentEditables.forEach((ce: HTMLElement, ix) => {
					if (ce.dataset.miniwebedittype == editType.key) {
						editType.editEnd();
						//break because one does all
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
		const toggleContentInserts = function (on: boolean) {
			if (on) {
				document.querySelectorAll('[data-miniwebsection]').forEach(el => {
					const section = (el as HTMLElement).dataset.miniwebsection;
					el.insertAdjacentHTML('beforeend', '<button class="miniweb-insertcontent" data-add-content-to="' + section + '">add content</button>');
				});
				document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate] .miniweb-template-actions').forEach(el => el.remove());
				document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate]').forEach(el => el.insertAdjacentHTML('beforeend', '<div class="pull-right miniweb-template-actions"><button data-content-move="up">&#11014;</button><button data-content-move="down" >&#11015;</button>	<button class="danger" data-content-move="delete">&#11199;</button></div>'));


				/*
				$('.uploadimage').click(function (e) {
					e.preventDefault();
					$(this).closest('.editor-toolbar').find('.txtImage').click();
				});
				*/
			} else {
				document.querySelectorAll('.miniweb-insertcontent, .miniweb-template-actions').forEach(el => el.remove());
			}
		};
		const toggleSourceView = function () {
			// $(".source").bind("click", function () {
			// 	var self = $(this);
			// 	if (self.attr("data-cmd") === "source") {
			// 		self.attr("data-cmd", "design");
			// 		self.addClass("active");
			// 		var $content = self.closest('.editor-toolbar').next();
			// 		var html = $content.html();
			// 		html = html.replace(/\t/gi, '');
			// 		$content.text(html);
			// 		$content.wrapInner('<pre/>');
			// 	} else {
			// 		self.attr("data-cmd", "source");
			// 		self.removeClass("active");
			// 		var $content = self.closest('.editor-toolbar').next();
			// 		var html = $('pre', $content).text();
			// 		$content.html(html);

			// 	}
			// });
		};
		const getParsedHtml = function (source: HTMLElement) {
			var parsedDOM;
			parsedDOM = new DOMParser().parseFromString(source.innerHTML, 'text/html');
			parsedDOM = new XMLSerializer().serializeToString(parsedDOM);
			/<body>([\s\S]*)<\/body>/im.exec(parsedDOM);
			parsedDOM = RegExp.$1;
			return parsedDOM;
		};
		const saveContent = function (e) {
			if (!document.querySelector('body').classList.contains('miniweb-editing')) return;

			//TODO
			// if ($(".source").attr("data-cmd") === "design") {
			// 	$(".source").click();
			// }
			var items = [];
			document.querySelectorAll('[data-miniwebsection]').forEach((section: HTMLElement, index) => {
				var sectionid = section.dataset.miniwebsection;
				section.querySelectorAll('[data-miniwebtemplate]').forEach((tmpl: HTMLElement, tindex) => {
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
					//find all dynamic properties

					tmpl.querySelectorAll('[data-miniwebprop]').forEach((prop: HTMLElement, pindex) => {
						var key = prop.getAttribute('data-miniwebprop')
						var value = getParsedHtml(prop);
						item.Values[key] = value;
						//update attributes if any
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
					if (data.result) showMessage(true, "The page was saved successfully");
					else showMessage(false, "Save page failed");
					cancelEdit();
				});
		};
		const getVerificationToken = function () {
			return (<HTMLInputElement>document.querySelector('#miniweb-templates input[name=__RequestVerificationToken]')).value;
		};
		const savePage = function () {
			const form = (<HTMLFormElement>document.querySelector('#pageProperties form'));
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
					} else {
						showMessage(false, data.message);
					}
				}).catch(res => {
					showMessage(false, 'failed to post');
				});
		};
		const removePage = function () {
			if (confirm('are you sure?')) {
				// $.post(options.apiEndpoint + "removepage", {
				// 	'__RequestVerificationToken': $('#miniweb-templates input[name=__RequestVerificationToken]').val(),
				// 	url: $('#miniweb-admin-nav').attr('data-miniweb-path')
				// }).done(function (data) {
				// 	showMessage(true, "The page was saved successfully");
				// 	setTimeout(function () {
				// 		document.location.href = data.url
				// 	}, 1500);
				// }).fail(function (data) {
				// 	var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
				// 	showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
				// });
			}
		};
		const addNewPage = function () {
			//copy and empty current page property modal
			// if ($('#newPageProperties').length == 0) {
			// 	var newP = $('#pageProperties').clone();
			// 	newP.attr('id', 'newPageProperties');
			// 	$('input[name=OldUrl]', newP).remove();
			// 	$('input,textarea', newP).not('[type=hidden],[type=checkbox]').val('');
			// 	var parentUrl = $('#pageProperties input[name=Url]').val();
			// 	$('input[name=Url]', newP).val(parentUrl.substring(0, parentUrl.lastIndexOf('/') + 1));
			// 	adminTag.append(newP);
			// 	$('#newPageProperties .btn-primary').bind('click', savePage);
			// }
			// $('#newPageProperties').mw-modal();
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
				} catch (ex) {
					(<any>document.body).createTextRange().select();
					(<any>document).selection.empty();
				}

				selection.addRange(selectedRange);
			}
		};
		const ctrl_s_save = function (event) {
			if (document.querySelector('body').classList.contains('miniweb-editing')) {
				if (event.ctrlKey && event.keyCode == 83) {
					event.preventDefault();
					saveContent(event);
				};
			}
		};

		// const modalTriggers = document.querySelectorAll('[data-show-modal]');
		// modalTriggers.forEach((t, i) => {
		// 	t.addEventListener('click', (e) => {
		// 		if (e.target instanceof Element) {
		// 			const modalTargetSelector = (e.target as HTMLElement).dataset.showModal;
		// 			const modalTarget = document.querySelector(modalTargetSelector) as HTMLElement;
		// 			if (modalTarget) {
		// 				modalTarget.classList.contains('show') ? modalTarget.classList.remove('show') : modalTarget.classList.add('show');
		// 			}
		// 		}
		// 	});
		// });

		//global events...
		document.addEventListener('click', (e) => {
			const target = e.target as HTMLElement;
			console.log('documentclick', e, target);
			if (!target) return;
			if (target.dataset.showModal) {
				e.preventDefault();
				const modal = document.querySelector(target.dataset.showModal) as HTMLElement;
				if (modal) {
					modal.classList.contains('show') ? modal.classList.remove('show') : modal.classList.add('show');
				}
			} else if (target.dataset.dismiss) {
				e.preventDefault();
				document.querySelectorAll('.mw-modal.show').forEach(el => {
					el.classList.remove('show');
				});
			} else if (target.dataset.addContentTo) {
				e.preventDefault();
				const contentTarget = target.dataset.addContentTo;
				const modal = document.querySelector('#newContentAdd') as HTMLElement;
				modal.dataset.targetsection = contentTarget;
				modal.classList.add('show');
			} else if (target.dataset.addContentId) {
				e.preventDefault();
				const contentId = target.dataset.addContentId;
				const targetSection = (<HTMLElement>target.closest('.mw-modal')).dataset.targetsection;
				const el = <HTMLElement>(document.getElementById(contentId).firstElementChild.cloneNode(true));
				const section = document.querySelector('[data-miniwebsection=' + targetSection + ']');
				console.log(target, contentId, targetSection, section, el);
				section.append(el);
				cancelEdit();
				editContent();
				document.querySelectorAll('.mw-modal.show').forEach(el => {
					el.classList.remove('show');
				});
			} else if (target.dataset.contentMove) {
				const move = target.dataset.contentMove;
				const item = target.closest('[data-miniwebtemplate]');
				console.log('move', move, item, target);
				if (move == "up") {
					item.parentNode.insertBefore(item, item.previousElementSibling);
				} else if (move == "down") {
					item.parentNode.insertBefore(item, item.nextElementSibling.nextElementSibling);
				} else if (move == "delete") {
					if (confirm('are you sure?')) {
						item.remove();
					}
				} else {
					console.error("unknown move", move, target);
				}
			}
		});

		btnNew = document.getElementById("miniwebButtonNew");
		btnSavePage = document.getElementById("miniwebSavePage");
		btnEdit = document.getElementById("miniwebButtonEdit");//.bind("click", editContent);
		btnSave = document.getElementById("miniwebButtonSave");//.bind("click", saveContent);
		btnCancel = document.getElementById("miniwebButtonCancel");//.bind("click", cancelEdit);
		contentEditables = document.querySelectorAll('[data-miniwebprop]');

		btnSavePage.addEventListener('click', savePage);
		btnEdit.addEventListener('click', editContent);
		btnSave.addEventListener('click', saveContent);
		btnCancel.addEventListener('click', cancelEdit);

		// $('#pageProperties .btn-primary').bind('click', savePage);
		// $('#pageProperties .btn-danger').bind("click", removePage);
		// $('#newContentAdd .btn-primary').bind("click", addNewContent);
		// $('#newPage').bind('click', addNewPage);
		// $('#navigateOnEnter').bind('keypress', function (e) {
		// 	if (e.which == 13) {
		// 		document.location.href = $(this).val();
		// 	}
		// })


		window.addEventListener('keydown', ctrl_s_save, true);
		// $(document).keyup(function (e) {
		// 	if (!(<HTMLElement>document.activeElement).isContentEditable) {
		// 		if (e.keyCode === 27) { // ESC key
		// 			cancelEdit();
		// 		}
		// 	}
		// });
		//always cancel edit on refresh, stops remembering of firefox for inputs and stuff
		cancelEdit();

		return this;
	};

	//PAGER STUFF
	// var checkAssetPagerVisibility = function () {
	// 	if ($('#miniweb-assetlist li:not(".is-hidden")').length > 15 &&
	// 		$('#miniweb-assetlist li:visible:last').get(0) != $('#miniweb-assetlist li:not(".is-hidden"):last').get(0)) {
	// 		$('#miniweb-asset-page-right').show();

	// 	} else {
	// 		$('#miniweb-asset-page-right').hide();
	// 	}
	// 	if ($('#miniweb-assetlist').data('page') == 0) {

	// 		$('#miniweb-asset-page-left').hide();
	// 	} else {
	// 		$('#miniweb-asset-page-left').show();
	// 	}
	// };
	// var setupAssetPager = function () {
	// 	checkAssetPagerVisibility();
	// 	$(".miniweb-asset-pager").unbind('click').click(function () {
	// 		var page = $('#miniweb-assetlist').data('page');
	// 		var move = $(this).data('page-move');
	// 		console.log('goto page', page, move);
	// 		var newPage = page + move;
	// 		if (newPage < 0) newPage = 0;
	// 		$('#miniweb-assetlist').data('page', newPage);
	// 		newPage = (newPage * 15) + 1;

	// 		$('body.miniweb-editing #miniweb-assetlist li').hide();
	// 		$('body.miniweb-editing #miniweb-assetlist li:nth-child(n+' + newPage + '):nth-child(-n+' + (newPage + 14) + ')').css({ 'display': 'inline-block' })
	// 		checkAssetPagerVisibility();
	// 	});

	// };
	const htmlEscape = function (str) {
		return str
			.replace(/&/g, '&')
			.replace(/'/g, "'")
			.replace(/"/g, '"')
			.replace(/>/g, '>')
			.replace(/</g, '<');
	}

	var txtMessage = document.querySelector("miniwebadmin .alert");
	var showMessage = function (success: boolean, message: string, isHtml: boolean = false) {
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


	// $(document).on('click', 'button.add-multiplepages', function (e) {
	// 	e.preventDefault();
	// 	$('input[type=file].add-multiplepages').click().change(function () {
	// 		if (this.type === 'file' && this.files && this.files.length > 0) {
	// 			var formData = new FormData(<HTMLFormElement>$(this).closest('form')[0]);
	// 			formData.append('__RequestVerificationToken', $('#miniweb-templates input[name=__RequestVerificationToken]').val());
	// 			var apiEndpoint = $('#miniweb-admin-nav').data('miniweb-apiendpoint');
	// 			console.log('add file', this, apiEndpoint, formData);
	// 			$.ajax({
	// 				url: apiEndpoint + 'multiplepages',
	// 				data: formData,
	// 				processData: false,
	// 				contentType: false,
	// 				type: 'POST',
	// 				success: function (data) {
	// 					if (data && data.result) {
	// 						showMessage(true, "The pages were added successfully");
	// 					} else {
	// 						showMessage(false, "Save pages failed " + data.message);
	// 					}
	// 				},
	// 				error: function (data) {
	// 					console.log(data);
	// 					showMessage(false, "Save pages failed " + data.responseText);
	// 				}
	// 			});
	// 		}
	// 	});
	// });
	// $.fn.miniwebAdmin.createHyperlink = function (wysiwygObj: any) {
	// 	$('#addHyperLink .btn-primary').unbind('click.hyperlink.wysiwyg');
	// 	$('#addHyperLink .btn-primary').bind('click.hyperlink.wysiwyg', function () {
	// 		$('#addHyperLink').mw-modal('hide');
	// 		var args = $('#addHyperLink #createInternalUrl').val();
	// 		if (args == '') {
	// 			args = $('#addHyperLink #createLinkUrl').val();
	// 		}
	// 		$('#addHyperLink #createInternalUrl').val('');
	// 		$('#addHyperLink #createLinkUrl').val('http://');
	// 		wysiwygObj.restoreSelection();
	// 		wysiwygObj.me.focus();
	// 		document.execCommand("createLink", false, args);
	// 		wysiwygObj.updateToolbar();
	// 		wysiwygObj.saveSelection();
	// 	});
	// 	$('#addHyperLink #createInternalUrl').val('');
	// 	$('#addHyperLink #createLinkUrl').val('http://');
	// 	if (wysiwygObj.selectedRange.commonAncestorContainer.parentNode.tagName == 'A') {
	// 		var curHref = $(wysiwygObj.selectedRange.commonAncestorContainer.parentNode).attr('href');
	// 		if (curHref.indexOf('http') == 0) {
	// 			$('#addHyperLink #createLinkUrl').val(curHref);
	// 		} else {
	// 			$('#addHyperLink #createInternalUrl').val(curHref);
	// 		}
	// 	}
	// 	wysiwygObj.saveSelection();
	// 	$('#addHyperLink').mw-modal();
	// };
	// $.fn.miniwebAdmin.insertAsset = function (wysiwygObj: any) {
	// 	$('#miniweb-assetlist li img').each(function () {
	// 		var $this = $(this);
	// 		$this.attr('src', $this.data('src'));
	// 		//console.log($this);
	// 	});
	// 	$('.miniweb .select-asset-folder').unbind('change').bind('change', function () {
	// 		var val = $(this).val();
	// 		$('#miniweb-assetlist li').addClass("is-hidden");
	// 		var curEls = $('#miniweb-assetlist li[data-path="' + val + '"]');
	// 		curEls.removeClass('is-hidden');
	// 		//HACK:move to start in dom so css selectors for paging keep working
	// 		curEls.detach().prependTo('#miniweb-assetlist');
	// 		$('#miniweb-assetlist').data('page', 0);
	// 		setTimeout(function () { $('#miniweb-asset-page-reset').click(); }, 500);
	// 	}).unbind('input').bind('input', function () { $(this).change(); });
	// 	$('.miniweb .select-asset-folder').change();
	// 	$('#imageAdd button.add-asset').unbind('click').bind('click', function () {
	// 		$('input[type=file].add-asset').click().change(function () {
	// 			if (this.type === 'file' && this.files && this.files.length > 0) {
	// 				var formData = new FormData(<HTMLFormElement>$(this).closest('form')[0]);
	// 				formData.append('__RequestVerificationToken', $('#miniweb-templates input[name=__RequestVerificationToken]').val());
	// 				var apiEndpoint = $('#miniweb-admin-nav').data('miniweb-apiendpoint');
	// 				console.log('add file', this, apiEndpoint, formData);
	// 				$.ajax({
	// 					url: apiEndpoint + 'saveassets',
	// 					data: formData,
	// 					processData: false,
	// 					contentType: false,
	// 					type: 'POST',
	// 					success: function (data) {
	// 						if (data && data.result) {
	// 							for (var i = 0; i < data.assets.length; i++) {
	// 								var asset = data.assets[i];
	// 								if (asset.type == 0) {
	// 									$('#miniweb-assetlist').append('<li data-path="' + asset.folder + '"><img data-src="' + asset.virtualPath + '" src="' + asset.virtualPath + '" data-filename=' + asset.fileName + '" class="miniweb-asset-pick" ></li>')
	// 								} else {
	// 									$('#miniweb-assetlist').append('<li data-path="' + asset.folder + '"><a href="' + asset.virtualPath + '" >' + asset.fileName + '</a></li>')
	// 								}
	// 							}
	// 							//HACK:move to start in dom so css selectors for paging keep working
	// 							setTimeout(() => { $('.miniweb .select-asset-folder').change() }, 500);
	// 							showMessage(true, "The assets were saved successfully");
	// 						} else {
	// 							showMessage(false, "Save assets failed");
	// 						}
	// 					}
	// 				});
	// 			}
	// 			this.value = '';
	// 		});
	// 	});
	// 	$('#miniweb-assetlist').unbind("click").on('click', 'li', function (e) {
	// 		e.stopPropagation();
	// 		e.preventDefault();
	// 		var dataUrl = $('img', this).attr('src');

	// 		var filename = $('img', this).data('filename');
	// 		if (filename == null) {
	// 			filename = 'newfile.png';
	// 		}

	// 		var imageHtml = '<img src="' + dataUrl + '" data-filename="' + filename + '"/>';
	// 		wysiwygObj.execCommand('inserthtml', imageHtml);
	// 		$('#imageAdd').mw-modal('hide');
	// 	});
	// 	$('#imageAdd').mw-modal();
	// }
	const miniwebAdminDefaults = {
		apiEndpoint: '/miniweb-api/',
		editTypes: [
			{
				key: 'html',
				editStart: function (element: HTMLElement, index) {
					var thisTools = <HTMLElement>(document.getElementById('tools').cloneNode(true));
					thisTools.removeAttribute("id");
					thisTools.classList.add('miniweb-editor-toolbar');

					element.parentNode.insertBefore(thisTools, element);
					thisTools.querySelectorAll('button').forEach((b, i) => {
						b.addEventListener('click', (e) => {
							const commandWithArgs = b.dataset.edit;
							if (commandWithArgs) {
								e.preventDefault();
								e.stopPropagation();
								const commandArr = commandWithArgs.split(' '),
									command = commandArr.shift(),
									args = commandArr.join(' ');
								document.execCommand(command, false, args);
							} else if (b.dataset.custom) {
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
					// $(this).addClass('miniweb-asset-edit').unbind('click').click(function (e) {
					// 	var $el = $(this);
					// 	//only trigger on :after click...
					// 	if (e.offsetX > this.offsetWidth) {
					// 		$('#imageAdd button.add-asset').unbind('click').bind('click', function () {

					// 			$('input[type=file].add-asset').click().change(function () {
					// 				if (this.type === 'file' && this.files && this.files.length > 0) {
					// 					//post to apiupload

					// 					//only first for now
					// 					var fileInfo = this.files[0];
					// 					$.when(readFileIntoDataUrl(fileInfo)).done(function (dataUrl) {
					// 						dataUrl = dataUrl.replace(';base64', ';filename=' + fileInfo.name + ';base64')
					// 						var imageHtml = '<img src="' + dataUrl + '" data-filename="' + fileInfo.name + '"/>';
					// 						var el = $('<li></li>');
					// 						el.append(imageHtml);
					// 						$('#miniweb-assetlist').append(el);
					// 					}).fail(function (e) {
					// 						alert("file-reader" + e);
					// 					});
					// 				}
					// 				this.value = '';
					// 			});
					// 		});
					// 		$('#miniweb-assetlist').unbind("click").on('click', 'li', function (e) {
					// 			e.stopPropagation();
					// 			e.preventDefault();
					// 			$el.text($('img', this).attr('src'));
					// 			$('#imageAdd').mw-modal('hide');
					// 		});
					// 		$('#imageAdd').mw-modal();
					// 	}
					// });
				},
				editEnd: function (index) {
					//$(this).removeClass('miniweb-asset-edit');
				}
			},
			{
				key: 'url',
				editStart: function (index) {
					// $(this).addClass('miniweb-url-edit').unbind('click').click(function (e) {
					// 	var $el = $(this);
					// 	//only trigger on :after click...
					// 	if (e.offsetX > this.offsetWidth) {
					// 		var curHref = $el.text();
					// 		if (curHref.indexOf('http') == 0) {
					// 			$('#addHyperLink #createLinkUrl').val(curHref);
					// 		} else {
					// 			$('#addHyperLink #createInternalUrl').val(curHref);
					// 		}
					// 		$('#addHyperLink .btn-primary').unbind('click').bind('click', function () {
					// 			var newHref = $('#addHyperLink #createInternalUrl').val();
					// 			if (newHref == '') {
					// 				newHref = $('#addHyperLink #createLinkUrl').val();
					// 			}
					// 			$el.text(newHref);
					// 			$('#addHyperLink').mw-modal('hide');
					// 		});
					// 		$('#addHyperLink').mw-modal();
					// 	}
					// });
				},
				editEnd: function (index) {
					//$(this).removeClass('miniweb-url-edit');
				}
			}
		]
	};
	const toggleHiddenMenuItems = function (on: Boolean) {
		const items = document.querySelectorAll('.miniweb-hidden-menu');
		items.forEach((item, ix) => {
			if (on) {
				item.classList.add('show');
			} else {
				item.classList.remove('show');
			}
		});
	};

	document.querySelector('#miniwebShowHiddenPages input').addEventListener('click', (e) => {
		sessionStorage.setItem('miniwebShowHiddenPages', (<HTMLInputElement>(e.target)).checked ? "true" : "false");
		toggleHiddenMenuItems((<HTMLInputElement>(e.target)).checked);
	});

	if (sessionStorage.getItem('miniwebShowHiddenPages') === "true") {
		(<HTMLInputElement>document.querySelector('#miniwebShowHiddenPages input')).checked = true
		toggleHiddenMenuItems(true);
	} else {
		toggleHiddenMenuItems(false);
	}
})();
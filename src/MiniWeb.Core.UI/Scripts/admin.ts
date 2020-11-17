
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

			//TODO:
			// for (var i = 0; i < options.editTypes.length; i++) {
			// 	var editType = options.editTypes[i];
			// 	contentEditables.filter('[data-miniwebedittype=' + editType.key + ']').each(editType.editStart);
			// }

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

			//TODO:
			// for (var i = 0; i < options.editTypes.length; i++) {
			// 	var editType = options.editTypes[i];
			// 	contentEditables.filter('[data-miniwebedittype=' + editType.key + ']').each(editType.editEnd);
			// }

			btnNew.removeAttribute("disabled");
			btnEdit.removeAttribute("disabled");
			btnSave.setAttribute("disabled", "true");
			btnCancel.setAttribute("disabled", "true");

			toggleContentInserts(false);

		};
		const toggleContentInserts = function (on: boolean) {
			if (on) {
				document.querySelectorAll('[data-miniwebsection]').forEach(el => el.insertAdjacentHTML('beforeend', '<a href="#" class="miniweb-insertcontent btn btn-info" data-toggle="modal" data-target="#newContentAdd">add content</a>'));
				document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate] .miniweb-template-actions').forEach(el => el.remove());
				document.querySelectorAll('[data-miniwebsection] [data-miniwebtemplate]').forEach(el => el.insertAdjacentHTML('beforeend', '<div class="btn-group pull-right miniweb-template-actions"><a class="btn btn-mini articleUp" ><i class="glyphicon glyphicon-arrow-up" > </i> </a><a class="btn btn-mini articleDown" > <i class="glyphicon glyphicon-arrow-down" > </i> </a>	<a class="btn btn-mini remove" title= "Remove article" > <i class="glyphicon glyphicon-remove" > </i> remove article</a>	</div>'));

				/*
				$('.miniweb-insertcontent').click(function () {
					$('#newContentAdd').attr('data-targetsection', $(this).closest('[data-miniwebsection]').attr('data-miniwebsection'));
				});
				$('[data-miniwebtemplate] .miniweb-template-actions .btn.remove').click(function () {
					if (confirm('are you sure?')) {
						$(this).closest('[data-miniwebtemplate]').remove();
					}
				});
				$('[data-miniwebtemplate] .miniweb-template-actions .btn.articleUp').click(function () {
					var cur = $(this).closest('[data-miniwebtemplate]');
					cur.prev().before(cur);
				});
				$('[data-miniwebtemplate] .miniweb-template-actions .btn.articleDown').click(function () {
					var cur = $(this).closest('[data-miniwebtemplate]');
					cur.next().after(cur);
				});
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
		const getParsedHtml = function (source: any) {
			var parsedDOM;
			parsedDOM = new DOMParser().parseFromString(source.cleanHtml(), 'text/html');
			parsedDOM = new XMLSerializer().serializeToString(parsedDOM);
			/<body>([\s\S]*)<\/body>/im.exec(parsedDOM);
			parsedDOM = RegExp.$1;
			return $.trim(parsedDOM);
		};
		const saveContent = function (e) {
			if (!document.querySelector('body').classList.contains('miniweb-editing')) return;

			//TODO
			// if ($(".source").attr("data-cmd") === "design") {
			// 	$(".source").click();
			// }

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
					//find all dynamic properties

					tmpl.querySelectorAll('[data-miniwebprop]').forEach((prop, pindex) => {
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

			console.log(JSON.stringify(items));
			showMessage(true, "TEST SUCCESS");
			// $.post(options.apiEndpoint + 'savecontent', {
			// 	url: $('#admin').attr('data-miniweb-path'),
			// 	items: JSON.stringify(items),
			// 	'__RequestVerificationToken': $('#miniweb-templates input[name=__RequestVerificationToken]').val()
			// }).done(function (data) {
			// 	if (data.result) {
			// 		showMessage(true, "The page was saved successfully");
			// 	} else {
			// 		showMessage(false, "Save page failed");
			// 	}
			// 	cancelEdit();
			// }).fail(function (data) {

			// 	var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
			// 	showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
			// });

		};
		const savePage = function () {
			const form = (<HTMLFormElement>document.querySelector('#pageProperties form'));
			const items = form.querySelectorAll('select,input,textarea');
			let formArr = new FormData();
			items.forEach((el: HTMLInputElement, ix) => {
				console.log(el.getAttribute('name'), el.value);
				formArr.append(el.getAttribute('name'), el.value);
			});
			formArr.append('__RequestVerificationToken', (<HTMLInputElement>document.querySelector('#miniweb-templates input[name=__RequestVerificationToken]')).value);
			console.log('savePage', options.apiEndpoint, formArr);
			fetch(options.apiEndpoint + "savepage", {
				method: "POST",				
				body: formArr
			}).then(res => res.json())
			.then(data => {
				if (data.result) {
					showMessage(true, "saved page successfully");
					document.querySelector('.modal.show').classList.remove('show');
				} else { 
					showMessage(false, data.message);
				}
			}).catch(res => {
				showMessage(false, 'failed to post');
			});
			// $.post(options.apiEndpoint + "savepage",
			// 	formArr
			// ).done(function (data) {
			// 	if (data && data.result) {
			// 		document.location.href = data.url;
			// 	} else {
			// 		showMessage(false, data.message);
			// 	}
			// }).fail(function (data) {
			// 	var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
			// 	showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
			// });
		};
		const removePage = function () {
			if (confirm('are you sure?')) {
				// $.post(options.apiEndpoint + "removepage", {
				// 	'__RequestVerificationToken': $('#miniweb-templates input[name=__RequestVerificationToken]').val(),
				// 	url: $('#admin').attr('data-miniweb-path')
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
		const addNewContent = function () {
			// var htmlid = $(this).data('contentid');
			// var target = $(this).closest('#newContentAdd').attr('data-targetsection');
			// $('[data-miniwebsection=' + target + ']').append($('#' + htmlid).html());
			// cancelEdit();
			// editContent();
			// $('#newContentAdd').modal('hide');
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
			// $('#newPageProperties').modal();
		};
		const ctrl_s_save = function (event) {
			if (document.querySelector('body').classList.contains('miniweb-editing')) {
				if (event.ctrlKey && event.keyCode == 83) {
					event.preventDefault();
					saveContent(event);
				};
			}
		};

		const modalTriggers = document.querySelectorAll('[data-show-modal]');
		modalTriggers.forEach((t, i) => {
			t.addEventListener('click', (e) => {
				if (e.target instanceof Element) {
					const modalTargetSelector = (e.target as HTMLElement).dataset.showModal;
					const modalTarget = document.querySelector(modalTargetSelector) as HTMLElement;
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
					const modalTargetSelector = (e.target as HTMLElement).dataset.dismiss;
					const modalTarget = document.querySelector(modalTargetSelector) as HTMLElement;
					if (modalTarget) {
						modalTarget.classList.remove('show');
					}
				}
			});
		});

		btnNew = document.getElementById("btnNew");
		btnSavePage = document.getElementById("miniwebSavePage");
		btnEdit = document.getElementById("btnEdit");//.bind("click", editContent);
		btnSave = document.getElementById("btnSave");//.bind("click", saveContent);
		btnCancel = document.getElementById("btnCancel");//.bind("click", cancelEdit);
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
	// 			var apiEndpoint = $('#admin').data('miniweb-apiendpoint');
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
	// 		$('#addHyperLink').modal('hide');
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
	// 	$('#addHyperLink').modal();
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
	// 				var apiEndpoint = $('#admin').data('miniweb-apiendpoint');
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
	// 		$('#imageAdd').modal('hide');
	// 	});
	// 	$('#imageAdd').modal();
	// }
	const miniwebAdminDefaults = {
		apiEndpoint: '/miniweb-api/',
		editTypes: [
			{
				key: 'html',
				editStart: function (index) {
					var thisTools = <HTMLElement>(document.getElementById('tools').cloneNode(true));
					thisTools.setAttribute("id", "");
					thisTools.setAttribute("data-role", "editor-toolbar" + index);
					thisTools.classList.add("editor-toolbar");

					this.before(thisTools);
					// $(this).wysiwyg({
					// 	hotKeys: {},
					// 	activeToolbarClass: "active",
					// 	toolbarSelector: '[data-role=editor-toolbar' + index + ']',
					// 	createLink: $.fn.miniwebAdmin.createHyperlink,
					// 	insertAsset: $.fn.miniwebAdmin.insertAsset
					// });
				},
				editEnd: function (index) {
					document.querySelectorAll(".editor-toolbar").forEach(tb => tb.remove());
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
					// 			$('#imageAdd').modal('hide');
					// 		});
					// 		$('#imageAdd').modal();
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
					// 			$('#addHyperLink').modal('hide');
					// 		});
					// 		$('#addHyperLink').modal();
					// 	}
					// });
				},
				editEnd: function (index) {
					//$(this).removeClass('miniweb-url-edit');
				}
			}
		]
	};

	document.querySelector('#showHiddenPages input').addEventListener('click', (e) => {
		sessionStorage.setItem('showhiddenpages', (<HTMLInputElement>(e.target)).checked ? "true" : "false");
		//$('.miniweb-hidden-menu').toggle($(this).is(':checked'));
	});
	// if (sessionStorage.getItem('showhiddenpages') === "true") {
	// 	$('.miniweb-hidden-menu').toggle(true);
	// 	$('#showHiddenPages input').attr('checked', 'checked');
	// }
})();
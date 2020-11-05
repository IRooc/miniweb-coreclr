﻿/// <reference path="jquery.d.ts" />
/// <reference path="bootstrap.d.ts" />
/// <reference path="bootstrap-wysiwyg.ts" />
if (typeof jQuery === 'undefined') {
	// jQuery is not loaded
	var script = document.createElement('script');
	script.type = "text/javascript";
	script.src = "https://code.jquery.com/jquery-2.2.4.min.js";
	document.getElementsByTagName('head')[0].appendChild(script);
}
if (typeof $().modal !== 'function') {
	var script = document.createElement('script');
	script.type = "text/javascript";
	script.src = "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js";
	document.getElementsByTagName('head')[0].appendChild(script);
}

(function ($) {
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

	$.fn.miniwebAdmin = function (userOptions) {
		var adminTag = $(this);
		var options = $.extend({}, $.fn.miniwebAdmin.defaults, userOptions);
		var contentEditables, txtMessage, btnNew, btnEdit, btnSave, btnCancel,

			editContent = function () {
				$('body').addClass('miniweb-editing');
				//reassign arrays so al new items are parsed
				contentEditables = $('[data-miniwebprop]');
				contentEditables.attr('contentEditable', true);

				for (var i = 0; i < options.editTypes.length; i++) {
					var editType = options.editTypes[i];
					contentEditables.filter('[data-miniwebedittype=' + editType.key + ']').each(editType.editStart);
				}

				btnNew.attr("disabled", true);
				btnEdit.attr("disabled", true);
				btnSave.removeAttr("disabled");
				btnCancel.removeAttr("disabled");

				toggleContentInserts(true);

				toggleSourceView();
				$(".editor-toolbar").fadeIn().css("display", "block");

				setupAssetPager();
			},
			cancelEdit = function () {
				$('body').removeClass('miniweb-editing');
				contentEditables.removeAttr('contentEditable');

				for (var i = 0; i < options.editTypes.length; i++) {
					var editType = options.editTypes[i];
					contentEditables.filter('[data-miniwebedittype=' + editType.key + ']').each(editType.editEnd);
				}
				btnCancel.focus();

				btnNew.removeAttr("disabled");
				btnEdit.removeAttr("disabled");
				btnSave.attr("disabled", true);
				btnCancel.attr("disabled", true);

				toggleContentInserts(false);

			},
			toggleContentInserts = function (on: boolean) {
				if (on) {
					$('[data-miniwebsection]').append('<a href="#" class="miniweb-insertcontent btn btn-info" data-toggle="modal" data-target="#newContentAdd">add content</a>');
					$('[data-miniwebsection] [data-miniwebtemplate] .miniweb-template-actions').remove();
					$('[data-miniwebsection] [data-miniwebtemplate]').append('<div class="btn-group pull-right miniweb-template-actions"><a class="btn btn-mini articleUp" ><i class="glyphicon glyphicon-arrow-up" > </i> </a><a class="btn btn-mini articleDown" > <i class="glyphicon glyphicon-arrow-down" > </i> </a>	<a class="btn btn-mini remove" title= "Remove article" > <i class="glyphicon glyphicon-remove" > </i> remove article</a>	</div>');

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
				} else {
					$('.miniweb-insertcontent, .miniweb-template-actions').remove();
				}
			},
			toggleSourceView = function () {
				$(".source").bind("click", function () {
					var self = $(this);
					if (self.attr("data-cmd") === "source") {
						self.attr("data-cmd", "design");
						self.addClass("active");
						var $content = self.closest('.editor-toolbar').next();
						var html = $content.html();
						html = html.replace(/\t/gi, '');
						$content.text(html);
						$content.wrapInner('<pre/>');
					} else {
						self.attr("data-cmd", "source");
						self.removeClass("active");
						var $content = self.closest('.editor-toolbar').next();
						var html = $('pre', $content).text();
						$content.html(html);

					}
				});
			},
			getParsedHtml = function (source: any) {
				var parsedDOM;
				parsedDOM = new DOMParser().parseFromString(source.cleanHtml(), 'text/html');
				parsedDOM = new XMLSerializer().serializeToString(parsedDOM);
				/<body>([\s\S]*)<\/body>/im.exec(parsedDOM);
				parsedDOM = RegExp.$1;
				return $.trim(parsedDOM);
			},
			saveContent = function (e) {
				if (!$('body').hasClass('miniweb-editing')) return;

				if ($(".source").attr("data-cmd") === "design") {
					$(".source").click();
				}
				var items = [];
				$('[data-miniwebsection]').each(function (index) {
					var sectionid = $(this).attr('data-miniwebsection');
					$('[data-miniwebtemplate]', this).each(function () {
						var $tmpl = $(this);
						if (items[index] == null) {
							items[index] = {};
							items[index].Key = sectionid;
							items[index].Items = [];
						}
						var item = {
							Template: $(this).attr('data-miniwebtemplate'),
							Values: {}
						};
						//find all dynamic properties
						$tmpl.find('[data-miniwebprop]').each(function () {
							var key = $(this).attr('data-miniwebprop')
							var value = getParsedHtml($(this));
							item.Values[key] = value;
							//update attributes if any
							if (key.indexOf(':') > 0) {
								var orig = key.split(':')[0];
								var attrib = key.split(':')[1];
								$tmpl.find('[data-miniwebprop="' + orig + '"]').attr(attrib, value);
							}
						});
						items[index].Items.push(item);
					});
				});

				//console.log(JSON.stringify(items));

				$.post(options.apiEndpoint + 'savecontent', {
					url: $('#admin').attr('data-miniweb-path'),
					items: JSON.stringify(items),
					'__RequestVerificationToken': $('#miniweb-templates input[name=__RequestVerificationToken]').val()
				}).done(function (data) {
					if (data.result) {
						showMessage(true, "The page was saved successfully");
					} else {
						showMessage(false, "Save page failed");
					}
					cancelEdit();
				}).fail(function (data) {

					var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
					showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
				});

			},
			savePage = function () {
				var formArr = $(this).closest('#pageProperties').find('form').serializeArray();
				formArr.push({ name: '__RequestVerificationToken', value: $('#miniweb-templates input[name=__RequestVerificationToken]').val() });
				$.post(options.apiEndpoint + "savepage",
					formArr
				).done(function (data) {
					if (data && data.result) {
						document.location.href = data.url;
					} else {
						showMessage(false, data.message);
					}
				}).fail(function (data) {
					var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
					showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
				});
			},
			removePage = function () {
				if (confirm('are you sure?')) {
					$.post(options.apiEndpoint + "removepage", {
						'__RequestVerificationToken': $('#miniweb-templates input[name=__RequestVerificationToken]').val(),
						url: $('#admin').attr('data-miniweb-path')
					}).done(function (data) {
						showMessage(true, "The page was saved successfully");
						setTimeout(function () {
							document.location.href = data.url
						}, 1500);
					}).fail(function (data) {
						var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
						showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
					});
				}
			},
			addNewContent = function () {
				var htmlid = $(this).data('contentid');
				var target = $(this).closest('#newContentAdd').attr('data-targetsection');
				$('[data-miniwebsection=' + target + ']').append($('#' + htmlid).html());
				cancelEdit();
				editContent();
				$('#newContentAdd').modal('hide');
			},
			addNewPage = function () {
				//copy and empty current page property modal
				if ($('#newPageProperties').length == 0) {
					var newP = $('#pageProperties').clone();
					newP.attr('id', 'newPageProperties');
					$('input[name=OldUrl]', newP).remove();
					$('input,textarea', newP).not('[type=hidden],[type=checkbox]').val('');
					var parentUrl = $('#pageProperties input[name=Url]').val();
					$('input[name=Url]', newP).val(parentUrl.substring(0, parentUrl.lastIndexOf('/') + 1));
					adminTag.append(newP);
					$('#newPageProperties .btn-primary').bind('click', savePage);
				}
				$('#newPageProperties').modal();
			},
			ctrl_s_save = function (event) {
				if ($('body').hasClass('miniweb-editing')) {
					if (event.ctrlKey && event.keyCode == 83) {
						event.preventDefault();
						saveContent(event);
					};
				}
			};


		btnNew = $("#btnNew");
		btnEdit = $("#btnEdit").bind("click", editContent);
		btnSave = $("#btnSave").bind("click", saveContent);
		btnCancel = $("#btnCancel").bind("click", cancelEdit);
		contentEditables = $('[data-miniwebprop]');
		$('#pageProperties .btn-primary').bind('click', savePage);
		$('#pageProperties .btn-danger').bind("click", removePage);
		$('#newContentAdd .btn-primary').bind("click", addNewContent);
		$('#newPage').bind('click', addNewPage);
		$('#navigateOnEnter').bind('keypress', function(e){
			if (e.which ==13) {
				document.location.href = $(this).val();
			}
		})


		window.addEventListener('keydown', ctrl_s_save, true);
		$(document).keyup(function (e) {
			if (!(<HTMLElement>document.activeElement).isContentEditable) {
				if (e.keyCode === 27) { // ESC key
					cancelEdit();
				}
			}
		});
		//always cancel edit on refresh, stops remembering of firefox for inputs and stuff
		cancelEdit();

		return this;
	};
	var checkAssetPagerVisibility = function () {
		if ($('#miniweb-assetlist li:not(".is-hidden")').length > 15 &&
			$('#miniweb-assetlist li:visible:last').get(0) != $('#miniweb-assetlist li:not(".is-hidden"):last').get(0)) {
			$('#miniweb-asset-page-right').show();

		} else {
			$('#miniweb-asset-page-right').hide();
		}
		if ($('#miniweb-assetlist').data('page') == 0) {

			$('#miniweb-asset-page-left').hide();
		} else {
			$('#miniweb-asset-page-left').show();
		}
	};
	var setupAssetPager = function () {
		checkAssetPagerVisibility();
		$(".miniweb-asset-pager").unbind('click').click(function () {
			var page = $('#miniweb-assetlist').data('page');
			var move = $(this).data('page-move');
			console.log('goto page', page, move);
			var newPage = page + move;
			if (newPage < 0) newPage = 0;
			$('#miniweb-assetlist').data('page', newPage);
			newPage = (newPage * 15) + 1;

			$('body.miniweb-editing #miniweb-assetlist li').hide();
			$('body.miniweb-editing #miniweb-assetlist li:nth-child(n+' + newPage + '):nth-child(-n+' + (newPage + 14) + ')').css({ 'display': 'inline-block' })
			checkAssetPagerVisibility();
		});

	};
	var txtMessage = $("#admin .alert");
	var showMessage = function (success: boolean, message: string, isHtml: boolean = false) {
		var className = success ? "alert-success" : "alert-danger";
		var timeout = success ? 4000 : 8000;
		txtMessage.addClass(className);
		if (isHtml)
			txtMessage.html(message);
		else
			txtMessage.text(message);
		txtMessage.parent().fadeIn();

		setTimeout(function () {
			txtMessage.parent().fadeOut("slow", function () {
				txtMessage.removeClass(className);
			});
		}, timeout);
	};

	
	$(document).on('click', 'button.add-multiplepages', function (e) {
		e.preventDefault();
		$('input[type=file].add-multiplepages').click().change(function () {
			if (this.type === 'file' && this.files && this.files.length > 0) {
				var formData = new FormData(<HTMLFormElement>$(this).closest('form')[0]);
				formData.append('__RequestVerificationToken', $('#miniweb-templates input[name=__RequestVerificationToken]').val());
				var apiEndpoint = $('#admin').data('miniweb-apiendpoint');
				console.log('add file', this, apiEndpoint, formData);
				$.ajax({
					url: apiEndpoint + 'multiplepages',
					data: formData,
					processData: false,
					contentType: false,
					type: 'POST',
					success: function (data) {
						if (data && data.result) {
							showMessage(true, "The pages were added successfully");
						} else {
							showMessage(false, "Save pages failed " + data.message );
						}
					},
					error: function (data) {
						console.log(data);
						showMessage(false, "Save pages failed " + data.responseText);
					}
				});
			}
		});
	});
	$.fn.miniwebAdmin.createHyperlink = function (wysiwygObj: any) {
		$('#addHyperLink .btn-primary').unbind('click.hyperlink.wysiwyg');
		$('#addHyperLink .btn-primary').bind('click.hyperlink.wysiwyg', function () {
			$('#addHyperLink').modal('hide');
			var args = $('#addHyperLink #createInternalUrl').val();
			if (args == '') {
				args = $('#addHyperLink #createLinkUrl').val();
			}
			$('#addHyperLink #createInternalUrl').val('');
			$('#addHyperLink #createLinkUrl').val('http://');
			wysiwygObj.restoreSelection();
			wysiwygObj.me.focus();
			document.execCommand("createLink", false, args);
			wysiwygObj.updateToolbar();
			wysiwygObj.saveSelection();
		});
		$('#addHyperLink #createInternalUrl').val('');
		$('#addHyperLink #createLinkUrl').val('http://');
		if (wysiwygObj.selectedRange.commonAncestorContainer.parentNode.tagName == 'A') {
			var curHref = $(wysiwygObj.selectedRange.commonAncestorContainer.parentNode).attr('href');
			if (curHref.indexOf('http') == 0) {
				$('#addHyperLink #createLinkUrl').val(curHref);
			} else {
				$('#addHyperLink #createInternalUrl').val(curHref);
			}
		}
		wysiwygObj.saveSelection();
		$('#addHyperLink').modal();
	};
	$.fn.miniwebAdmin.insertAsset = function (wysiwygObj: any) {
		$('#miniweb-assetlist li img').each(function () {
			var $this = $(this);
			$this.attr('src',$this.data('src'));
			//console.log($this);
		});
		$('.miniweb .select-asset-folder').unbind('change').bind('change', function () {
			var val = $(this).val();
			$('#miniweb-assetlist li').addClass("is-hidden");
			var curEls = $('#miniweb-assetlist li[data-path="' + val + '"]');
			curEls.removeClass('is-hidden');
			//HACK:move to start in dom so css selectors for paging keep working
			curEls.detach().prependTo('#miniweb-assetlist');
			$('#miniweb-assetlist').data('page', 0);
			setTimeout(function () { $('#miniweb-asset-page-reset').click(); }, 500);
		}).unbind('input').bind('input', function () { $(this).change();});
		$('.miniweb .select-asset-folder').change();
		$('#imageAdd button.add-asset').unbind('click').bind('click', function () {
			$('input[type=file].add-asset').click().change(function () {
				if (this.type === 'file' && this.files && this.files.length > 0) {
					var formData = new FormData(<HTMLFormElement>$(this).closest('form')[0]);
					formData.append('__RequestVerificationToken', $('#miniweb-templates input[name=__RequestVerificationToken]').val());
					var apiEndpoint = $('#admin').data('miniweb-apiendpoint');
					console.log('add file', this, apiEndpoint, formData);
					$.ajax({
						url: apiEndpoint + 'saveassets',
						data: formData,
						processData: false,
						contentType: false,
						type: 'POST',
						success: function (data) {
							if (data && data.result) {
								for (var i = 0; i < data.assets.length; i++) {
									var asset = data.assets[i];
									if (asset.type == 0) {
										$('#miniweb-assetlist').append('<li data-path="' + asset.folder + '"><img data-src="' + asset.virtualPath + '" src="' + asset.virtualPath + '" data-filename=' + asset.fileName + '" class="miniweb-asset-pick" ></li>')
									} else {
										$('#miniweb-assetlist').append('<li data-path="' + asset.folder + '"><a href="' + asset.virtualPath + '" >' + asset.fileName + '</a></li>')
									}
								}
								//HACK:move to start in dom so css selectors for paging keep working
								setTimeout(() => { $('.miniweb .select-asset-folder').change() }, 500);
								showMessage(true, "The assets were saved successfully");
							} else {
								showMessage(false, "Save assets failed");
							}
						}
					});
				}
				this.value = '';
			});
		});
		$('#miniweb-assetlist').unbind("click").on('click', 'li', function (e) {
			e.stopPropagation();
			e.preventDefault();
			var dataUrl = $('img', this).attr('src');

			var filename = $('img', this).data('filename');
			if (filename == null) {
				filename = 'newfile.png';
			}

			var imageHtml = '<img src="' + dataUrl + '" data-filename="' + filename + '"/>';
			wysiwygObj.execCommand('inserthtml', imageHtml);
			$('#imageAdd').modal('hide');
		});
		$('#imageAdd').modal();
	}
	$.fn.miniwebAdmin.defaults = {
		apiEndpoint: '/miniweb-api/',
		editTypes: [
			{
				key: 'html',
				editStart: function (index) {
					var thisTools = $('#tools').clone();
					thisTools.attr('id', '').attr('data-role', 'editor-toolbar' + index).addClass('editor-toolbar');
					$(this).before(thisTools);
					$(this).wysiwyg({
						hotKeys: {},
						activeToolbarClass: "active",
						toolbarSelector: '[data-role=editor-toolbar' + index + ']',
						createLink: $.fn.miniwebAdmin.createHyperlink,
						insertAsset: $.fn.miniwebAdmin.insertAsset
					});
				},
				editEnd: function (index) {
					$(".editor-toolbar").remove();
				}
			},
			{
				key: 'asset',
				editStart: function (index) {
					$(this).addClass('miniweb-asset-edit').unbind('click').click(function (e) {
						var $el = $(this);
						//only trigger on :after click...
						if (e.offsetX > this.offsetWidth) {
							$('#imageAdd button.add-asset').unbind('click').bind('click', function () {

								$('input[type=file].add-asset').click().change(function () {
									if (this.type === 'file' && this.files && this.files.length > 0) {
										//post to apiupload

										//only first for now
										var fileInfo = this.files[0];
										$.when(readFileIntoDataUrl(fileInfo)).done(function (dataUrl) {
											dataUrl = dataUrl.replace(';base64', ';filename=' + fileInfo.name + ';base64')
											var imageHtml = '<img src="' + dataUrl + '" data-filename="' + fileInfo.name + '"/>';
											var el = $('<li></li>');
											el.append(imageHtml);
											$('#miniweb-assetlist').append(el);
										}).fail(function (e) {
											alert("file-reader" + e);
										});
									}
									this.value = '';
								});
							});
							$('#miniweb-assetlist').unbind("click").on('click', 'li', function (e) {
								e.stopPropagation();
								e.preventDefault();
								$el.text($('img', this).attr('src'));
								$('#imageAdd').modal('hide');
							});
							$('#imageAdd').modal();
						}
					});
				},
				editEnd: function (index) {
					$(this).removeClass('miniweb-asset-edit');
				}
			},
			{
				key: 'url',
				editStart: function (index) {
					$(this).addClass('miniweb-url-edit').unbind('click').click(function (e) {
						var $el = $(this);
						//only trigger on :after click...
						if (e.offsetX > this.offsetWidth) {
							var curHref = $el.text();
							if (curHref.indexOf('http') == 0) {
								$('#addHyperLink #createLinkUrl').val(curHref);
							} else {
								$('#addHyperLink #createInternalUrl').val(curHref);
							}
							$('#addHyperLink .btn-primary').unbind('click').bind('click', function () {
								var newHref = $('#addHyperLink #createInternalUrl').val();
								if (newHref == '') {
									newHref = $('#addHyperLink #createLinkUrl').val();
								}
								$el.text(newHref);
								$('#addHyperLink').modal('hide');
							});
							$('#addHyperLink').modal();
						}
					});
				},
				editEnd: function (index) {
					$(this).removeClass('miniweb-url-edit');
				}
			}
		]
	};

	$('#showHiddenPages input').click(function () {
		sessionStorage.setItem('showhiddenpages', <string><any>$(this).is(':checked'));
		$('.miniweb-hidden-menu').toggle($(this).is(':checked'));
	});
	if (sessionStorage.getItem('showhiddenpages') === "true") {
		$('.miniweb-hidden-menu').toggle(true);
		$('#showHiddenPages input').attr('checked', 'checked');
	}
})(jQuery);
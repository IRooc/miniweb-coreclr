/// <reference path="jquery.d.ts" />
(function($) {

	var contentEditables, contentHtmlEditables, txtMessage, btnNew, btnEdit, btnSave, btnCancel,

		createHyperLink = function(editor, saveSelection, restoreSelection, updateToolbar) {

			$('#addHyperLink .btn-primary').unbind('click');
			$('#addHyperLink .btn-primary').bind('click', function() {
				$('#addHyperLink').modal('hide');
				var args = $('#addHyperLink #createInternalUrl').val();
				if (args == '') {
					args = $('#addHyperLink #createLinkUrl').val();
				}
				$('#addHyperLink #createInternalUrl').val('');
				$('#addHyperLink #createLinkUrl').val('http://');
				restoreSelection();
				editor.focus();
				document.execCommand("createLink", false, args);
				updateToolbar();
				saveSelection();
			});
			saveSelection()
			$('#addHyperLink').modal();
		},
		editContent = function() {
			$('body').addClass('miniweb-editing');
			//reassign arrays so al new items are parsed
			contentEditables = $('[data-miniwebprop]');
			contentHtmlEditables = $('[data-miniwebedittype=html]');

			contentEditables.attr('contentEditable', true);
			contentHtmlEditables.each(function(index) {
				var thisTools = $('#tools').clone();
				thisTools.attr('id', '').attr('data-role', 'editor-toolbar' + index).addClass('editor-toolbar');
				$(this).before(thisTools);
				$(this).wysiwyg({ hotKeys: {}, activeToolbarClass: "active", toolbarSelector: '[data-role=editor-toolbar' + index + ']', createLink: createHyperLink });
			});

			btnNew.attr("disabled", true);
			btnEdit.attr("disabled", true);
			btnSave.removeAttr("disabled");
			btnCancel.removeAttr("disabled");

			toggleContentInserts(true);
			toggleSourceView();

			$(".editor-toolbar").fadeIn().css("display", "block");
		},
		cancelEdit = function() {
			$('body').removeClass('miniweb-editing');
			contentEditables.removeAttr('contentEditable');
			contentHtmlEditables.removeAttr('contentEditable');
			btnCancel.focus();

			btnNew.removeAttr("disabled");
			btnEdit.removeAttr("disabled");
			btnSave.attr("disabled", true);
			btnCancel.attr("disabled", true);

			$(".editor-toolbar").remove();

			toggleContentInserts(false);
		},
		toggleContentInserts = function(on: boolean) {
			if (on) {
				$('[data-miniwebsection]').append('<a href="#" class="miniweb-insertcontent btn btn-info" data-toggle="modal" data-target="#newContentAdd">add content</a>');
				$('[data-miniwebsection] article .article-actions').remove();
				$('[data-miniwebsection] article').append('<div class="btn-group pull-right article-actions"><a class="btn btn-mini articleUp" ><i class="glyphicon glyphicon-arrow-up" > </i> </a><a class="btn btn-mini articleDown" > <i class="glyphicon glyphicon-arrow-down" > </i> </a>	<a class="btn btn-mini remove" title= "Remove article" > <i class="glyphicon glyphicon-remove" > </i> remove article</a>	</div>');

				$('.miniweb-insertcontent').click(function() {
					$('#newContentAdd').attr('data-targetsection', $(this).closest('[data-miniwebsection]').attr('data-miniwebsection'));
				});
				$('.btn.remove').click(function() {
					if (confirm('are you sure?')) {
						$(this).closest('article').remove();
					}
				});
				$('.btn.articleUp').click(function() {
					var cur = $(this).closest('article');
					cur.prev().before(cur);
				});
				$('.btn.articleDown').click(function() {
					var cur = $(this).closest('article');
					cur.next().after(cur);
				});
				$('.uploadimage').click(function(e) {
					e.preventDefault();
					$(this).closest('.editor-toolbar').find('.txtImage').click();
				});
			} else {
				$('.miniweb-insertcontent, .article-actions').remove();
			}
		},
		toggleSourceView = function() {
			$(".source").bind("click", function() {
				var self = $(this);
				if (self.attr("data-cmd") === "source") {
					self.attr("data-cmd", "design");
					self.addClass("active");
					//txtContent.text(txtContent.html());
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
		showMessage = function(success, message, isHtml = false) {
			var className = success ? "alert-success" : "alert-danger";
			var timeout = success ? 4000 : 8000;
			txtMessage.addClass(className);
			if (isHtml)
				txtMessage.html(message);
			else
				txtMessage.text(message);
			txtMessage.parent().fadeIn();

			setTimeout(function() {
				txtMessage.parent().fadeOut("slow", function() {
					txtMessage.removeClass(className);
				});
			}, timeout);
		},
		getParsedHtml = function(source: any) {
			var parsedDOM;
			parsedDOM = new DOMParser().parseFromString(source.cleanHtml(), 'text/html');
			parsedDOM = new XMLSerializer().serializeToString(parsedDOM);
			/<body>([\s\S]*)<\/body>/im.exec(parsedDOM);
			parsedDOM = RegExp.$1;
			return $.trim(parsedDOM);
		},
		saveContent = function(e) {
			if ($(".source").attr("data-cmd") === "design") {
				$(".source").click();
			}
			var items = [];
			$('[data-miniwebsection]').each(function(index) {
				var sectionid = $(this).attr('data-miniwebsection');
				$('article', this).each(function() {
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
					$(this).find('[data-miniwebprop]').each(function() {
						item.Values[$(this).attr('data-miniwebprop')] = getParsedHtml($(this));
					});
					items[index].Items.push(item);
				});
			});

			//console.log(JSON.stringify(items));

			$.post('/miniweb-api/savecontent', {
				url: $('#admin').attr('data-miniweb-path'),
				items: JSON.stringify(items),
				'__RequestVerificationToken': $('input[name=__RequestVerificationToken]').val()
			}).done(function(data) {
				if (data.result) {
					showMessage(true, "The page was saved successfully");
				} else {
					showMessage(false, "Save page failed");
				}
				cancelEdit();
			}).fail(function(data) {

				var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
				showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
			});

		},
		savePage = function() {
			var formArr = $(this).closest('form').serializeArray();
			formArr.push({ name: '__RequestVerificationToken', value: $('input[name=__RequestVerificationToken]').val() });
			$.post("/miniweb-api/savepage",
						formArr
				  ).done(function(data) {
					if (data && data.result) {
						document.location.href = '/' + data.url;
					} else {
						showMessage(false, data.message);
					}
				}).fail(function(data) {
					var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
					showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
				});
		},
		removePage = function() {
			if (confirm('are you sure?')) {				
				$.post("/miniweb-api/removepage", {
					'__RequestVerificationToken': $('input[name=__RequestVerificationToken]').val(),
					url: $('#admin').attr('data-miniweb-path')
				}).done(function(data) {
					showMessage(true, "The page was saved successfully");
					setTimeout(function() {
						document.location.href = '/' + data.url
					}, 1500);
				}).fail(function(data) {
					var message = data.responseText.match('\<div class=\"titleerror\"\>([^\<]+)\</div\>');
					showMessage(false, "Something bad happened. Server reported<br/>" + message[1], true);
				});
			}
		},
		addNewContent = function() {
			var htmlid = $(this).data('contentid');
			var target = $(this).closest('#newContentAdd').attr('data-targetsection');
			$('[data-miniwebsection=' + target + ']').append($('#' + htmlid).html());
			cancelEdit();
			editContent();
			$('#newContentAdd').modal('hide');
		},
		addNewPage = function() {
			//copy and empty current page property modal
			if ($('#newPageProperties').length == 0) {
				var newP = $('#pageProperties').clone();
				newP.attr('id', 'newPageProperties');
				$('input[name=OldUrl]', newP).remove();
				$('input,textarea', newP).not('[type=hidden],[type=checkbox]').val('');
				var parentUrl = $('#pageProperties input[name=Url]').val();
				$('input[name=Url]', newP).val(parentUrl.substring(0, parentUrl.lastIndexOf('/') + 1));
				$('body').append(newP);
				$('#newPageProperties .btn-primary').bind('click', savePage);
			}
			$('#newPageProperties').modal();
		};

	txtMessage = $("#admin .alert");

	btnNew = $("#btnNew");
	btnEdit = $("#btnEdit").bind("click", editContent);
	btnSave = $("#btnSave").bind("click", saveContent);
	btnCancel = $("#btnCancel").bind("click", cancelEdit);
	contentEditables = $('[data-miniwebprop]');
	contentHtmlEditables = $('[data-miniwebedittype=html]');
	$('#pageProperties .btn-primary').bind('click', savePage);
	$('#pageProperties .btn-danger').bind("click", removePage);
	$('#newContentAdd .btn-primary').bind("click", addNewContent);
	$('#newPage').bind('click', addNewPage);


	$(document).keyup(function(e) {
		if (!(<HTMLElement>document.activeElement).isContentEditable) {
			if (e.keyCode === 27) { // ESC key
				cancelEdit();
			}
		}
	});
	//always cancel edit on refresh, stops remembering of firefox for inputs and stuff
	cancelEdit();

	$('#showHiddenPages input').click(function() {
		sessionStorage.setItem('showhiddenpages', $(this).is(':checked'));
		$('.hiddenitem').toggle($(this).is(':checked'));
	});
	if (sessionStorage.getItem('showhiddenpages') === "true") {
		$('.hiddenitem').toggle(true);
		$('#showHiddenPages input').attr('checked', 'checked');
	}


})(jQuery);

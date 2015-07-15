# MiniWeb small CMS for coreclr

## What is miniweb
Firstly an easy to use Simple CMS website that just needs page templates and content item templates in form of cshtml files

Secondly it is an experiment with what .net coreclr can and can't do. I'm currently using
* mvc and webapi
* basic authentication
* custom configuration
* custom taghelpers
* custom middleware

it currently runs on ![coreclr x64](https://img.shields.io/myget/aspnetvnext/v/dnx-coreclr-win-x64.svg?style=flat).
Tested on windows, mac osx, linux (ubuntu) and windows IoT  

with some workarounds voor mac and linux 
* remove "resource" line from Core project.json otherwise it won't build
* rename /wwwroot/miniweb-resourcefallback directory to /wwwroot/miniweb-resource for edit functionality 

Based on the [MiniBlog](https://github.com/madskristensen/miniblog) package by Mats Kristensen.

It needs bootstrap v3.2 for now for the admin menu to work

## Example
Reference the MiniWeb.Core and one of the storage packages. Create an empty website. See the TestWeb project for an example implementation. Make sure the basic bootstrap files ar in the wwwroot folder.

Page templates are stored in the /Views/Pages folder

A page template example:
```HTML
@using MiniWeb
@model MiniWeb.SitePage

@{
	Layout = Model.Layout;
}
<div role="main" class="col-sm-9" miniweb-section="content">
</div>

<aside role="complementary" class="col-sm-3" miniweb-section="aside">
</aside>
```

Content items can be added to miniweb-section tags and should live in the /Views/Items folder

A content item example
```HTML
@model MiniWeb.ContentItem
<article miniweb-template="@Model.Template" >
	<h3 miniweb-prop="title"></h3>
	<div miniweb-prop="content" miniweb-edittype="html"></div>
	<div miniweb-prop="other" miniweb-edittype="html"></div>
</article>
```
every tag can have a miniweb-prop attribute that will be stored in the content item, edittype is eiter single line or specified as html

## Storage
Currently there are two storage packages
* MiniWeb.Storage.JsonStorage
* MiniWeb.Storage.XmlStorage

both store their files in the /App_Data/Sitepages folder

## TODO
* Wait for embedded file fix in linux [https://github.com/aspnet/dnx/issues/2187](https://github.com/aspnet/dnx/issues/2187)
	* temp linux workaround through static files (remove the fallback from the wwwroot/miniweb-resourcefallback directory name)
* Embedded admin view through virtualpathprovider oid
* Open ID authentication possibility
* Multiple edittypes
* Better CSS and JS hooks for editing
* Better image handling (enable picking existing images as well)
* Wait for .net core release :D



# MiniWeb small CMS for coreclr

Based on the [MiniBlog](https://github.com/madskristensen/miniblog) package by Mats Kristensen.

It needs bootstrap v3.2 for now for the admin menu to work

## What is miniweb
Firstly an easy to use Simple CMS website that just needs page templates and content item templates in form of cshtml files

Secondly it is an experiment with what .net coreclr can and can't do. I'm currently using
* mvc and webapi
* basic authentication
* custom configuration
* custom taghelpers
* custom middleware

it currently runs on 1.0.0-beta6-12216 coreclr x64.
Tested on windows, windows IoT and linux wih some workarounds

## Example
Reference the MiniWeb.Core and one of the storage packages. Create an empty website.

See the Web project for an example implementation

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
* Wait for .net core release :D
* Wait for embedded file fix in linux [https://github.com/aspnet/dnx/issues/2187](https://github.com/aspnet/dnx/issues/2187)
	* temp linux workaround through admin middleware
* Embedded admin view through virtualpathprovider oid
* Open ID authentication
* Multiple edittypes
* Better CSS and JS hooks for editing
* Better image handling (enable picking existing images as well)



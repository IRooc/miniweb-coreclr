# MiniWeb small CMS for coreclr

## What is miniweb
Firstly an easy to use Simple CMS website that just needs page templates and content item templates in form of cshtml files

Secondly it is an experiment with what .net coreclr can and can't do. I'm currently using
* mvc and webapi
* oAuth & basic authentication 
* custom configuration
* custom taghelpers
* custom middleware
* embedded Razor View

it currently runs on  1.0.0-beta7-12255 coreclr x64.
Tested on windows, mac osx, linux (ubuntu) and windows IoT  

with some workarounds voor mac and linux 
* remove "resource" line from Core project.json otherwise it won't build
* make symlink from /src/MiniWeb.Core/Resources to /test/TestWeb/wwwroot/miniweb-resource for edit functionality 

Inspired by the [MiniBlog](https://github.com/madskristensen/miniblog) package by Mats Kristensen.

It needs bootstrap v3.2 for now for the admin menu to work and a modified version of [bootstrap-wysiwyg](http://github.com/mindmup/bootstrap-wysiwyg)  

## Example
Reference the MiniWeb.Core and one of the storage packages. Create an empty website. See the TestWeb project for an example implementation. Make sure the basic bootstrap files ar in the wwwroot folder.

Page templates are stored in the /Views/Pages folder

A page template example:
```HTML
@model MiniWeb.Core.SitePage
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
@model MiniWeb.Core.ContentItem
<article miniweb-template="@Model.Template" >
	<h3 miniweb-prop="title"></h3>
	<div miniweb-prop="content" miniweb-edittype="html"></div>
	<div miniweb-prop="other" miniweb-edittype="html"></div>
</article>
```
every tag can have a miniweb-prop attribute that will be stored in the content item, edittype is eiter single line or specified as html

the minimal startup will be something like this:
```c#

public IConfiguration Configuration { get; set; }

public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
{
	// Setup configuration sources.
	var configuration = new ConfigurationBuilder(appEnv.ApplicationBasePath)
					.AddJsonFile("miniweb.json", optional: true)
					.AddJsonFile($"miniweb.{env.EnvironmentName}.json", optional: true)
					.AddEnvironmentVariables();
					
	//Remember Configuration for use in ConfigureServices
	Configuration = configuration.Build();
}

public void ConfigureServices(IServiceCollection services)
{
	// Default services used by MiniWeb
	services.AddAuthentication();
	services.AddAntiforgery();
	services.AddMvc();

	//Setup miniweb injection through one of the storage overrides
	services.AddMiniWebJsonStorage(Configuration);
}
public void Configure(IApplicationBuilder app)
{
	// Default middleware used by MiniWeb
	app.UseErrorPage();
	app.UseStaticFiles();

	//Registers the miniweb middleware and MVC Routes
	app.UseMiniWebSite(Configuration);
}
```

## Storage
Currently there are two storage packages
* MiniWeb.Storage.JsonStorage
* MiniWeb.Storage.XmlStorage

both store their files in the /App_Data/Sitepages folder

## TODO
* Wait for embedded file fix in linux [https://github.com/aspnet/dnx/issues/2187](https://github.com/aspnet/dnx/issues/2187)
	* temp linux workaround through static files (remove the fallback from the wwwroot/miniweb-resourcefallback directory name)
* Open ID authentication possibility
* Multiple edittypes
* Better image handling (enable picking existing images as well)
* Wait for .net core release :D



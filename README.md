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
* [embedded assets](http://irooc.github.io/miniweb-coreclr/embedded-assets.html) (css/js)

it currently runs on  1.0.0-rc1-final coreclr x64.
Tested on windows, mac osx, linux (ubuntu) and windows IoT 

with some workarounds voor mac and linux (used until beta7)
* remove "resource" line from Core project.json otherwise it won't build
* make symlink from /src/MiniWeb.Core/Resources to /test/TestWeb/wwwroot/miniweb-resource for edit functionality 

Inspired by the [MiniBlog](https://github.com/madskristensen/miniblog) package by Mats Kristensen.

It needs bootstrap v3.2 for now for the admin menu to work and contains a modified version of [bootstrap-wysiwyg](http://github.com/mindmup/bootstrap-wysiwyg)  

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
every tag can have a miniweb-prop attribute that will be stored in the content item, edittype is eiter single line or specified as html. 
For this to work the miniweb taghelpers need to be registered for instance in the /Views/_ViewImports.cshtml
```HTML
@addTagHelper "MiniWeb.Core.*, MiniWeb.Core"
```


the minimal startup will be something like this:
```c#
public IConfiguration Configuration { get; set; }

public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
{
	// Setup configuration sources, not needed if defaults are used
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
	services.AddAntiforgery();
	services.AddMvc();

    //registers miniweb and json storage provider
	services.AddMiniWebJsonStorage(Configuration);
}

public void Configure(IApplicationBuilder app)
{
	// Default middleware used by MiniWeb
	app.UseDeveloperExceptionPage();
	app.UseStaticFiles();

	//Registers the miniweb middleware and MVC Routes
	app.UseMiniWebSite();
}
```

## Storage
Currently there are two storage packages
* MiniWeb.Storage.JsonStorage
* MiniWeb.Storage.XmlStorage
* MiniWeb.Storage.EFStorage (using SQL not tested on *nix)

both are filesystem stores and store their files in the /App_Data/Sitepages folder

## TODO
* Multiple edittypes
* Extra Storage Provider examples (documentdb, sql)
* Better image handling (enable picking existing images as well)
* Upgrade to new bootstrap
* Setup integration with clientside packages (bower grunt and so on)
* Wait for .net core release :D



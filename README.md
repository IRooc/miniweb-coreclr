# MiniWeb small CMS for coreclr

## What is miniweb
Firstly an easy to use Simple CMS website that just needs page templates and content item templates in form of cshtml files

Secondly it is an experiment with what .net coreclr can and can't do. I'm currently using
* [mvc and webapi](http://irooc.github.io/miniweb-coreclr/mvc-api.html)
* oAuth & basic authentication 
* custom configuration
* custom taghelpers
* [embedded assets through custom middleware](http://irooc.github.io/miniweb-coreclr/embedded-assets.html)
* [embedded Razor View](http://irooc.github.io/miniweb-coreclr/embedded-razorviews.html)
* [embedded assets](http://irooc.github.io/miniweb-coreclr/embedded-assets.html) (css/js)

it currently runs on  .net core 3.1.
Tested on windows, mac osx, linux (ubuntu)

Inspired by the [MiniBlog](https://github.com/madskristensen/miniblog) package by Mats Kristensen.

It contains bootstrap v3.2 for now for the admin menu to work and contains a modified version of [bootstrap-wysiwyg](http://github.com/mindmup/bootstrap-wysiwyg)  

## Example
Reference the MiniWeb.Core and one of the storage packages. Create an empty website. See the samples/SampleWeb project for an example implementation. Make sure the basic bootstrap files are in the wwwroot folder.

Page templates are stored in the /Views/Pages folder

A page template example:
```HTML
@addTagHelper MiniWeb.Core.*, MiniWeb.Core
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
@addTagHelper MiniWeb.Core.*, MiniWeb.Core
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
@addTagHelper MiniWeb.Core.*, MiniWeb.Core
@addTagHelper MiniWeb.Core.UI.*, MiniWeb.Core.UI
```
MiniWeb.Core.UI contains the `<miniwebadmin>` taghelper so the edit UI can be shown when a user is logged in with the `MiniWeb-CmsRole` role claim


the minimal startup will be something like this:
```c#
public class Startup
{
	public IConfiguration Configuration { get; set; }
	public IWebHostEnvironment Environment { get; set; }

	public Startup(IConfiguration configuration, IWebHostEnvironment env)
	{				
		//Remember Configuration for use in ConfigureServices
		Configuration = configuration;
		Environment = env;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		// Default services used by MiniWeb
		services.AddAntiforgery();
		var builder = services.AddMvc(options =>
		{
			options.EnableEndpointRouting = false;  //for now...
		});

		//register core, storage and filestorage
		services.AddMiniWeb(Configuration, Environment)
				.AddMiniWebJsonStorage(Configuration)
				.AddMiniWebAssetFileSystemStorage(Configuration);
	}

	public void Configure(IApplicationBuilder app)
	{
		// Default middleware used by MiniWeb
		app.UseDeveloperExceptionPage();
		app.UseStaticFiles();

		//Registers the miniweb middleware and MVC Routes
		app.UseMiniWebSite();
	}
}
```

## Storage
Currently there are three storage packages
* MiniWeb.Storage.JsonStorage
* MiniWeb.Storage.XmlStorage
* MiniWeb.Storage.EFStorage (SqlServer)

The first two are filesystem stores and store their files in the /App_Data/Sitepages folder

## Asset Storage
File uploads are handled with this package:
* MiniWeb.AssetStorage.FileSystem

Default stores the files in wwwroot/images so that they are served as well, needs Write rights to do this 

## Login
If you use the JsonStorage example make sure your username password is added to the miniweb.json
```JSON
"MiniWebStorage": {
	"Users": {
		"username":"password"
	}
}
```
Other authentication mechanisms can also be used, see sampleweb for an example of Github auth.

## TODO
* Remove need for `EnableEndpointRouting=false` (issue #12)
* Remove external UI dependencies
  * Remove Bootstrap if easily possible
  * Remove JQuery dependency to ES6 scripts
* Maybe change usage files to a Razor Pages




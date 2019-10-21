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

it currently runs on  1.0.0-rc2-24008 coreclr x64.
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
@addTagHelper "MiniWeb.Core.*, MiniWeb.Core"
```


the minimal startup will be something like this:
```c#
public IConfigurationRoot Configuration { get; set; }

public Startup(IHostingEnvironment env)
{
	// Setup configuration sources, not needed if defaults are used
	var configuration = new ConfigurationBuilder()
					.SetBasePath(env.ContentRootPath)
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

//Main entrypoint
public static void Main(string[] args)
{
    
	var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseUrls("http://localhost:5001")
				.UseIISIntegration()
				.UseStartup<Startup>()
				.Build();

	host.Run();
}

```

## Storage
Currently there are three storage packages
* MiniWeb.Storage.JsonStorage
* MiniWeb.Storage.XmlStorage
* MiniWeb.Storage.EFStorage (SqlServer)

The first two are filesystem stores and store their files in the /App_Data/Sitepages folder

## Login
If you use the JsonStorage example make sure your username password is added to the miniweb.json
```JSON
	"MiniWebStorage": {
		"Users": {
			"username":"password"
		}
	}
```

## TODO
* Move JQuery dependency to ES6 scripts
* Remove Bootstrap if easily possible
* Update to .netcore 3.0 and revisit current solution.
* Add clientview to a Razor Pages package
* Better image handling (enable picking existing images as well)



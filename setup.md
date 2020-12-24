# Setup

## Commands

dotnet ef migrations add {name of migration} -p SmallLister/SmallLister.csproj --startup-project SmallLister.Web/SmallLister.Web.csproj

dotnet publish -c Release -r win-x64

## Javascript

dotnet tool install -g Microsoft.Web.LibraryManager.Cli
libman init
libman install jquery@3.5.0 --destination wwwroot/lib/jquery --files jquery.min.js
libman install jquery-sortable@0.9.13 --destination wwwroot/lib/jquery-sortable --files jquery-sortable-min.js
libman install PickMeUp@3.2.1 --destination wwwroot/lib/pickmeup --files pickmeup.min.js --files css/pickmeup.css

## References

[MS Docs](https://docs.microsoft.com/en-us/aspnet/core/client-side/libman/libman-cli?view=aspnetcore-3.1)

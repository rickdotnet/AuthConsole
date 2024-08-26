using AuthConsole;
using Microsoft.Extensions.Hosting;

Tool.RunTool();
return;

var host = Host
    .CreateApplicationBuilder(args)
    .ConfigureApplication();

host.Run();
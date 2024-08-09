using AuthConsole;
using Microsoft.Extensions.Hosting;

var host = Host
    .CreateApplicationBuilder(args)
    .ConfigureApplication();

host.Run();
using LuaLanguageServer.LanguageServer;

var server = new LanguageServer();
await server.StartAsync(args);

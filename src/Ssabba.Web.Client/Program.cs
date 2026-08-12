using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Calls go back to the host, which attaches the auth cookie — the client never holds tokens.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
// Receives the authentication state the server serialised into the page.
builder.Services.AddAuthenticationStateDeserialization();

await builder.Build().RunAsync();

﻿using api.iapetus11.me.Extensions;
using api.iapetus11.me.Models;
using api.iapetus11.me.Services.Minecraft;
using LazyCache;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using ImageExtensions = api.iapetus11.me.Extensions.ImageExtensions;

namespace api.iapetus11.me.Services;

public class MinecraftServerService : IMinecraftServerService
{
    private readonly IAppCache _cache;
    private readonly IStaticAssetsService _assets;
    private readonly ILogger<MinecraftServerService> _log;

    public MinecraftServerService(IAppCache cache, IStaticAssetsService assets, ILogger<MinecraftServerService> log)
    {
        _cache = cache;
        _assets = assets;
        _log = log;
    }
    
    private async Task<MinecraftServer> FetchServer(string address)
    {
        return await _cache.GetOrAddAsync(GetCacheKey(address), async () =>
        {
            _log.LogInformation("Start fetching server {ServerAddress}", address);

            MinecraftServer.TryParseAddress(address, out var host, out var port, out var invalidAddress);
            var server = new MinecraftServer(host, port);

            if (!invalidAddress)
            {
                try
                {
                    await server.FetchStatus();
                }
                catch (Exception e)
                {
                    _log.LogError("An error occurred while fetching the status of a server: {@Exception}", e);
                }
            }
            
            _log.LogInformation("End fetching server {ServerAddress} {@Server}", address, server);

            return server;
        }, DateTimeOffset.UtcNow.AddSeconds(30));
    }

    public async Task<MinecraftServerStatus> FetchServerStatus(string address)
    {
        return (await FetchServer(address)).Status;
    }
    
    public async Task<Stream> FetchServerImage(string address, string name)
    {
        var status = await FetchServerStatus(address);

        using (var image = new ServerImage(status, _assets).Generate(name))
        {
            return image.ToPngStream();
        }
    }

    public async Task<Stream> FetchServerFavicon(string address)
    {
        var status = await FetchServerStatus(address);

        if (status.Favicon == null) return _assets.DefaultFaviconImage.ToPngStream();

        var image = ImageExtensions.FromB64Png(status.Favicon);
        
        image.Mutate(x => x.SetGraphicsOptions(new GraphicsOptions{Antialias = true}).Resize(128, 128, new NearestNeighborResampler()));

        return image.ToPngStream();
    }

    private static string GetCacheKey(string address) => $"MinecraftServer:{address}";
}
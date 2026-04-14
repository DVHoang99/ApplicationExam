using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Common.Caching;
using ZiggyCreatures.Caching.Fusion;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace WebAppExam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CacheTestController : ControllerBase
{
    private readonly IFusionCache _fusionCache;
    private readonly ICacheService _cacheService;
    private readonly IMemoryCache _memoryCache;

    public CacheTestController(IFusionCache fusionCache, ICacheService cacheService, IMemoryCache memoryCache)
    {
        _fusionCache = fusionCache;
        _cacheService = cacheService;
        _memoryCache = memoryCache;
    }

    [HttpGet("fusion-test/{key}")]
    public async Task<IActionResult> TestFusionCache(string key)
    {
        var sw = Stopwatch.StartNew();
        
        var result = await _fusionCache.GetOrSetAsync<string>(
            key,
            async ct => 
            {
                await Task.Delay(2000); 
                return $"Data for {key} generated at {DateTime.Now}";
            },
            TimeSpan.FromMinutes(5)
        );
        
        sw.Stop();
        
        return Ok(new 
        {
            Value = result,
            ElapsedMs = sw.ElapsedMilliseconds,
            Message = sw.ElapsedMilliseconds < 100 ? "Cache Hit" : "Cache Miss"
        });
    }

    [HttpGet("service-test/{key}")]
    public async Task<IActionResult> TestCacheService(string key)
    {
        var sw = Stopwatch.StartNew();
        
        var result = await _cacheService.GetAsync<string>(
            key,
            async () => 
            {
                await Task.Delay(2000);
                return $"Service data for {key} at {DateTime.Now}";
            },
            TimeSpan.FromMinutes(5)
        );
        
        sw.Stop();
        
        return Ok(new 
        {
            Value = result,
            ElapsedMs = sw.ElapsedMilliseconds,
            Source = sw.ElapsedMilliseconds < 50 ? "L1 (Memory)" : (sw.ElapsedMilliseconds < 500 ? "L2 (Redis)" : "Database/Factory")
        });
    }

    [HttpDelete("clear/{prefix}")]
    public async Task<IActionResult> ClearCache(string prefix)
    {
        await _cacheService.RemoveByPrefixAsync(prefix);
        return Ok($"Cache with prefix '{prefix}' cleared.");
    }

    [HttpGet("dump-ram")]
    public IActionResult DumpRam()
    {
        // 1. Find the REAL memory cache being used by FusionCache
        var mcaField = _fusionCache.GetType().GetField("_mca", BindingFlags.Instance | BindingFlags.NonPublic);
        var mca = mcaField?.GetValue(_fusionCache);
        
        IMemoryCache? actualMemoryCache = null;
        if (mca != null)
        {
            var field = mca.GetType().GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            actualMemoryCache = field?.GetValue(mca) as IMemoryCache;
        }

        actualMemoryCache ??= _memoryCache;

        // 2. Extract the keys from the discovered MemoryCache
        var fieldInfo = typeof(MemoryCache).GetField("_coherentState", BindingFlags.Instance | BindingFlags.NonPublic);
        var state = fieldInfo?.GetValue(actualMemoryCache);
        
        if (state == null) return NotFound("Could not access private MemoryCache state.");

        var entriesCollection = state.GetType().GetProperty("EntriesCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        var entries = entriesCollection?.GetValue(state) as dynamic;

        var keys = new List<object>();
        if (entries != null)
        {
            foreach (var entry in entries)
            {
                var val = entry.Value.Value;
                var realValue = val?.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(val) ?? val;

                keys.Add(new { 
                    Key = entry.Key.ToString(),
                    Type = realValue?.GetType().Name,
                    Value = realValue
                });
            }
        }

        return Ok(new {
            TotalCount = keys.Count,
            L1_Instances = keys
        });
    }

    [HttpGet("test-ram-link")]
    public async Task<IActionResult> TestRamLink()
    {
        var testKey = "diagnostic_test_key";
        var testValue = "RAM_LINK_OK_" + DateTime.Now.Ticks;

        await _fusionCache.SetAsync(testKey, testValue, TimeSpan.FromMinutes(1));

        var mcaField = _fusionCache.GetType().GetField("_mca", BindingFlags.Instance | BindingFlags.NonPublic);
        var mca = mcaField?.GetValue(_fusionCache);
        
        IMemoryCache? actualMemoryCache = null;
        if (mca != null)
        {
            var field = mca.GetType().GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            actualMemoryCache = field?.GetValue(mca) as IMemoryCache;
        }

        bool found = (actualMemoryCache ?? _memoryCache).TryGetValue(testKey, out var val);

        return Ok(new {
            Success = found,
            FusionType = _fusionCache.GetType().Name,
            Message = found ? "Link Working!" : "Link still Broken.",
            ValueFound = val != null
        });
    }
}

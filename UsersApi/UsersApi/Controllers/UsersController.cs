using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UsersApi.Data;
using UsersApi.Entities;

namespace UsersApi.Controllers;

[ApiController]
[Route("")]
public class UsersController(UsersDbContext db, IMemoryCache cache) : ControllerBase
{
    private const string CacheKey = "users_all";

    // POST /create-users
    [HttpPost("create-users")]
    public async Task<ActionResult<User>> CreateOne()
    {
        var faker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Age, f => f.Random.Int(18, 80))
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.TimeStamp, _ => DateTime.UtcNow);

        var user = faker.Generate();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        cache.Remove(CacheKey); // invalidate
        return Created($"/users/{user.Id}", user);
    }

    // POST /create-bulk-users?count=10000
    [HttpPost("create-bulk-users")]
    public async Task<ActionResult> CreateBulk([FromQuery] int count = 10_000)
    {
        db.ChangeTracker.AutoDetectChangesEnabled = false;

        var faker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Age, f => f.Random.Int(18, 80))
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.TimeStamp, _ => DateTime.UtcNow);

        const int batchSize = 2_000;
        int remaining = count;

        while (remaining > 0)
        {
            var take = Math.Min(batchSize, remaining);
            var batch = faker.Generate(take);

            await db.Users.AddRangeAsync(batch);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            remaining -= take;
        }

        cache.Remove(CacheKey);
        return Ok(new { Inserted = count });
    }

    // GET /fetch-users
    [HttpGet("fetch-users")]
    public async Task<ActionResult<IEnumerable<User>>> FetchAll()
    {
        if (!cache.TryGetValue(CacheKey, out List<User>? users))
        {
            users = await db.Users.AsNoTracking().ToListAsync();
            cache.Set(CacheKey, users, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }
        return Ok(users);
    }
}
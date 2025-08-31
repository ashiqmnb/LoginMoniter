using LoginMonitering.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace LoginMonitering.Middlewares
{
    public class ExstractMetaData
    {
        private readonly RequestDelegate _next;
        public ExstractMetaData(RequestDelegate next) { _next = next; }

        public async Task InvokeAsync(HttpContext ctx, AppDbContext db)
        {

            // Extract IP - careful with proxies
            //string ip = ctx.Connection.RemoteIpAddress?.ToString() ?? ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "unknown";


            // For dev: take from header, else from connection
            string ip = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? ctx.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";

            string ua = ctx.Request.Headers["User-Agent"].FirstOrDefault() ?? "";
            string fingerprint = ctx.Request.Headers["X-Device-Fingerprint"].FirstOrDefault() ?? "";

         
            // attach results
            ctx.Items["RequestIp"] = ip;
            ctx.Items["DeviceFingerprint"] = fingerprint;
            ctx.Items["UserAgent"] = ua;

            await _next(ctx);
        }
    }
}

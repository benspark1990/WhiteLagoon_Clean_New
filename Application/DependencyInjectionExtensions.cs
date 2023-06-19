using Microsoft.Extensions.DependencyInjection;
using WhiteLagoon.Application.Services.Implementations;
using WhiteLagoon.Application.Services.Interfaces;

namespace WhiteLagoon.Application;

public static class DependencyInjectionExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IVillaService, VillaService>();
        services.AddScoped<IVillaNumberService, VillaNumberService>();
        services.AddScoped<IAmenityService, AmenityService>();
    }
}

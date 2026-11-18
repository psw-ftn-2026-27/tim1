using Exploration.Domain.Images;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationModuleInitializer : IHostedService
{
    private static readonly Guid FirstExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public ExplorationModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ExplorationDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment() && !await dbContext.Tours.AnyAsync())
        {
            var images = new List<Image>();
            dbContext.Tours.AddRange(CreateDemoTours(images));
            dbContext.Images.AddRange(images);
            await dbContext.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // The demo keypoints get image records only, without files; reading their images returns 404.
    private static Guid CreateAttachedImage(List<Image> images)
    {
        var image = new Image(FirstExplorer, 800, 600, 1000);
        image.AttachToKeypoint(FirstExplorer);
        images.Add(image);
        return image.Id;
    }

    private static List<Tour> CreateDemoTours(List<Image> images)
    {
        var fortress = new Tour(FirstExplorer, "Šetnja Petrovaradinskom tvrđavom",
            "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav i stari deo Novog Sada.",
            TourDifficulty.Easy, ["istorija", "priroda"], new GroupSize(1, 15));
        fortress.AddTransportTime(TransportMode.Walking, 120);
        fortress.AddKeypoint("Početak", "Polazna tačka ture.", CreateAttachedImage(images), null, true, new Coordinate(45.2517, 19.8647));
        fortress.AddKeypoint("Kraj", "Završna tačka ture.", CreateAttachedImage(images), null, false, new Coordinate(45.2530, 19.8610));
        fortress.SetLength(2.7);
        fortress.Publish(0);

        var fruskaGora = new Tour(FirstExplorer, "Biciklom preko Fruške gore",
            "Ruta vodi od Sremske Kamenice preko Popovice do Iriškog venca, pored manastira Beočin i vidikovaca sa kojih se vidi ceo Srem, uz nekoliko dužih uspona.",
            TourDifficulty.Hard, ["biciklizam", "planina"], new GroupSize(2, 8));
        fruskaGora.AddTransportTime(TransportMode.Bicycle, 240);
        fruskaGora.AddKeypoint("Početak", "Polazna tačka ture.", CreateAttachedImage(images), null, true, new Coordinate(45.2290, 19.8640));
        fruskaGora.AddKeypoint("Kraj", "Završna tačka ture.", CreateAttachedImage(images), null, false, new Coordinate(45.1560, 19.7220));
        fruskaGora.SetLength(2.7);
        fruskaGora.Publish(0);

        var cityCenter = new Tour(FirstExplorer, "Obilazak centra grada",
            "Kratak obilazak Trga slobode, Dunavske ulice i Dunavskog parka.",
            TourDifficulty.Moderate, ["grad"], new GroupSize(1, 20));

        var vineyards = new Tour(FirstExplorer, "Degustacija u sremskim vinogradima",
            "Tura vodi kroz vinograde Sremskih Karlovaca, uz zaustavljanje u podrumima i degustaciju lokalnih vina, a završava se na vidikovcu iznad Dunava.",
            TourDifficulty.Moderate, ["gastronomija", "priroda"], new GroupSize(2, 8));
        vineyards.AddTransportTime(TransportMode.Car, 90);
        vineyards.AddKeypoint("Početak", "Polazna tačka ture.", CreateAttachedImage(images), null, true, new Coordinate(45.2000, 20.0960));
        vineyards.AddKeypoint("Kraj", "Završna tačka ture.", CreateAttachedImage(images), null, false, new Coordinate(45.2050, 20.1000));
        vineyards.SetLength(2.7);
        vineyards.Publish(0);
        vineyards.Archive();

        return [fortress, fruskaGora, cityCenter, vineyards];
    }
}

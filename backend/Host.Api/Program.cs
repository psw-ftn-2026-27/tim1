using System.Text;
using System.Text.Json.Serialization;
using Exploration.Api;
using Exploration.Infrastructure;
using Games.Api;
using Games.Infrastructure;
using Host.Api;
using Identity.Api;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.IdentityModel.Tokens;
using Payment.Api;
using Payment.Infrastructure;
using Scalar.AspNetCore;
using Shared.Infrastructure.Images;
using Social.Api;
using Social.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// One request body limit for the whole application: the largest image file plus headroom
// for the multipart envelope. An endpoint that needs a larger body raises it on itself with
// [RequestSizeLimit]. Kestrel enforces it while reading any body, including one sent
// without a Content-Length; the middleware below answers 413 before reading a body whose
// declared length is too large. See Shared/Shared.Infrastructure/Images/README.md.
const long maxRequestBodyBytes = ImageProcessor.MaxFileBytes + 4096;
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = maxRequestBodyBytes);

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddIdentityControllers()
    .AddExplorationControllers()
    .AddGamesControllers()
    .AddSocialControllers()
    .AddPaymentControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddExplorationModule(builder.Configuration)
    .AddGamesModule(builder.Configuration)
    .AddSocialModule(builder.Configuration)
    .AddPaymentModule(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.Use(async (context, next) =>
{
    var endpointLimit = context.GetEndpoint()?.Metadata.GetMetadata<IRequestSizeLimitMetadata>();
    var limit = endpointLimit is null ? maxRequestBodyBytes : endpointLimit.MaxRequestBodySize;
    if (context.Request.ContentLength > limit)
    {
        await Results.Problem(statusCode: StatusCodes.Status413PayloadTooLarge, title: "The request is too large.")
            .ExecuteAsync(context);
        return;
    }

    await next(context);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes("Bearer"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;

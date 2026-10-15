using System.Text;
using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Auth.Commands.Login;
using HelpDesk.Application.Categories.Queries.GetCategories;
using HelpDesk.Application.Comments.Commands.AddComment;
using HelpDesk.Application.Comments.Queries.GetComments;
using HelpDesk.Application.Dashboard.Queries.GetMetrics;
using HelpDesk.Application.Dashboard.Queries.GetEmployeeDashboard;
using HelpDesk.Application.Dashboard.Queries.GetAgentDashboard;
using HelpDesk.Application.Dashboard.Queries.GetAdminDashboard;
using HelpDesk.Application.Tickets.Commands.AssignTicket;
using HelpDesk.Application.Tickets.Commands.ChangeTicketStatus;
using HelpDesk.Application.Tickets.Commands.CreateTicket;
using HelpDesk.Application.Tickets.Queries.GetTicketById;
using HelpDesk.Application.Tickets.Queries.GetTickets;
using HelpDesk.Application.Users.Queries.GetUsers;
using HelpDesk.Application.Users.Queries.GetUserById;
using HelpDesk.Application.Users.Commands.CreateUser;
using HelpDesk.Application.Users.Commands.UpdateUser;
using HelpDesk.Application.Users.Commands.DeleteUser;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Persistence.Repositories;
using HelpDesk.Infrastructure.Security;
using HelpDesk.WebAPI.Middleware;
using HelpDesk.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HelpDesk.Application.Tickets.Commands.CancelTicket;
using HelpDesk.Application.Tickets.Commands.EditTicket;



var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Database
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HelpDeskDb")));

// ---------------------------------------------------------------------------
// JWT configuration
// ---------------------------------------------------------------------------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// Repositories
// ---------------------------------------------------------------------------
builder.Services.AddScoped<ITicketRepository, EfTicketRepository>();
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
builder.Services.AddScoped<ICommentRepository, EfCommentRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();

// ---------------------------------------------------------------------------
// Security services
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// ---------------------------------------------------------------------------
// Application handlers
// ---------------------------------------------------------------------------
builder.Services.AddScoped<GetTicketsQueryHandler>();
builder.Services.AddScoped<GetTicketByIdQueryHandler>();
builder.Services.AddScoped<CreateTicketCommandHandler>();
builder.Services.AddScoped<AssignTicketCommandHandler>();
builder.Services.AddScoped<ChangeTicketStatusCommandHandler>();
builder.Services.AddScoped<GetCategoriesQueryHandler>();
builder.Services.AddScoped<GetCommentsQueryHandler>();
builder.Services.AddScoped<AddCommentCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<CancelTicketCommandHandler>();
builder.Services.AddScoped<EditTicketCommandHandler>();
builder.Services.AddScoped<GetMetricsQueryHandler>();
builder.Services.AddScoped<GetEmployeeDashboardQueryHandler>();
builder.Services.AddScoped<GetAgentDashboardQueryHandler>();
builder.Services.AddScoped<GetAdminDashboardQueryHandler>();
builder.Services.AddScoped<GetUsersQueryHandler>();
builder.Services.AddScoped<GetUserByIdQueryHandler>();
builder.Services.AddScoped<CreateUserCommandHandler>();
builder.Services.AddScoped<UpdateUserCommandHandler>();
builder.Services.AddScoped<DeleteUserCommandHandler>();

// ---------------------------------------------------------------------------
// Current user + HTTP context
// ---------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, JwtCurrentUserService>();

// ---------------------------------------------------------------------------
// ASP.NET Core plumbing
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Seed users with hashed passwords so login works.
    await Seeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher>());
}

app.UseMiddleware<DomainExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
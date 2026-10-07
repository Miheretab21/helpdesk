using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Tickets.Commands.AssignTicket;
using HelpDesk.Application.Tickets.Commands.ChangeTicketStatus;
using HelpDesk.Application.Tickets.Commands.CreateTicket;
using HelpDesk.Application.Tickets.Queries.GetTickets;
using HelpDesk.Application.Tickets.Queries.GetTicketById;
using HelpDesk.Application.Categories.Queries.GetCategories;
using HelpDesk.Application.Comments.Queries.GetComments;
using HelpDesk.Application.Comments.Commands.AddComment;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.WebAPI.Middleware;
using HelpDesk.WebAPI.Services;
using HelpDesk.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HelpDeskDb")));


builder.Services.AddScoped<ITicketRepository, EfTicketRepository>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HeaderCurrentUserService>();
builder.Services.AddScoped<GetTicketsQueryHandler>();
builder.Services.AddScoped<GetTicketByIdQueryHandler>();
builder.Services.AddScoped<CreateTicketCommandHandler>();
builder.Services.AddScoped<AssignTicketCommandHandler>();
builder.Services.AddScoped<ChangeTicketStatusCommandHandler>();
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
builder.Services.AddScoped<GetCategoriesQueryHandler>();
builder.Services.AddScoped<ICommentRepository, EfCommentRepository>();
builder.Services.AddScoped<GetCommentsQueryHandler>();
builder.Services.AddScoped<AddCommentCommandHandler>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await Seeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<DomainExceptionMiddleware>();
app.MapControllers();

app.Run();


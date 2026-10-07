using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Tickets.Commands.AssignTicket;
using HelpDesk.Application.Tickets.Commands.ChangeTicketStatus;
using HelpDesk.Application.Tickets.Commands.CreateTicket;
using HelpDesk.Application.Tickets.Queries.GetTickets;
using HelpDesk.Application.Tickets.Queries.GetTicketById;
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


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

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


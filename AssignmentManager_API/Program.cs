using FluentValidation;
using TaskManagerApi.Endpoints;
using TaskManagerApi.Repositories;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration.GetValue<string>("AllowedOrigins")!;

#region Servicios
builder.Services.AddCors(opciones =>
{
	opciones.AddDefaultPolicy(configuracion =>
	{
		configuracion.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
	});
});

builder.Services.AddOutputCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
#endregion

var app = builder.Build();

#region Middlewares
if (builder.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors();
app.UseOutputCache();
#endregion

app.MapGroup("/assignment").MapAssignments();

app.Run();

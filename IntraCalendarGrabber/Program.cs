using IntraCalendarGrabber;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<CalendarOptions>(builder.Configuration.GetSection("CalendarService"));

CalendarOptions calendarOptions = new CalendarOptions();
builder.Configuration.GetSection("CalendarService").Bind(calendarOptions);

builder.Services.AddTransient<IntraAuthenticationHandler>();
builder.Services.AddHttpClient<IIntraAuthenticationService, IntraAuthenticationService>()
    .ConfigureHttpClient(c => c.BaseAddress = calendarOptions.BaseAddress);

builder.Services.AddHttpClient<ICalendarService, CalendarService>()
    .ConfigureHttpClient(c => c.BaseAddress = calendarOptions.BaseAddress)
    .AddHttpMessageHandler<IntraAuthenticationHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.MapControllers();

app.Run();
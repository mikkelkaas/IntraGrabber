using IntraGrabber;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

builder.Services.Configure<IntraGrabberOptions>(builder.Configuration.GetSection("IntraGrabber"));

IntraGrabberOptions intraGrabberOptions = new IntraGrabberOptions();
builder.Configuration.GetSection("IntraGrabber").Bind(intraGrabberOptions);

builder.Services.AddTransient<IntraAuthenticationHandler>();
builder.Services.AddScoped<ICalendarService, CalendarService>();
builder.Services.AddScoped<IWeekPlansService, WeekPlansService>();

builder.Services.AddHttpClient<IIntraAuthenticationService, IntraAuthenticationService>()
    .ConfigureHttpClient(c => c.BaseAddress = intraGrabberOptions.BaseAddress);

builder.Services.AddHttpClient("IntraGrabber")
    .ConfigureHttpClient(c => c.BaseAddress = intraGrabberOptions.BaseAddress)
    .AddHttpMessageHandler<IntraAuthenticationHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseResponseCaching();

app.UseHttpsRedirection();


app.MapControllers();

app.Run();
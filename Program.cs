using Amazon.Runtime;
using Amazon.S3;
using AnimalDrawing.Models;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var awsOption = builder.Configuration.GetAWSOptions("AmazonS3Configuration");
awsOption.Credentials = new BasicAWSCredentials(builder.Configuration["AmazonS3Configuration:Accesskey"], builder.Configuration["AmazonS3Configuration:SecretKey"]);
builder.Services.AddDefaultAWSOptions(awsOption);
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IAWSS3BucketHelper, AWSS3BucketHelper>();
builder.Services.Configure<S3Config>(builder.Configuration.GetSection("AmazonS3Configuration"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Upload}/{action=Index}/{id?}");

app.Run();


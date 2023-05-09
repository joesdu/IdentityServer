using IdentityServer.Hosting.DependencyInjection;
using Idp.IdentityServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//ʹ�����õ�cookie��ΪĬ�ϵ���ѯ����
builder.Services.AddAuthentication(configureOptions =>
    {
        configureOptions.DefaultChallengeScheme = "Cookie";
    })
    .AddCookie("Cookie");
//������Ȩ����
builder.Services.AddAuthorization(configure =>
{
    configure.AddPolicy("default", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});
//ע��������֤Idp
builder.Services.AddIdentityServer()
    .AddInMemoryClients(IdpResource.Clients);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseIdentityServer();

//����������֤
app.UseAuthentication();
//������Ȩ
app.UseAuthorization();

app.MapDefaultControllerRoute()
    //ȫ��������Ȩ
    .RequireAuthorization("default");

app.Run();
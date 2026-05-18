namespace HCRS.RestApi.Extensions
{
    public static class SwaggerExtension
    {
        public static void ConfigureSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new()
                {
                    Title = "HCRS RestApi",
                    Version = "v1",
                    Description = "High-Concurrency Reservation System API for real-time bookings"
                });
            });
        }
    }
}

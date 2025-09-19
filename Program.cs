using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// SQL Server connection string
string connectionString = @"Data Source=DESKTOP-8RH76VU\SQLEXPRESS;Database=API;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;Packet Size=4096;Command Timeout=0";

// POST /insert endpoint
app.MapPost("/insert", async (UserDetails user) =>
{
    try
    {
        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        // Duplicate check: phone or PAN
        string checkQuery = "SELECT COUNT(*) FROM UserDetails WHERE phone = @phone OR pan = @pan";
        using var checkCmd = new SqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@phone", user.Phone);
        checkCmd.Parameters.AddWithValue("@pan", user.PAN);

        int exists = (int?)(await checkCmd.ExecuteScalarAsync()) ?? 0;

        if (exists > 0)
            return Results.Conflict(new { message = "Duplicate entry found for phone or PAN" });

        // Insert user data (consent will automatically store only date)
        string insertQuery = @"
            INSERT INTO UserDetails 
            (name, phone, city, dob, email, employment, gender, income, pan, pincode, state, consent)
            VALUES (@name, @phone, @city, @dob, @email, @employment, @gender, @income, @pan, @pincode, @state, CAST(GETDATE() AS DATE))";

        using var insertCmd = new SqlCommand(insertQuery, conn);
        insertCmd.Parameters.AddWithValue("@name", user.Name);
        insertCmd.Parameters.AddWithValue("@phone", user.Phone);
        insertCmd.Parameters.AddWithValue("@city", user.City);
        insertCmd.Parameters.AddWithValue("@dob", user.DOB);
        insertCmd.Parameters.AddWithValue("@email", user.Email);
        insertCmd.Parameters.AddWithValue("@employment", user.Employment);
        insertCmd.Parameters.AddWithValue("@gender", user.Gender);
        insertCmd.Parameters.AddWithValue("@income", user.Income);
        insertCmd.Parameters.AddWithValue("@pan", user.PAN);
        insertCmd.Parameters.AddWithValue("@pincode", user.Pincode);
        insertCmd.Parameters.AddWithValue("@state", user.State);

        int rows = await insertCmd.ExecuteNonQueryAsync();

        return rows > 0
            ? Results.Ok(new
            {
                message = "User inserted successfully.",
                createdAt = DateTime.Now.ToString("yyyy-MM-dd") // only date in response
            })
            : Results.Problem("Failed to insert user.");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

// UserDetails model
public class UserDetails
{
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required string City { get; set; }
    public required string DOB { get; set; }
    public required string Email { get; set; }
    public required string Employment { get; set; }
    public required string Gender { get; set; }
    public required string Income { get; set; }
    public required string PAN { get; set; }
    public required string Pincode { get; set; }
    public required string State { get; set; }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Searchlight.Query;
using Searchlight.Tests.Models;
using Testcontainers.MsSql;

namespace Searchlight.Tests.Executors;

[TestClass]
public class SqlServerExecutorTests
{
    private DataSource _src;
    private string _connectionString;
    private Func<SyntaxTree, Task<FetchResult<EmployeeObj>>> _sqlServer;
    private List<EmployeeObj> _list;
    private MsSqlContainer _container;

    private const string CreateSql =
        "CREATE TABLE employeeobj (name nvarchar(255) null, id int not null, hired datetime not null, paycheck decimal not null, onduty bit not null, employeetype int null DEFAULT 0, dims nvarchar(max) null)";

    private const string InsertSql =
        "INSERT INTO employeeobj (name, id, hired, paycheck, onduty, employeetype, dims) VALUES (@name, @id, @hired, @paycheck, @onduty, @employeetype, @dims)";

    [TestInitialize]
    public async Task SetupClient()
    {
        _src = DataSource.Create(null, typeof(EmployeeObj), AttributeMode.Strict);
        _container = new MsSqlBuilder().Build();
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();

        // Construct the database schema and insert some test data
        await using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Create basic table
            await using (var command = new SqlCommand(CreateSql, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            // Insert rows
            foreach (var record in EmployeeObj.GetTestList())
            {
                await using var command = new SqlCommand(InsertSql, connection);
                var json = JsonConvert.SerializeObject(record.dims);
                command.Parameters.AddWithValue("@name", (object)record.name ?? DBNull.Value);
                command.Parameters.AddWithValue("@id", record.id);
                command.Parameters.AddWithValue("@hired", record.hired);
                command.Parameters.AddWithValue("@paycheck", record.paycheck);
                command.Parameters.AddWithValue("@onduty", record.onduty);
                command.Parameters.AddWithValue("@employeetype", record.employeeType);
                command.Parameters.AddWithValue("@dims", json == "null" ? DBNull.Value : json);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Keep track of the correct result expectations and execution process
        _list = EmployeeObj.GetTestList();
        _sqlServer = async syntax =>
        {
            var sql = syntax.ToSqlServerCommand();
            var result = new List<EmployeeObj>();
            var numResults = 0;
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new SqlCommand(sql.CommandText, connection))
                {
                    foreach (var p in sql.Parameters)
                    {
                        var type = sql.ParameterTypes[p.Key];
                        command.Parameters.AddWithValue(p.Key,
                            type == typeof(DateTime) ? ((DateTime)p.Value).ToUniversalTime() : p.Value);
                    }

                    try
                    {
                        var reader = await command.ExecuteReaderAsync();
                        await reader.ReadAsync();
                        numResults = reader.GetInt32(0);

                        // Skip ahead to next result set
                        await reader.NextResultAsync();
                        while (await reader.ReadAsync())
                        {
                            result.Add(new EmployeeObj()
                            {
                                name = reader.IsDBNull(0) ? null : reader.GetString("name"),
                                id = reader.GetInt32("id"),
                                hired = reader.GetDateTime("hired"),
                                paycheck = reader.GetDecimal("paycheck"),
                                onduty = reader.GetBoolean("onduty"),
                                employeeType = (EmployeeObj.EmployeeType)reader.GetInt32(5),
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail(ex.ToString());
                    }
                }
            }

            // TODO: Would this be better if we used dapper?
            return new FetchResult<EmployeeObj>()
            {
                totalCount = numResults,
                records = result.ToArray(),
            };
        };
    }

    private SqlDbType ConvertTsqlType(Type parameterType)
    {
        if (parameterType == typeof(bool))
        {
            return SqlDbType.Bit;
        }

        if (parameterType == typeof(string))
        {
            return SqlDbType.NVarChar;
        }

        if (parameterType == typeof(int))
        {
            return SqlDbType.Int;
        }

        if (parameterType == typeof(decimal))
        {
            return SqlDbType.Decimal;
        }

        if (parameterType == typeof(DateTime))
        {
            return SqlDbType.DateTime;
        }

        throw new Exception("Not recognized type");
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task EmployeeTestSuite()
    {
        await Executors.EmployeeTestSuite.BasicTestSuite(_src, _list, _sqlServer);
    }

    [TestMethod]
    public async Task JsonColumn_Filter()
    {
        var syntax = _src.ParseFilter("dims.\"test\" eq 'value'");
        var results = await _sqlServer(syntax);

        Assert.AreEqual(2, results.records.Length);
    }

    [TestMethod]
    public async Task JsonColumn_Sort()
    {
        var syntax = _src.ParseFilter("dims.\"test\".\"inner\" IS NOT NULL OR dims.\"test\" eq 'value'");
        syntax.OrderBy = _src.ParseOrderBy("dims.\"test\".\"inner\"");
        var results = await _sqlServer(syntax);

        Assert.AreEqual(4, results.records.Length);
    }

    [TestMethod]
    public async Task JsonColumn_Nested()
    {
        var syntax = _src.ParseFilter("dims.\"test\".\"inner\" eq 'value'");
        var results = await _sqlServer(syntax);

        Assert.AreEqual(1, results.records.Length);
    }

    [TestMethod]
    public async Task JsonColumn_Parsing()
    {
        var syntax = _src.ParseFilter("dims.\"=<>\\\"\" eq 'value'");
        var results = await _sqlServer(syntax);

        Assert.AreEqual(0, results.records.Length);
    }
}
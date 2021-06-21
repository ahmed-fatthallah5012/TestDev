using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TestDev.Data
{
    public class ControlData
    {
        private readonly IConfiguration _configuration;

        public ControlData(IConfiguration configuration) 
            => _configuration = configuration;

        public async Task<IEnumerable<Accounts>> FillData()
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:Default"]);
            const string query = @"SELECT [Acc_Number] , [ACC_Parent] , [Balance] FROM [TestDev].[dbo].[Accounts]";
            var command = new SqlCommand(query, connection);
            try
            {
                command.CommandType = CommandType.Text;
                await connection.OpenAsync();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.TableMappings.Add("Table", "Accounts");
                adapter.SelectCommand = command;
                var dataSet = new DataSet("Accounts");
                adapter.Fill(dataSet);
                DataTable accounts = dataSet.Tables["Accounts"];
                var result = accounts?.AsEnumerable().Select(p => new Accounts
                {
                    Id = p.Field<string>("Acc_Number"),
                    Parent = p.Field<string>("ACC_Parent"),
                    Balance = p.Field<decimal?>("Balance")
                }).ToList();

                var res = result?.Where(a => a.Parent == null)
                    .Select(a => new Accounts
                    {
                        Id = a.Id,
                        Parent = a.Parent,
                        Balance = a.Balance,
                        Children = GetChildren(result , a.Id)
                    });
                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private List<Accounts> GetChildren(IEnumerable<Accounts> accounts, string parentId)
        {
            return accounts.Where(a => a.Parent == parentId).Select(a => new Accounts
            {
                Id = a.Id,
                Parent = a.Parent,
                Balance = a.Balance,
                Children = GetChildren(accounts , a.Id)
            }).ToList();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestDev.Data
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ControlData _control;

        public AccountController(ControlData control) => _control = control;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var id = Request.Query["id"];
                return await Get(id);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        public async Task<IActionResult> Get(string id)
        {
            var items = await _control.FillData();
            var parentAccount = (items).FirstOrDefault(a => a.Id.Contains(id) );

            var stringBuilder = new StringBuilder();
            if (parentAccount == null) return BadRequest("There is an error While Retrieve Data");
            foreach (var itemChild in parentAccount.Children)
            {
                var itemLine = "Account ";
                var accountSum = parentAccount.Balance??0;
                itemLine += parentAccount.Id.Trim();
                itemLine += $"-{itemChild.Id.Trim()}";
                accountSum += itemChild.Balance??0;
                (itemLine, accountSum) = RetrieveTotal(itemChild , itemLine , accountSum);
                itemLine += $" = {Math.Round(accountSum,2)}";
                stringBuilder.AppendLine(itemLine);
            }

            return Ok(stringBuilder.ToString());
        }

        private (string, decimal) RetrieveTotal(Accounts account ,string itemLine , decimal accountSum)
        {
            if (!account.Children.Any()) return (itemLine, accountSum);
            foreach (var item in account.Children)
            {
                itemLine += $"-{item.Id.Trim()}";
                accountSum += item.Balance??0;
                (itemLine, accountSum) = RetrieveTotal(item , itemLine , accountSum);
            }
            return (itemLine, accountSum);
        }
        
    }
}

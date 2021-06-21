using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TestDev.Data;

namespace TestDev.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ControlData _data;

        public IndexModel(ControlData data) => _data = data;

        public List<Accounts> Accounts { get; set; }

        public async Task OnGetAsync()
        {
            Accounts = (await _data.FillData())
                .Select(a=> new Accounts
                {
                    Id = a.Id,
                    Parent = a.Parent,
                    Balance = a.Balance,
                    Total = RetrieveTotal(a, a.Balance),
                    Children = a.Children
                }).ToList();
            
        }

        private decimal RetrieveTotal(Accounts account ,decimal? total)
        {
            total ??= 0;
            if (!account.Children.Any()) return total.Value;
            foreach (var item in account.Children)
            {
                total += item.Balance ?? 0;
                total = RetrieveTotal(item , total);
            }
            return total.Value;
        }
    }
}

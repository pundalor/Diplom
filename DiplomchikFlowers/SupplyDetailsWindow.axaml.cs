using Avalonia.Controls;
using DiplomchikFlowers.Model;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DiplomchikFlowers
{
    public partial class SupplyDetailsWindow : Window
    {
        public SupplyDetailsWindow()
        {
            InitializeComponent();
        }

        public SupplyDetailsWindow(Supply supply)
        {
            InitializeComponent();

            this.Opened += async (_, __) => await LoadSupply(supply.Id);
        }

        private async Task LoadSupply(int supplyId)
        {
            using var ctx = new DemoContext();

            var fullSupply = await ctx.Supplies
                .Include(s => s.Supplier)
                .Include(s => s.SupplyItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == supplyId);

            DataContext = fullSupply;
        }
    }
}
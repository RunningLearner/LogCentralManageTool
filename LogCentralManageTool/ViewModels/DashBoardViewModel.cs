using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;
using LogCentralManageTool.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LogCentralManageTool.ViewModels
{
    public class DashBoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Log SelectedLog { get; set; }

        public DashBoardViewModel(ProductInfo product, ProviderType provider)
        {
            try
            {
                using var context = DbContextFactory.GetContext(product.DatabaseName, provider, product.ConnectionString);
                SelectedLog = context.Set<Log>().OrderByDescending(l => l.Timestamp).FirstOrDefault();
            }
            catch (Exception e)
            { }
        }

        protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    }
}

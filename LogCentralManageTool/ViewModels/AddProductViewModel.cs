using LogCentralManageTool.Data;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LogCentralManageTool.ViewModels;

/// <summary>
/// 제품 추가 대화상자에서 사용되는 ViewModel입니다.
/// </summary>
public class AddProductViewModel : INotifyPropertyChanged
{
    private string _databaseName;
    private string _connectionString;
    private ProviderType _selectedProviderType;

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 데이터베이스 이름 (기본값 "NewProductDb")
    /// </summary>
    public string DatabaseName
    {
        get => _databaseName;
        set { _databaseName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 연결 문자열 (기본값 "mongodb://localhost:27017/NewProductDb")
    /// </summary>
    public string ConnectionString
    {
        get => _connectionString;
        set { _connectionString = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 선택된 ProviderType
    /// </summary>
    public ProviderType SelectedProviderType
    {
        get => _selectedProviderType;
        set
        {
            if (_selectedProviderType != value)
            {
                _selectedProviderType = value;
                OnPropertyChanged();
                UpdateDefaultConnectionString();
            }
        }
    }

    /// <summary>
    /// 선택 가능한 ProviderType 목록
    /// </summary>
    public ObservableCollection<ProviderType> ProviderTypes { get; set; }

    /// <summary>
    /// ProviderType과 DatabaseName에 따라 기본 연결 문자열을 업데이트합니다.
    /// </summary>
    private void UpdateDefaultConnectionString()
    {
        switch (SelectedProviderType)
        {
            case ProviderType.MongoDB:
                ConnectionString = $"mongodb://localhost:27017/{DatabaseName}";
                break;
            case ProviderType.MySQL:
                ConnectionString = $"server=localhost;port=3306;database={DatabaseName};user=root;password=yourpassword";
                break;
            case ProviderType.SQLite:
                ConnectionString = $"Data Source={DatabaseName}.sqlite";
                break;
            default:
                ConnectionString = $"mongodb://localhost:27017/{DatabaseName}";
                break;
        }
    }

    public AddProductViewModel()
    {
        DatabaseName = "NewProductDb";
        ConnectionString = "mongodb://localhost:27017/NewProductDb";
        ProviderTypes = new ObservableCollection<ProviderType>
            {
                ProviderType.MongoDB,
                ProviderType.MySQL,
                ProviderType.SQLite
            };
        SelectedProviderType = ProviderType.MongoDB;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

namespace Gamma.Interfaces
{
    /// <summary>
    /// Интерфейс проверки на возможность редактирования
    /// </summary>
    interface ICheckedAccess
    {
        bool IsReadOnly { get;}
    }
}

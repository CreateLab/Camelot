using System.Threading.Tasks;
using Camelot.ViewModels.Services;

namespace Camelot.ViewModels.Implementations.Dialogs
{
    public abstract class ParameterizedDialogViewModelBase<TResult, TParameter> : DialogViewModelBase<TResult>
        where TResult : DialogResultBase
        where TParameter : NavigationParameterBase
    {
        public abstract Task ActivateAsync(TParameter parameter);
    }

    public abstract class ParameterizedDialogViewModelBase<TParameter> : ParameterizedDialogViewModelBase<DialogResultBase, TParameter>
        where TParameter : NavigationParameterBase
    {

    }
}
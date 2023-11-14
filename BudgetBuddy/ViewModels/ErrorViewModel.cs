#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

using Microsoft.AspNetCore.Diagnostics;
namespace BudgetBuddy.ViewModels
{
    internal class ErrorViewModel
    {
        public string RequestId { get; set; }

        public IExceptionHandlerPathFeature ExceptionInfo { get; set; }
    }
}
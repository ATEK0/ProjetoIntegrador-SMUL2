using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Portal
{
    public static class ToastMessages
    {
        public static void SetErrors(Controller controller)
        {
            var errors = GetModelStateMessages(controller);
            if (errors.Count == 0) return;
            if (errors.Count == 1)
                controller.TempData["Error"] = errors[0];
            else
                controller.TempData["ToastErrors"] = JsonSerializer.Serialize(errors);
        }

        public static List<string> GetModelStateMessages(Controller controller)
        {
            return controller.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct()
                .ToList();
        }
    }
}

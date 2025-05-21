using System;
using System.Globalization;
using System.Web.Mvc;

namespace Web.App_Start
{
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            
            if (valueResult == null)
                return null;

            var modelState = new ModelState { Value = valueResult };
            object actualValue = null;
            
            try
            {
                string attemptedValue = valueResult.AttemptedValue;
                
                // Handle different culture formats (comma or period as decimal separator)
                if (String.IsNullOrEmpty(attemptedValue))
                    return null;

                // Replace comma with period if needed
                attemptedValue = attemptedValue.Replace(',', '.');
                
                // Use invariant culture to parse
                actualValue = Convert.ToDecimal(attemptedValue, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                modelState.Errors.Add(e);
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
            return actualValue;
        }
    }
} 
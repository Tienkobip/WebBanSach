using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace WebApp.Admin.Validation
{
    public class FluentValidator : ComponentBase, IDisposable
    {
        private EditContext? _previousEditContext;
        private ValidationMessageStore? _messageStore;

        [Inject]
        private IServiceProvider ServiceProvider { get; set; } = default!;

        [CascadingParameter]
        private EditContext? CurrentEditContext { get; set; }

        protected override void OnParametersSet()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(FluentValidator)} bắt buộc phải nằm bên trong một <EditForm>.");
            }

            // Tự động gỡ và đăng ký lại sự kiện khi EditContext thay đổi
            if (CurrentEditContext != _previousEditContext)
            {
                DetachEvents();

                _previousEditContext = CurrentEditContext;
                _messageStore = new ValidationMessageStore(CurrentEditContext);

                CurrentEditContext.OnValidationRequested += HandleValidationRequested;
                CurrentEditContext.OnFieldChanged += HandleFieldChanged;
            }
        }

        /// <summary>
        /// Xử lý khi người dùng gõ xong 1 ô input và rời con trỏ chuột
        /// </summary>
        private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            _messageStore?.Clear(e.FieldIdentifier);
            ValidateField(CurrentEditContext!, e.FieldIdentifier);
        }

        /// <summary>
        /// Xử lý khi người dùng bấm Submit Form (Validate toàn bộ Model)
        /// </summary>
        private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStore?.Clear();
            ValidateModel(CurrentEditContext!);
        }

        private void ValidateModel(EditContext editContext)
        {
            var validator = GetValidatorForModel(editContext.Model);
            if (validator == null) return;

            var context = new ValidationContext<object>(editContext.Model);
            var failureResults = validator.Validate(context);

            foreach (var error in failureResults.Errors)
            {
                var fieldIdentifier = new FieldIdentifier(editContext.Model, error.PropertyName);
                _messageStore?.Add(fieldIdentifier, error.ErrorMessage);
            }

            editContext.NotifyValidationStateChanged();
        }

        private void ValidateField(EditContext editContext, FieldIdentifier fieldIdentifier)
        {
            var validator = GetValidatorForModel(fieldIdentifier.Model);
            if (validator == null) return;

            var context = ValidationContext<object>.CreateWithOptions(
                fieldIdentifier.Model,
                options => options.IncludeProperties(fieldIdentifier.FieldName));

            var failureResults = validator.Validate(context);

            foreach (var error in failureResults.Errors)
            {
                _messageStore?.Add(fieldIdentifier, error.ErrorMessage);
            }

            editContext.NotifyValidationStateChanged();
        }

        private IValidator? GetValidatorForModel(object model)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
            return ServiceProvider.GetService(validatorType) as IValidator;
        }

        private void DetachEvents()
        {
            if (_previousEditContext != null)
            {
                _previousEditContext.OnValidationRequested -= HandleValidationRequested;
                _previousEditContext.OnFieldChanged -= HandleFieldChanged;
            }
        }

        public void Dispose()
        {
            DetachEvents();
        }
    }
}

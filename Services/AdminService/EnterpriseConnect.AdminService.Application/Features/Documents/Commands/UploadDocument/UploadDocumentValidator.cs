using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".csv" };

    public UploadDocumentValidator()
    {
        RuleFor(x => x.UploaderId).NotEmpty();
        RuleFor(x => x.Version).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Version));

        RuleFor(x => x)
            .Must(x => x.FileStream != null || !string.IsNullOrWhiteSpace(x.FileUrl))
            .WithMessage("Phải cung cấp tệp đính kèm (File) hoặc đường dẫn tệp (FileUrl).");

        RuleFor(x => x.FileName)
            .Must(name => string.IsNullOrEmpty(name) || AllowedExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
            .WithMessage("Định dạng tệp không được hỗ trợ. Chỉ cho phép các định dạng: .pdf, .doc, .docx, .xls, .xlsx, .txt, .csv.");
    }
}

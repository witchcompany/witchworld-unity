using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.Tool;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Editor.Validation
{
    public static class ItemBuildValidator
    {
        private const string FileSizeErrorMsg = "파일 크기 : {0}/{1} KB\n최대 용량을 초과했습니다. 다시 시도해주세요.";
        private const string ScriptErrorMsg = "등록할 상품 Prefab의 최상단 오브젝트에 WitchGearItem 컴포넌트가 있어야 합니다.";
        private const string NullSalesItemIdErrorMsg = "Sales Item Id를 설정해주세요";
        
        public static ValidationReport ValidationCheck()
        {
            return new ValidationReport()
                .Append(ValidationFileSize())
                .Append(ValidationGearScript())
                .Append(ValidationSalesItemId());
        }

        private static ValidationReport ValidationFileSize()
        {
            var report = new ValidationReport();

            var fileSize = AssetTool.GetFileSizeByte(ExportBundleConfig.PrefabPath);
            
            if (fileSize > ExportBundleConfig.MaxProductSizeKb * 1024)
            {
                var error = new ValidationError(string.Format(FileSizeErrorMsg, fileSize,
                    ExportBundleConfig.MaxProductSizeKb), ValidationTag.TagProduct, ExportBundleConfig.Prefab);

                report.Append(error);
            }
            return report;
        }

        private static ValidationReport ValidationGearScript()
        {
            var report = new ValidationReport();
            if (ExportBundleConfig.Prefab.TryGetComponent(out WitchGearItem gear))
            {
                return gear.ValidationReport();
            }
            else
            {
                var error = new ValidationError(ScriptErrorMsg, ValidationTag.TagProduct, ExportBundleConfig.Prefab);
                report.Append(error);
            }
            return report;
        }

        private static ValidationReport ValidationSalesItemId()
        {
            var report = new ValidationReport();
            if (UploadBundleConfig.SalesItemId == 0)
            {
                var error = new ValidationError(NullSalesItemIdErrorMsg, ValidationTag.TagProduct);
                report.Append(error);
            }

            return report;
        }
    }
}
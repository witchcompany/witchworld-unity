namespace WitchCompany.Toolkit.Editor.DataStructure
{
    /// <summary>컨트롤 패널 타입</summary>
    public enum ControlPanelType
    {
        // 꺼짐
        Disabled = -1,
        // 인증 패널
        Auth = 0,
        // 유효성 검증 패널
        Validation = 1,
        // 빌드 패널
        Publish = 2,
        // // 어드민 패널
        // Admin = 3,
        // 상품 패널
        //ExportBundle = 3,
        // 상품 생성 및 업로드 패널
        ExportAndUploadBundle = 3,
        // 설정 패널
        Setting = 4
    }
}
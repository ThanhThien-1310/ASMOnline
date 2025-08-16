public interface ISettingsSection
{
    // Đổ dữ liệu đã lưu vào UI (gọi khi mở Settings hoặc khi Cancel)
    void LoadFromPrefs();

    // Áp dụng thay đổi hiện tại + lưu PlayerPrefs (gọi khi bấm Apply)
    void ApplyAndSave();
}
